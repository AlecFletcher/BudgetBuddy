using Budget_Buddy.Models;
using CommunityToolkit.Maui.Alerts;
using System.Diagnostics;


namespace Budget_Buddy;

public partial class Dashboard : ContentPage
{
    int UserID;
    DateTime DBPayday = new DateTime();
    DateTime ViewPayday = new DateTime();
    int PayFrequency = 0;
    List<double> PreferencesList = new List<double>();
    double Income;
    double SavingPercent;
    double DebtPercent;


    public Dashboard()
	{
		InitializeComponent();
    }

	public Dashboard(int userID)
	{
        InitializeComponent();

        UserID = userID;
        bill_collectionview.ItemsSource = Bill.BillList;
        DBPayday = DBHandler.GetPayperiod(UserID);
        PayFrequency = DBHandler.GetPayFrequency(UserID);
        ViewPayday = DBPayday;

        payperiod_label.Text = $"{ViewPayday.ToString("MM/dd/yy")} - {ViewPayday.AddDays(PayFrequency - 1).ToString("MM/dd/yy")}";

        DBHandler.GenerateBills(UserID, DBPayday, DBPayday.AddDays(PayFrequency - 1));

        double billTotal = 0;
        foreach (Bill bill in Bill.BillList)
        {
            billTotal += bill.Price;
        }

        // Indices as follows  0-Income, 1-Savings Percent, 2-Debt Percent
        PreferencesList = DBHandler.GetUserPreferences(UserID);
        Income = PreferencesList[0];
        SavingPercent = PreferencesList[1] / 100;
        DebtPercent = PreferencesList[2] / 100;

        double remainingBalance = Income - billTotal;

        remaining_balance_label.Text = $"${remainingBalance}";

        if(billTotal > Income)
        {
            savings_amount_label.Text = "$0.00";
            debt_amount_label.Text = "$0.00";
            leisure_amount_label.Text = "$0.00";
            return;
        }

        double debtAmount = Math.Round(remainingBalance * DebtPercent,2);
        double savingAmount = Math.Round(remainingBalance * SavingPercent,2);
        savings_amount_label.Text = $"${savingAmount.ToString("N2")}";
        debt_amount_label.Text = $"${debtAmount.ToString("N2")}";
        leisure_amount_label.Text = $"${(remainingBalance - (debtAmount + savingAmount)).ToString("N2")}";
    }

    private void RepopulateGUI(int UserID)
    {
        DBHandler.GenerateBills(UserID, ViewPayday, ViewPayday.AddDays(PayFrequency - 1));

        double billTotal = 0;
        foreach (Bill bill in Bill.BillList)
        {
            billTotal += bill.Price;
        }

        double remainingBalance = Income - billTotal;

        remaining_balance_label.Text = $"${remainingBalance}";

        double debtAmount = Math.Round(remainingBalance * DebtPercent, 2);
        double savingAmount = Math.Round(remainingBalance * SavingPercent, 2);
        savings_amount_label.Text = $"${savingAmount.ToString("N2")}";
        debt_amount_label.Text = $"${debtAmount.ToString("N2")}";
        leisure_amount_label.Text = $"${(remainingBalance - (debtAmount + savingAmount)).ToString("N2")}";

        payperiod_label.Text = $"{ViewPayday.ToString("MM/dd/yy")} - {ViewPayday.AddDays(PayFrequency - 1).ToString("MM/dd/yy")}";
    }

    private void paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        
    }

    //Settings Icon
    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        PreferencesPage preferencesPage = new PreferencesPage(1);
        await Navigation.PushAsync(preferencesPage);
    }

    private void forward_arrow_clicked(object sender, EventArgs e)
    {
        ViewPayday = ViewPayday.AddDays(PayFrequency);
        RepopulateGUI(UserID);
    }

    private void backwards_arrow_clicked(object sender, EventArgs e)
    {
        ViewPayday = ViewPayday.AddDays(-PayFrequency);
        RepopulateGUI(UserID);
    }
}