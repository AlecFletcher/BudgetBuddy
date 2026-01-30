using Android.Security.Identity;
using Budget_Buddy.Models;
using Microcharts;
using Microsoft.VisualBasic;
using NETCore.Encrypt;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Budget_Buddy;

public partial class Dashboard : ContentPage
{
    static int UserID;
    string UserName;
    DateTime DBPayday = new DateTime();
    DateTime ViewPayday = new DateTime();
    DateTime NextPayday = new DateTime();
    string PayFrequencyString;
    int PayFrequency = 0;
    int PayFrequencyIndex = 0;
    List<double> PreferencesList = new List<double>();
    double Income;
    double SavingsPercent;
    double DebtPercent;
    double Balance;
    double CurrentPayperiodBillTotal;
    double SavingsDollarAmount;
    double DebtDollarAmount;
    int SetDayOne;
    int SetDayTwo;
    int PrimaryIncomeId;



    public Dashboard(int userID)
	{
        InitializeComponent();

        UserID = userID;
    }

    private async void CheckUpdatePayday()
    {
        int dayDifference = Math.Abs((DBPayday - DateTime.Now).Days);

        Console.WriteLine(PayFrequencyString);

        switch (PayFrequencyString)
        {
            //NEED TO DETERMINE HOW MANY DAYS TO SET FORWARD RUN RECURSIVELY??
            case "Weekly":
                PayFrequencyIndex = 0;
                PayFrequency = 6;
                if(dayDifference >= 7)
                {
                    await DBHandler.UpdatePayDay(PrimaryIncomeId, DBPayday.AddDays(Math.Floor((double)dayDifference / 7)));
                    DBPayday = DBPayday.AddDays(Math.Floor((double)dayDifference/7));
                    NewPaydayHit();
                }
                PopulateCurrentPayPeriodGUI();
                break;

            case "BiWeekly":
                PayFrequencyIndex = 1;
                PayFrequency = 13;
                if (dayDifference >= 14)
                {
                    await DBHandler.UpdatePayDay(PrimaryIncomeId, DBPayday.AddDays(Math.Floor((double)dayDifference / 14)));
                    DBPayday = DBPayday.AddDays(Math.Floor((double)dayDifference / 14));
                    NewPaydayHit();
                }
                PopulateCurrentPayPeriodGUI();
                break;

            case "BiMonthly":
                PayFrequencyIndex = 3;
                List<int> dayList = await DBHandler.GetSetDays(UserID);
                SetDayOne = dayList[0];
                SetDayTwo = dayList[1];
                Console.WriteLine(DBPayday.ToString());
                Console.WriteLine(DBPayday.Month);
                //Current payday is setDayOne
                if (DBPayday.Day == SetDayOne)
                {


                    if (DateTime.Now.Day >= SetDayTwo && DateTime.Now.Month == DBPayday.Month)
                    {
                        //Update to 2nd payday
                        await DBHandler.UpdatePayDay(PrimaryIncomeId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, SetDayTwo));
                        DBPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, SetDayTwo);
                        NewPaydayHit();
                    }


                }

                
                else if (DBPayday < DateTime.Now && DBPayday.Month != DateTime.Now.Month)
                {
                    //Find which payday was more recent
                    if (DateTime.Now.Day < SetDayTwo)
                    {
                        await DBHandler.UpdatePayDay(PrimaryIncomeId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, SetDayOne));
                        DBPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, SetDayOne);
                        NewPaydayHit();
                    }
                    else
                    {
                        await DBHandler.UpdatePayDay(PrimaryIncomeId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, SetDayTwo));
                        DBPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, SetDayTwo);
                        NewPaydayHit();
                    }
                }

                //Upate Pay Frequency
                if (DBPayday.Day == SetDayOne)
                {
                    PayFrequency = Math.Abs(DBPayday.Day - SetDayTwo);
                }

                else
                {
                    PayFrequency = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - SetDayTwo + SetDayOne;
                }
                PopulateCurrentPayPeriodGUI();
                break;

            case "Monthly":
                PayFrequencyIndex = 2;

                if (dayDifference >= DateTime.DaysInMonth(DBPayday.Year, DBPayday.Month))
                {
                    await DBHandler.UpdatePayDay(PrimaryIncomeId, DBPayday.AddMonths((int)Math.Floor((double)dayDifference / 30)));
                    DBPayday = DBPayday.AddDays(Math.Floor((double)dayDifference / 14));
                    NewPaydayHit();
                }
                PopulateCurrentPayPeriodGUI();
                break;
                
        }


        ViewPayday = DBPayday;
    }

    private async void NewPaydayHit()
    {
        await DBHandler.UpdateBalance(UserID, Income);
        DBPayday = await DBHandler.GetPayday(UserID);
        await DBHandler.RefreshPaidBills(UserID);
        await DBHandler.ResetSavingsAndDebtPaid(UserID);
        await DBHandler.RemoveAllTempBills(UserID);
        RepopulateGUI(UserID);
    }

    private async void PopulateCurrentPayPeriodGUI()
    {

        
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



        if (PayFrequency > 20)
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
        if (PayFrequency > 20)
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
        if(PayFrequency > 20)
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
        HideMonthlyGrid();
        bill_collectionview.ItemsSource = Bill.BillList;
        tempbill_collectionview.ItemsSource = Bill.TempBillList;
        recurringbill_collectionview.ItemsSource = Bill.RecurringBillList;
        UserName = await DBHandler.GetNameOfUser(UserID);
        DBPayday = await DBHandler.GetPayday(UserID);
        other_payperiod_dashboard_grid.IsVisible = false;
        current_payperiod_dashboard_grid.IsVisible = true;
        remaining_balance_grid.IsVisible = false;
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
        PrimaryIncomeId = (int)PreferencesList[3];

        PayFrequencyString = await DBHandler.GetPayFrequency(UserID);

        CheckUpdatePayday();
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

    private async void bill_paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        Bill bill = (Bill)checkBox.BindingContext;
        await DBHandler.UpdatePaidStatus(bill.BillID, bill.Paid);
        RecalculateFunds(Balance);
    }

    public static int GetUserID()
    {
        return UserID;
    }

    private async void Calendar_Clicked(object sender, EventArgs e)
    {
        if (current_payperiod_dashboard_grid.IsVisible)
        {
            PopulateMonthlyGrid();
            await DoMonthlyCalculation();
        }
        else
        {
            HideMonthlyGrid();
            PopulateCurrentPayPeriodGUI();
        }

    }

    private async Task DoMonthlyCalculation()
    {
        double TempIncome = 0;
        int extrapaydays = 1;
        

        switch (PayFrequency)
        {
            case 0:
                extrapaydays = 3;
                break;

            case 1:
                extrapaydays = 1;
                break;

            case 2:
                extrapaydays = 0;
                break;

            case 3:
                extrapaydays = 1;
                break;
        }

        TempIncome = Income * (extrapaydays + 1);
        payperiod_label.Text = $"{ViewPayday.ToString("MM/dd/yy")} - {ViewPayday.AddMonths(1).ToString("MM/dd/yy")}";
        Console.WriteLine(DBPayday.ToString() + ". " + ViewPayday.ToString());
        await DBHandler.GenerateBills(UserID, DBPayday, DBPayday.AddMonths(1));

        CurrentPayperiodBillTotal = 0;
        foreach (Bill bill in Bill.BillList)
        {
            CurrentPayperiodBillTotal += bill.Price;
            if (!bill.Paid)
            {
                
            }

        }
        foreach (Bill bill in Bill.TempBillList)
        {
            CurrentPayperiodBillTotal += bill.Price;
            if (!bill.Paid)
            {
                
            }
        }

        if(extrapaydays > 0)
        {
            List<Bill> tempList = Bill.RecurringBillList.ToList();
            for (int i = 0; i < extrapaydays; i++)
            {
                foreach (Bill bill in tempList) 
                {
                    Bill.RecurringBillList.Add(bill);
                }
            }
        }

        

            foreach (Bill bill in Bill.RecurringBillList)
        {
            CurrentPayperiodBillTotal += bill.Price;

            if (!bill.Paid)
            {
                
            }
        }
        Balance = await DBHandler.GetBalance(UserID);
        monthly_current_balance_entry.Text = Balance.ToString();
        monthly_bill_total_label.Text = "$" + CurrentPayperiodBillTotal.ToString();
        remaining_monthly_balance_label.Text = "$" + (Convert.ToDouble(TempIncome) - CurrentPayperiodBillTotal).ToString();

    }

    private void PopulateMonthlyGrid()
    {

        current_payperiod_dashboard_grid.IsVisible = false;
        monthly_balance_grid.IsVisible = true;
        current_balance_grid.IsVisible = false;
        current_balance_label.IsVisible = false;
        current_balance_entry.IsVisible = false;
        remaining_balance_grid.IsVisible = false;

        MakePieChart();
    }

    private void HideMonthlyGrid()
    {
        current_payperiod_dashboard_grid.IsVisible = true;
        monthly_balance_grid.IsVisible = false;
        current_balance_grid.IsVisible = true;
        current_balance_label.IsVisible = true;
        current_balance_entry.IsVisible = true;
        remaining_balance_grid.IsVisible = false;

        chartGrid.IsVisible = false;
    }

    private void MakePieChart()
    {
        string[] colors = { "#1f3f5c", "#2670b5", "#5999d4", "#0c2d4d", "#2aa4bd", "#007d96", "#12414a", "#7c61ab", "#462a75", "#1a0440" };

        chartGrid.IsVisible = true;
        var entries = new[]
        {
            new ChartEntry(200) { Label = "Num (Pe%)", ValueLabel = "Leisure", Color = SKColor.Parse(colors[0]) },
            new ChartEntry(600) { Label = "Num (Pe%)", ValueLabel = "Car", Color = SKColor.Parse(colors[1]) },
            new ChartEntry(400) { Label = "Num (Pe%)", ValueLabel = "Debt", Color = SKColor.Parse(colors[2]) },
            new ChartEntry(50) { Label = "Num (Pe%)", ValueLabel = "Entertainment", Color = SKColor.Parse(colors[3]) },
            new ChartEntry(800) { Label = "Num (Pe%)", ValueLabel = "Rent", Color = SKColor.Parse(colors[4]) },
            new ChartEntry(100) { Label = "Num (Pe%)", ValueLabel = "Other", Color = SKColor.Parse(colors[5]) },
        };

        chartView.Chart = new PieChart()
        {
            Entries = entries,
            LabelTextSize = 40,
            LabelMode = LabelMode.RightOnly,
            AnimationDuration = TimeSpan.FromSeconds(5.5)
        };
    }
}