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

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        Bill_CollectionView.ItemsSource = SelectedDay.Bills;
        Income_CollectionView.ItemsSource = SelectedDay.Incomes;
        Day_Label.BindingContext = SelectedDay;

        await DBHandler.GetCategories(Dashboard.UserID);


        if (SelectedDay.Bills.Count > 0)
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

    private void AddBillClicked(object sender, EventArgs e)
    {
        if(BillNameEntry.Text != "" && BillAmountEntry.Text != "")
        {

        }
    }

    private void AddIncomeClicked(object sender, EventArgs e)
    {

    }
}