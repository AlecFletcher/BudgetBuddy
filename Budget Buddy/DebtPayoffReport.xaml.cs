using Budget_Buddy.Models;
using System.Collections.ObjectModel;

namespace Budget_Buddy;

public partial class DebtPayoffReport : ContentPage
{
	int UserID;
	public DebtPayoffReport(int userId)
	{
		InitializeComponent();
		UserID = userId;
	}

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
		double monthylIncome = await DBHandler.GetMonthlyIncome(UserID);
		double debtPercent = await DBHandler.GetDebtPercent(UserID);
		double billTotal = await DBHandler.GetBillTotal(UserID);
		double debtPerMonth = (monthylIncome - billTotal) * (debtPercent / 100);
		double payoffMonths = 0;
		ObservableCollection<Debt> debtList = await DBHandler.GenerateDebtList(UserID);
		List<DebtPayoff> debtPayoffList = new List<DebtPayoff>();
		double debtTotal = 0;
		foreach(Debt debt in debtList)
		{
			double payoffTime = Math.Round(debt.PrincipalBalance / debtPerMonth, 1);
			payoffMonths += payoffTime;
            debtTotal += debt.Price;
			if(payoffTime < 0)
			{
				payoffTime = 0;
			}
			DebtPayoff debtPayoff = new DebtPayoff(debt.Name, debt.PrincipalBalance, payoffTime);
			debtPayoffList.Add(debtPayoff);
        }
		if(payoffMonths < 0)
		{
            header_label.Text = $"Your bills are greater than your income. Payoff cannot be estimated.";
            header_label.HorizontalTextAlignment = TextAlignment.Center;
            timestamp_label.Text = $"Last reported: {DateTime.Now.ToString("dd/MM hh:mm")}";
            return;
        }
		payoff_collectionview.ItemsSource = debtPayoffList;
		header_label.Text = $"It will take {payoffMonths} months to become debt free!";
		header_label.HorizontalTextAlignment = TextAlignment.Center;
		timestamp_label.Text = $"Last reported: {DateTime.Now.ToString("dd/MM hh:mm")}";
    }
}