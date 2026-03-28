namespace Budget_Buddy;

public partial class CalendarPage : ContentPage
{
	public CalendarPage()
	{
		InitializeComponent();
    }
    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
            calendarGrid.IsVisible = true;
            //Sunday = 0, Monday = 1, Tuesday = 2, etc.

            int dayOfWeek = Convert.ToInt32(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek);
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);


            if (dayOfWeek + daysInMonth > 28)
            {
                calendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            }

            if (dayOfWeek + daysInMonth > 35)
            {
                calendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
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
                    StyleId = (j - dayOfWeek + 1).ToString()


                };

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
                Label label2 = new Label
                {
                    TextColor = Colors.Black,
                    FontSize = 10,
                    Text = $"1 Income",
                    HorizontalTextAlignment = TextAlignment.Center
                };

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    Calendar_Item_Clicked(s, e);
                };
                grid.GestureRecognizers.Add(tapGesture);
                grid.Add(label, 0, 0);
                grid.Add(label2, 0, 1);
                calendarGrid.Add(grid, j % 7, (int)Math.Floor((double)j / 7));
            }
    }

    private void Calendar_Item_Clicked(object sender, EventArgs e)
    {
        if (sender != null)
        {
            Grid grid = sender as Grid;
            Console.WriteLine(grid.StyleId);
        }

    }
}