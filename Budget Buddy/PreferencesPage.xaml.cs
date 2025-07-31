using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Budget_Buddy;

public partial class PreferencesPage : ContentPage
{
    int UserID;
    double DebtPercent;
    double SavingsPercent;
    int PayFrequency;
    double Income;
	public PreferencesPage()
	{
		InitializeComponent();
	}

    public PreferencesPage(int userId, double debtPercent, double savingsPercent, int payFrequency, double income)
    {
        InitializeComponent();
        UserID = userId;
        DebtPercent = debtPercent;
        SavingsPercent = savingsPercent;
        PayFrequency = payFrequency;
        Income = income;
    }

    private async void Report_Button_Clicked(object sender, EventArgs e)
    {
        FutureReports futureReports = new FutureReports(UserID, DebtPercent, SavingsPercent, PayFrequency, Income);
        await Navigation.PushAsync(futureReports);
    }

    private async void Update_Percentage_Button_Clicked(object sender, EventArgs e)
    {
        UpdatePreferences updatePreferences = new UpdatePreferences(UserID, "Percentages");
        await Navigation.PushAsync(updatePreferences);
    }

    private async void Update_Income_Clicked(object sender, EventArgs e)
    {
        UpdatePreferences updatePreferences = new UpdatePreferences(UserID, "Income");
        await Navigation.PushAsync(updatePreferences);
    }

    private async void Update_Bills_Button_Clicked(object sender, EventArgs e)
    {
        UpdatePreferences updatePreferences = new UpdatePreferences(UserID, "Bills");
        await Navigation.PushAsync(updatePreferences);
    }

    private async void Debt_Payoff_Button_Clicked(object sender, EventArgs e)
    {
        DebtPayoffReport debtPayoffReport = new DebtPayoffReport(UserID);
        await Navigation.PushAsync(debtPayoffReport);
    }

    private async void Bill_Debt_List_Button_Clicked(object sender, EventArgs e)
    {
        BillDebtReport billDebtReport = new BillDebtReport(UserID);
        await Navigation.PushAsync(billDebtReport);
    }

    private async void Log_out_clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Log out?", "Are you sure you want to log out?", "Yes", "No");
        if (answer)
        {
            await Navigation.PopToRootAsync();
        }
    }
}