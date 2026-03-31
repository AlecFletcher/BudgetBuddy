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

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        Bill_CollectionView.ItemsSource = SelectedDay.Bills;
        Income_CollectionView.ItemsSource = SelectedDay.Incomes;
        Day_Label.BindingContext = SelectedDay;
        if(SelectedDay.Bills.Count > 0)
        {
            Bill_Label.IsVisible = true;
            Bill_Label.Text = "Bill(s) due:";
        }

        if(SelectedDay.Incomes.Count > 0)
        {
            Income_Label.IsVisible = true;
            Income_Label.Text = "Income(s) received:";
        }


    }
}