using Budget_Buddy.Models;
using NETCore.Encrypt;
using System.Diagnostics;


namespace Budget_Buddy;

public partial class Dashboard : ContentPage
{
    int UserID;
    string UserName;
    DateTime DBPayday = new DateTime();
    DateTime ViewPayday = new DateTime();
    int PayFrequency = 0;
    List<double> PreferencesList = new List<double>();
    double Income;
    double SavingsPercent;
    double DebtPercent;
    double Balance;
    double CurrentPayperiodBillTotal;
    double SavingsDollarAmount;
    double DebtDollarAmount;


	public Dashboard(int userID)
	{
        InitializeComponent();

        UserID = userID;
    }

    private async void PopulateCurrentPayPeriodGUI()
    {
        other_payperiod_dashboard_grid.IsVisible = false;
        current_payperiod_dashboard_grid.IsVisible = true;
        remaining_balance_grid.IsVisible = false;
        bill_collectionview.ItemsSource = Bill.BillList;
        tempbill_collectionview.ItemsSource = Bill.TempBillList;
        recurringbill_collectionview.ItemsSource = Bill.RecurringBillList;
        current_balance_grid.IsVisible = true;
        savings_paid_checkbox.IsChecked = await DBHandler.GetSavingsPaid(UserID);
        debt_paid_checkbox.IsChecked = await DBHandler.GetDebtPaid(UserID);
        first_name_label.Text = $"Welcome, {UserName}!";
        first_name_label.HorizontalOptions = LayoutOptions.Center;
        Balance = await DBHandler.GetBalance(UserID);
        current_balance_entry.Text = Balance.ToString("N2");

        // Indices as follows  0-Income, 1-Savings Percent, 2-Debt Percent
        PreferencesList = await DBHandler.GetUserPreferences(UserID);
        Income = PreferencesList[0];
        SavingsPercent = PreferencesList[1] / 100;
        DebtPercent = PreferencesList[2] / 100;

        PayFrequency = await DBHandler.GetPayFrequency(UserID);
        int dayDifference = Math.Abs(((DateTime)DBPayday - DateTime.Now).Days);
        if (dayDifference > PayFrequency - 1)
        {
            //Update current pay period based on the modulo of the difference in days.
            await DBHandler.UpdatePayDay(UserID, DBPayday.AddDays(Math.Floor((double)(dayDifference / PayFrequency)) * PayFrequency));
            await DBHandler.UpdateBalance(UserID, Income);
            DBPayday = await DBHandler.GetPayday(UserID);
            await DBHandler.RefreshPaidBills(UserID);
            await DBHandler.ResetSavingsAndDebtPaid(UserID);
            await DBHandler.RemoveAllTempBills(UserID);

            await DBHandler.UpdatePayFrequencyForSetDays(UserID);

        }
        ViewPayday = DBPayday;

        await DBHandler.GenerateBills(UserID, DBPayday, DBPayday.AddDays(PayFrequency - 1));

        payperiod_label.Text = $"{ViewPayday.ToString("MM/dd/yy")} - {ViewPayday.AddDays(PayFrequency - 1).ToString("MM/dd/yy")}";

        CurrentPayperiodBillTotal = 0;
        foreach (Bill bill in Bill.BillList)
        {
            CurrentPayperiodBillTotal += bill.Price;
        }
        foreach (Bill bill in Bill.TempBillList)
        {
            CurrentPayperiodBillTotal += bill.Price;
        }
        foreach (Bill bill in Bill.RecurringBillList)
        {
            CurrentPayperiodBillTotal += bill.Price;
        }

        await DBHandler.UpdateBillAndDebtDollarAmount(UserID, Income - CurrentPayperiodBillTotal, DebtPercent, SavingsPercent);
        SavingsDollarAmount = await DBHandler.GetSavingsDollarAmount(UserID);
        DebtDollarAmount = await DBHandler.GetDebtDollarAmount(UserID);

        RecalculateFunds(Balance);

        double debtAmount = DebtDollarAmount;
        double savingAmount = SavingsDollarAmount;
        current_savings_amount_label.Text = $"${savingAmount.ToString("N2")}";
        current_debt_amount_label.Text = $"${debtAmount.ToString("N2")}";

        loading_icon.IsVisible = false;
    }


