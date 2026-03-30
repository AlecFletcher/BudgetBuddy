using Budget_Buddy.Models;
using Bumptech.Glide.Load;
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
        int dayOfWeek = Convert.ToInt32(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        month_Label.Text = DateTime.Now.ToString("MMMM");
        month_Label.HorizontalTextAlignment = TextAlignment.Center;



        if (dayOfWeek + daysInMonth > 28)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
        }

        if (dayOfWeek + daysInMonth > 35)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
        }


        GenerateCalendarDays();
    }

    private void Calendar_Item_Clicked(object sender, EventArgs e)
    {
        if (sender != null)
        {
            Grid grid = sender as Grid;
            Console.WriteLine(grid.BindingContext.ToString());
        }

    }

    private void GenerateCalendarDays()
    {
        int dayOfWeek = Convert.ToInt32(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek);
        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        var billsByDate = Bill.AllBills
            .GroupBy(b => b.DueDay)
            .ToDictionary(g => g.Key, g=> g.ToList());

        foreach(var cal in billsByDate)
        {
            Console.WriteLine("Key: " + cal.Key.ToString());
            foreach(Bill bill in cal.Value)
            {
                Console.WriteLine("Value: " + bill.Name);
            }
            
        }

        for (int j = dayOfWeek; j < daysInMonth + dayOfWeek; j++)
        {
            Grid grid = new Grid()
            {
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(30) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }
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

            if (billsByDate.TryGetValue(calendarDay.Date.Day, out var bills))
            {
                foreach (var bill in bills)
                    calendarDay.Bills.Add(bill);
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
                Text = $" {(j - dayOfWeek + 1).ToString()}"
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
                    FontSize = 12,
                    Text = $"{calendarDay.Bills.Count} {billString}",
                    HorizontalTextAlignment = TextAlignment.Center
                };
                grid.Add(label2, 0, 1);
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
        //Income.AllIncomes = await DBHandler
    }
}