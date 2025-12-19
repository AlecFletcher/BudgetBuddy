namespace Budget_Buddy;

using Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

public partial class UpdatePreferences : ContentPage
{
    ObservableCollection<Bill> BillList = new ObservableCollection<Bill>();
    int UserID {  get; set; }
    string Purpose;
	public UpdatePreferences()
	{
		InitializeComponent();
	}

	public UpdatePreferences(int userId, string purpose)
	{
        InitializeComponent();
        UserID = userId;
        Purpose = purpose;
    }

    private async void delete_button_clicked(object sender, EventArgs e)
    {
        ImageButton imageButton = sender as ImageButton;
        Bill bill = (Bill)imageButton.BindingContext;
        bool answer = await DisplayAlert("Delete?", $"Are you sure you want to delete {bill.Name} from your bills?", "Yes", "No");
        if (answer)
        {
            if (bill.BillID != null)
            {
                await DBHandler.RemoveBill(bill);
            }
            BillList.Remove(bill);
        }
    }

    private void Add_Bill_Clicked(object sender, EventArgs e)
    {
        Bill bill = new Bill("",0,1,false);
        BillList.Add(bill);
    }

    private async void Save_Button_Clicked(object sender, EventArgs e)
    {
        Save_Bills_Button.IsEnabled = false;
        Bills_Grid.IsVisible = false;
        Loading_Circle.IsVisible = true;
        Loading_Circle.IsRunning = true;

        foreach (Bill bill in BillList)
        {
            //Input validation checks
            if (bill.Name == string.Empty)
            {
                await DisplayAlert("Error", "You have a bill with no name.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (Math.Round(bill.Price, 2) < 0.01)
            {
                await DisplayAlert("Error", "Bills cannot have a zero or negative price.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (bill.DueDay == null || bill.DueDay < 1 || bill.DueDay > 28)
            {
                await DisplayAlert("Error", "You have a bill an invalid due day. Day must be between 1 and 28.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
        }

        foreach(Bill bill in Bill.TempBillList)
        {
            //Input validation checks
            if (bill.Name == string.Empty)
            {
                await DisplayAlert("Error", "You have a temporary bill with no name.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (Math.Round(bill.Price, 2) < 0.01)
            {
                await DisplayAlert("Error", "Temmporary bills cannot have a zero or negative price.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
        }

        foreach (Bill bill in Bill.RecurringBillList)
        {
            //Input validation checks
            if (bill.Name == string.Empty)
            {
                await DisplayAlert("Error", "You have a  recurring bill with no name.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (Math.Round(bill.Price, 2) < 0.01)
            {
                await DisplayAlert("Error", "Recurring bills cannot have a zero or negative price.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
        }


        foreach(Debt debt in Debt.DebtList)
        {
            //Input validation checks
            if (debt.Name == string.Empty)
            {
                await DisplayAlert("Error", "You have a bill with no name.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (Math.Round(debt.Price, 2) < 0.01)
            {
                await DisplayAlert("Error", "Debts cannot have a zero or negative price.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (debt.DueDay < 1 || debt.DueDay > 28)
            {
                await DisplayAlert("Error", "You have a debt with an invalid due day. Day must be between 1 and 28.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
            if (debt.PrincipalBalance <= 0)
            {
                await DisplayAlert("Error", "You have a debt with an principal balance. Balance must be greater than 0.", "Okay");
                Bills_Grid.IsVisible = true;
                Loading_Circle.IsVisible = false;
                Loading_Circle.IsRunning = false;
                return;
            }
        }

        //Now that all inputs are validated, add or update them accordingly
        foreach (Bill bill in BillList)
        {
            //If this is an added bill, add it to DB
            if (bill.BillID == null)
            {
                await DBHandler.AddBill(UserID, bill.Name, bill.Price, bill.DueDay);
            }
            else
            {
                await DBHandler.UpdateBIll(bill);
            }
        }

        foreach(Bill bill in Bill.TempBillList)
        {
            if(bill.BillID == null)
            {
                await DBHandler.AddTempBill(UserID, bill.Name, bill.Price);
            }
            else
            {
                await DBHandler.UpdateTempBIll(bill);
            }
        }

        foreach(Bill bill in Bill.RecurringBillList)
        {
            if(bill.BillID == null)
            {
                await DBHandler.AddRecurringBill(UserID, bill.Name, bill.Price);
            }
            else
            {
                await DBHandler.UpdateRecurringBIll(bill);
            }
        }

        foreach (Debt debt in Debt.DebtList)
        {
            if(debt.BillID == null)
            {
                await DBHandler.AddDebt(UserID, debt);
            }
            else
            {
                await DBHandler.UpdateDebt(debt);
            }
        }
        Bills_Grid.IsVisible = true;
        Loading_Circle.IsVisible = false;
        Loading_Circle.IsRunning = false;
        await Navigation.PopAsync();
    }

    private async void Income_Save_Button_Clicked(object sender, EventArgs e)
    {
        int income = 0;
        income = Convert.ToInt32(Math.Round(Convert.ToDouble(income_entry.Text), 0));
        if (income_entry.Text == string.Empty)
        {
            await DisplayAlert("Error", "Income is empty.", "Okay");
            return;
        }
        if(recent_payday_datepicker.Date > DateTime.Now)
        {
            await DisplayAlert("Error", "Recent payday cannot be in the future.", "Okay");
            return;
        }
        int payFrequency = pay_frequency_picker.SelectedIndex;
        switch (pay_frequency_picker.SelectedIndex)
        {
            case 3:

                payFrequency = pay_frequency_picker.SelectedIndex;
                int firstDay;
                int secondDay;

                if(first_date.Text == null || second_date.Text == null)
                {
                    return;
                }

                try
                {
                    firstDay = Convert.ToInt32(first_date.Text);
                    Console.WriteLine(firstDay);

                    secondDay = Convert.ToInt32(second_date.Text);
                    Console.WriteLine(secondDay);

                    await DBHandler.SetBiMonthlyPaydays(UserID, Convert.ToInt32(first_date.Text), Convert.ToInt32(second_date.Text));

                }
                catch (Exception ex) { Console.WriteLine("Could not cast"); }

                /////////////// MOVE BACK INTO THE TRY ////////////////////
                await DBHandler.UpdatePayFrequencyForSetDays(UserID);
                await Navigation.PopAsync();
                return;
                ///////////////////////////////////////////////////
        }
        await DBHandler.UpdateIncomeAndFrequency(UserID, payFrequency, income, recent_payday_datepicker.Date);
        await Navigation.PopAsync();
    }

    private async void Percentage_Save_Button_Clicked(object sender, EventArgs e)
    {
        int debtPercent = 0;
        int savingsPercent = 0;
        try
        {
            debtPercent = Convert.ToInt32(debt_entry.Text);
            savingsPercent = Convert.ToInt32(savings_entry.Text);
        }
        catch(Exception ex)
        {
            await DisplayAlert("Invalid Format", "Percentages cannot contain decimals", "Okay");
            return;
        }


        
        if(debtPercent < 0)
        {
            await DisplayAlert("Error", "Debt percent cannot be less than 0", "Okay");
            return;
        }
        if (debtPercent > 100)
        {
            await DisplayAlert("Error", "Debt percent cannot greater than 100", "Okay");
            return;
        }
        if (savingsPercent < 0)
        {
            await DisplayAlert("Error", "Savings percent cannot be less than 0", "Okay");
            return;
        }
        if (savingsPercent > 100)
        {
            await DisplayAlert("Error", "Savings percent cannot greater than 100", "Okay");
            return;
        }
        if(savingsPercent + debtPercent > 100)
        {
            await DisplayAlert("Error", "Percentages cannot be greater than 100", "Okay");
            return;
        }

        await DBHandler.UpdateDebtAndSavingsPercent(UserID, savingsPercent, debtPercent);
        await Navigation.PopAsync();
    }

    private async void debt_delete_button_clicked(object sender, EventArgs e)
    {
        ImageButton imageButton = sender as ImageButton;
        Debt debt = (Debt)imageButton.BindingContext;
        bool answer = await DisplayAlert("Delete?", $"Are you sure you want to delete {debt.Name} from your debts?", "Yes", "No");
        if (answer)
        {
            if (debt.BillID != null)
            {
                await DBHandler.RemoveBill(debt);
            }
            Debt.DebtList.Remove(debt);
        }
    }

    private void Add_Debt_Clicked(object sender, EventArgs e)
    {
        Debt debt = new Debt("", 0, 1,1);
        Debt.DebtList.Add(debt);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (Purpose == "Bills")
        {
            Bills_Grid.IsVisible = true;
            BillList = await DBHandler.GetAllBillsList(UserID);
            bills_collectionview.ItemsSource = BillList;
            Debt.DebtList = await DBHandler.GenerateDebtList(UserID);
            debt_collectionview.ItemsSource = Debt.DebtList;
            Bill.TempBillList = await DBHandler.GetAllTempBills(UserID);
            temp_bills_collectionview.ItemsSource = Bill.TempBillList;
            Bill.RecurringBillList = await DBHandler.GetAllRecurringBills(UserID);
            recurring_bills_collectionview.ItemsSource = Bill.RecurringBillList;

        }
        if (Purpose == "Income")
        {
            income_grid.IsVisible = true;
            double incomeText = await DBHandler.GetIncome(UserID);
            income_entry.Text = incomeText.ToString();
            pay_frequency_picker.SelectedIndex = await DBHandler.GetPayFrequencyIndex(UserID);
            recent_payday_datepicker.Date = await DBHandler.GetPayday(UserID);
            if(pay_frequency_picker.SelectedIndex == 3)
            {
                List<int> setDaysList = await DBHandler.GetSetDays(UserID);
                first_date.Text = setDaysList[0].ToString();
                second_date.Text = setDaysList[1].ToString();
            }

        }
        if (Purpose == "Percentages")
        {
            percentages_grid.IsVisible = true;
            percentages_label.IsVisible = true;
            int debtPercentNum = await DBHandler.GetDebtPercent(UserID);
            debt_entry.Text = debtPercentNum.ToString();
            int savingsPercentNum = await DBHandler.GetSavingsPercent(UserID);
            savings_entry.Text = savingsPercentNum.ToString();
        }

    }

    private async void temp_bill_delete_button_clicked(object sender, EventArgs e)
    {
        ImageButton imageButton = sender as ImageButton;
        Bill bill = (Bill)imageButton.BindingContext;
        bool answer = await DisplayAlert("Delete?", $"Are you sure you want to delete {bill.Name} from your temporary bills?", "Yes", "No");
        if (answer)
        {
            if (bill.BillID != null)
            {
                await DBHandler.RemoveTempBill(bill);
            }
            Bill.TempBillList.Remove(bill);
        }
    }

    private async void recurring_bill_delete_button_clicked(object sender, EventArgs e)
    {
        ImageButton imageButton = sender as ImageButton;
        Bill bill = (Bill)imageButton.BindingContext;
        bool answer = await DisplayAlert("Delete?", $"Are you sure you want to delete {bill.Name} from your recurring bills?", "Yes", "No");
        if (answer)
        {
            if (bill.BillID != null)
            {
                await DBHandler.RemoveRecurringBill(bill);
            }
            Bill.RecurringBillList.Remove(bill);
        }
    }

    private void Add_Recurring_Bill_Clicked(object sender, EventArgs e)
    {
        Bill bill = new Bill("", 0);
        Bill.RecurringBillList.Add(bill);
    }

    private void Add_Temp_Bill_Clicked(object sender, EventArgs e)
    {
        Bill bill = new Bill("", 0);
        Bill.TempBillList.Add(bill);
    }

    private void pay_frequency_picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (pay_frequency_picker.SelectedIndex) 
        {
            case 0:
                recent_payday_label.Text = "Most recent payday:";
                recent_payday_datepicker.IsVisible = true;
                first_date.IsVisible = false;
                second_date.IsVisible = false;
                break;
            case 1:
                recent_payday_label.Text = "Most recent payday:";
                recent_payday_datepicker.IsVisible = true;
                first_date.IsVisible = false;
                second_date.IsVisible = false;
                break;
            case 2:
                recent_payday_label.Text = "Most recent payday:";
                recent_payday_datepicker.IsVisible = true;
                first_date.IsVisible = false;
                second_date.IsVisible = false;
                break;
            case 3:
                recent_payday_label.Text = "Select your dates:";
                recent_payday_datepicker.IsVisible = false;
                first_date.IsVisible = true;
                second_date.IsVisible = true;
                break;
        }
    }
}