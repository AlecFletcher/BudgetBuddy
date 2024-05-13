using Budget_Buddy.Models;

namespace Budget_Buddy;

public partial class FirstTimeBills : ContentPage
{
    private int UserID;
	public FirstTimeBills()
	{
		InitializeComponent();
    }
    public FirstTimeBills(int userId)
    {
        InitializeComponent();
        BillListCollectionView.BindingContext = Bill.BillList;
        UserID = userId;
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        for (int i = 0; i < 10; i++)
        {
            Bill bill = new Bill($"Bill{i} testest", 25.33 + i, 10 + i, true);
            Bill.BillList.Add(bill);
        }
        BillListCollectionView.ItemsSource = Bill.BillList;
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        ImageButton deleteButton = (ImageButton)sender;
        Bill bill = (Bill)deleteButton.BindingContext;
        bool answer = await DisplayAlert("Delete?", $"Are you sure you want to remove {bill.Name}?", "Yes", "No");
        if (answer)
        {
            Bill.BillList.Remove(bill);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if(Bill.BillList.Count == 0)
        {
            await DisplayAlert("Error", "Please add at least 1 bill", "Okay");
            return;
        }
        foreach(Bill bill in Bill.BillList)
        {
            if(bill.Name != string.Empty && bill.DueDay != 0 && bill.DueDay < 31 && bill.Price > 0)
            {
                //DBHandler.AddBill(UserID, bill.Name, bill.Price, bill.DueDay);
            }
        }
        Dashboard dashboard = new Dashboard(UserID);
        await Navigation.PushModalAsync(dashboard);
    }

    private void plus_symbol_clicked(object sender, EventArgs e)
    {
        Bill bill = new Bill("", 0, 0, false);
        Bill.BillList.Add(bill);
    }
}