using AndroidX.Core.View;
using Budget_Buddy.Models;
using Bumptech.Glide.Load;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Budget_Buddy;

public partial class CalendarPage : ContentPage
{
	public CalendarPage()
	{
		InitializeComponent();
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (CalendarGrid.RowDefinitions.Count > 4)
        {
            int lastRowIndex = CalendarGrid.RowDefinitions.Count - 1;
            for (int i = lastRowIndex; i >= 4; i--)
            {
                CalendarGrid.RowDefinitions.RemoveAt(lastRowIndex);
            }

        }

        int dayOfWeek = Convert.ToInt32(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        month_Label.Text = DateTime.Now.ToString("MMMM");
        month_Label.HorizontalTextAlignment = TextAlignment.Center;



        if (dayOfWeek + daysInMonth > 28)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
        }

        if (dayOfWeek + daysInMonth > 35)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
        }


        GenerateCalendarDays();
    }

    private async Task Calendar_Item_Clicked(object sender, EventArgs e)
    {
        if (sender != null)
        {



            Grid grid = sender as Grid;
            CalendarDay calendarDay = (CalendarDay)grid.BindingContext;



            await Navigation.PushAsync(new CalendarDayEntryPage(calendarDay));
        }

    }

    private async void GenerateCalendarDays()
    {
        int dayOfWeek = Convert.ToInt32(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        var billsByDate = Bill.AllBills
            .GroupBy(b => b.DueDay)
            .ToDictionary(g => g.Key, g=> g.ToList());
        Console.WriteLine("Bills organized. Count = " + billsByDate.Count());


        await PopulateIncomeList();
        Console.WriteLine(Income.AllIncomes.Count());

        var incomesByDate = Income.AllIncomes
            .GroupBy(b => Convert.ToDateTime(b.PayDate))
            .ToDictionary(g => g.Key, g => g.ToList());

        for (int j = dayOfWeek; j < daysInMonth + dayOfWeek; j++)
        {
            Grid grid = new Grid()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(30) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
                ColumnDefinitions =
                {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }

                },

                //Assign StyleId so object can be addressed later
                StyleId = (j - dayOfWeek + 1).ToString(),

            };

            grid.BindingContext = new CalendarDay(new DateTime(DateTime.Now.Year, DateTime.Now.Month, j-dayOfWeek+1));

            CalendarDay calendarDay = (CalendarDay)grid.BindingContext;

            //Assign bills to each day
            if (billsByDate.TryGetValue(calendarDay.Date.Day, out var bills))
            {
                foreach (var bill in bills)
                    calendarDay.Bills.Add(bill);
            }

            if (incomesByDate.TryGetValue(calendarDay.Date, out var incomes))
            {
                foreach (var income in incomes)
                    calendarDay.Incomes.Add(income);
            }

            //Assign background color to light blue for current day
            if (grid.StyleId == DateTime.Now.Day.ToString())
            {
                grid.BackgroundColor = Colors.LightBlue;
            }
            else
            {
                grid.BackgroundColor = Colors.WhiteSmoke;
            }

            

            //Assign Label with the day of the month
            Label label = new Label
            {
                TextColor = Colors.Black,
                Text = $" {(j - dayOfWeek + 1).ToString()}",
                FontAttributes = FontAttributes.Bold
            };

            

            if(calendarDay.Bills.Count > 0)
            {
                string billString = "bill";
                if (calendarDay.Bills.Count > 1)
                {
                    billString = "bills";
                }
                Label label2 = new Label
                {
                    TextColor = Colors.Black,
                    FontSize = 11,
                    Text = $"{calendarDay.Bills.Count} {billString}",
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(1, 0, 0, 0)
                };
                grid.Add(label2, 0, 1);
            }

            if (calendarDay.Incomes.Count > 0)
            {
                string incomeString = "income";
                if (calendarDay.Bills.Count > 1)
                {
                    incomeString = "incomes";
                }
                Label label2 = new Label
                {
                    TextColor = Colors.Black,
                    FontSize = 11,
                    Text = $"{calendarDay.Incomes.Count} {incomeString}",
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(1,0,0,0)
                };
                grid.Add(label2, 0, 2);
            }

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                Calendar_Item_Clicked(s, e);
            };
            grid.GestureRecognizers.Add(tapGesture);
            grid.Add(label, 0, 0);
            CalendarGrid.Add(grid, j % 7, (int)Math.Floor((double)j / 7));

        }
    }

    private async Task PopulateIncomeList()
    {
        Income.AllIncomes = await DBHandler.GetAllIncomes(Dashboard.UserID);
    }
}