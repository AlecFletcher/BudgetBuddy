using Budget_Buddy.Models;

namespace Budget_Buddy;

public partial class CalendarDayEntryPage : ContentPage
{
	CalendarDay SelectedDay { get; set; }
	public CalendarDayEntryPage(CalendarDay calendarDay)
	{
		InitializeComponent();
		SelectedDay = calendarDay;
	}

    private void OnAddBillClicked(object sender, EventArgs e)
    {

    }

    private void OnAddIncomeClicked(object sender, EventArgs e)
    {

    }
}