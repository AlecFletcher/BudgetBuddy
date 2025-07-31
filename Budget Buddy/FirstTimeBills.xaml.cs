using Budget_Buddy.Models;

namespace Budget_Buddy;

public partial class FirstTimeBills : ContentPage
{
    private int UserID;

    public FirstTimeBills(int userId)
    {
        InitializeComponent();
        UserID = userId;
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        Bill.BillList.Clear();
        Bill bill = new Bill(UserID, "", 0, 1, false);
        Bill.BillList.Add(bill);
        BillListCollectionView.ItemsSource = Bill.BillList;
        Debt.DebtList.Clear();
        Debt.DebtList = await DBHandler.GenerateDebtList(UserID);
        DebtListCollectionView.ItemsSource = Debt.DebtList;
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

        foreach (Bill bill in Bill.BillList)
        {
            //Input validation checks
            if (bill.Name == string.Empty)
            { 
                await DisplayAlert("Error", "You have a bill with no name.", "Okay");
                return;
            }
            if (Math.Round(bill.Price, 2) < 0.01)
            {
                await DisplayAlert("Error", "Bills cannot have a zero or negative price.", "Okay");
                return;
            }
            if (bill.DueDay < 1 || bill.DueDay > 28)
            {
                await DisplayAlert("Error", "You have a bill an invalid due day. Day must be between 1 and 28.", "Okay");
                return;
            }
        }

        foreach (Bill bill in Bill.BillList)
            {
            if (bill.Recurring)
            {
                await DBHandler.AddRecurringBill(UserID, bill.Name, bill.Price);
            }
            else
            {
                await DBHandler.AddBill(UserID, bill.Name, bill.Price, bill.DueDay);
            }
            }
        Dashboard dashboard = new Dashboard(UserID);
        await Navigation.PushAsync(dashboard);
    }

    private void plus_symbol_clicked(object sender, EventArgs e)
    {
        Bill bill = new Bill(UserID, "", 0, 0, false);
        Bill.BillList.Add(bill);
    }

    private async void Debt_Delete_Button_Clicked(object sender, EventArgs e)
    {
        ImageButton imageButton = (ImageButton)sender;
        Debt debt = (Debt)imageButton.BindingContext;
        bool answer = await DisplayAlert("Delete?", $"Are you sure you want to delete {debt.Name}?", "Yes", "No");
        if (answer)
        {
            Debt.DebtList.Remove(debt);
            await DBHandler.RemoveDebt(debt.BillID);
        }
    }
}