    private async void RepopulateGUI(int UserID)
    {
        current_payperiod_dashboard_grid.IsVisible = false;
        other_payperiod_dashboard_grid.IsVisible = true;
        current_balance_grid.IsVisible = false;
        remaining_balance_grid.IsVisible = true;

        if (PayFrequency > 14)
        {
            await DBHandler.GenerateBills(UserID, ViewPayday, ViewPayday.AddMonths(1));
        }
        else
        {
            await DBHandler.GenerateBills(UserID, ViewPayday, ViewPayday.AddDays(PayFrequency - 1));
        }

        double billTotal = 0;
        foreach (Bill bill in Bill.BillList)
        {
            billTotal += bill.Price;
            bill.Paid = false;
        }

        foreach (Bill bill in Bill.RecurringBillList)
        {
            billTotal += bill.Price;
        }

        Bill.TempBillList.Clear();

        double remainingBalance = Income - billTotal;

        remaining_balance_label.Text = $"${remainingBalance.ToString("N2")}";
        payperiod_label.Text = $"{ViewPayday.ToString("MM/dd/yy")} - {ViewPayday.AddDays(PayFrequency - 1).ToString("MM/dd/yy")}";

        double debtAmount = remainingBalance * DebtPercent;
        double savingAmount = remainingBalance * SavingsPercent;
        double leisureAmount = remainingBalance - (debtAmount + savingAmount);
        other_savings_amount_label.Text = $"${savingAmount.ToString("N2")}";
        other_debt_amount_label.Text = $"${debtAmount.ToString("N2")}";
        other_leisure_amount_label.Text = $"${leisureAmount.ToString("N2")}";

        if (remainingBalance <= 0)
        {
            other_savings_amount_label.Text = "$0.00";
            other_debt_amount_label.Text = "$0.00";
            return;
        }
    }

    private async void paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        if (ViewPayday != DBPayday)
        {
            checkBox.IsChecked = false;
            return;
        }
        Bill bill = (Bill)checkBox.BindingContext;
        await DBHandler.UpdatePaidStatus(bill.BillID, bill.Paid);
        RecalculateFunds(Balance);
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        PreferencesPage preferencesPage = new PreferencesPage(UserID,DebtPercent,SavingsPercent,PayFrequency,Income);
        await Navigation.PushAsync(preferencesPage);
    }

    private void forward_arrow_clicked(object sender, EventArgs e)
    {
        if (PayFrequency > 14)
        {
            ViewPayday = ViewPayday.AddMonths(1);
        }
        else
        {
            ViewPayday = ViewPayday.AddDays(PayFrequency);
        }

        if(ViewPayday == DBPayday)
        {
            PopulateCurrentPayPeriodGUI();
            return;
        }
        else
        {
            RepopulateGUI(UserID);
            return;
        }
    }

    private void backwards_arrow_clicked(object sender, EventArgs e)
    {
        if(PayFrequency > 14)
        {
            ViewPayday = ViewPayday.AddMonths(-1);
        }
        else
        {
            ViewPayday = ViewPayday.AddDays(-PayFrequency);
        }

        if (ViewPayday == DBPayday)
        {
            PopulateCurrentPayPeriodGUI();
            return;
        }
        else
        {
            RepopulateGUI(UserID);
            return;
        }
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        UserName = await DBHandler.GetNameOfUser(UserID);
        DBPayday = await DBHandler.GetPayday(UserID);
        PopulateCurrentPayPeriodGUI();
    }

    private async void current_balance_entry_textchanged(object sender, TextChangedEventArgs e)
    {
        if(current_balance_entry.Text == "")
        {
            return;
        }
        await DBHandler.UpdateBalance(UserID, Math.Round(Convert.ToDouble(current_balance_entry.Text), 2));
        Balance = await DBHandler.GetBalance(UserID);
        RecalculateFunds(Balance);
    }

    private async void RecalculateFunds(double balance)
    {
        current_leisure_amount_label.Text = "Calculating...";
        double runningTotal = balance;
        foreach (Bill bill in Bill.BillList)
        {
            if (!bill.Paid)
            {
                runningTotal -= bill.Price;
            }
        }

        foreach (Bill bill in Bill.RecurringBillList)
        {
            if (!bill.Paid)
            {
                runningTotal -= bill.Price;
            }
        }

        foreach (Bill bill in Bill.TempBillList)
        {
            if (!bill.Paid)
            {
                runningTotal -= bill.Price;
            }
        }
        if (!await DBHandler.GetDebtPaid(UserID))
        {
            runningTotal -= DebtDollarAmount;
        }
        if (!await DBHandler.GetSavingsPaid(UserID))
        {
            runningTotal -= SavingsDollarAmount;
        }

        current_leisure_amount_label.Text = $"${runningTotal.ToString("N2")}";
    }

    private async void savings_paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        await DBHandler.UpdateSavingsPaid(UserID, savings_paid_checkbox.IsChecked);
        RecalculateFunds(Balance);
    }

    private async void debt_paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        await DBHandler.UpdateDebtPaid(UserID, debt_paid_checkbox.IsChecked);
        RecalculateFunds(Balance);
    }

    private async void temp_paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        if (ViewPayday > DBPayday)
        {
            return;
        }
        CheckBox checkBox = sender as CheckBox;

        Bill bill = (Bill)checkBox.BindingContext;
        await DBHandler.UpdateTempBillPaidStatus(bill.BillID, bill.Paid);
        RecalculateFunds(Balance);
    }

    private async void recurring_bill_paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        Bill bill = (Bill)checkBox.BindingContext;
        await DBHandler.UpdateRecurringBillPaidStatus(bill.BillID, bill.Paid);
        RecalculateFunds(Balance);
    }
}