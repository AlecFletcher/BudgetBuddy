namespace Budget_Buddy;

public partial class FutureReports : ContentPage
{
	int UserID;
	double Income;
	int PayFrequency;
    double MonthylBillTotal;
	double DebtPercent;
    double SavingsPercent;
    public FutureReports(int userId, double debtPercent, double savingsPercent, int payFrequency, double income)
    {
		InitializeComponent();
		UserID = userId;
		Income = income;
		PayFrequency = payFrequency;
		DebtPercent = debtPercent;
		SavingsPercent = savingsPercent;
        savings_label.Text = $"Savings ({savingsPercent * 100}%):";
        debt_label.Text = $"Debt ({debtPercent * 100}%):";

		if(PayFrequency == 14)
		{
			Income = Income * 2;
		}

		if(PayFrequency == 7)
		{
			Income = Income * 4;
		}
	}

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
		double tempIncome = new double();
		double tempBillTotal = new double();
		Picker picker = (Picker)sender;
		int index = picker.SelectedIndex;
		switch(index)
		{
			case 0:
                tempIncome = Income * 3;
				tempBillTotal = MonthylBillTotal * 3;
				break;

			case 1:
                tempIncome = Income * 6;
                tempBillTotal = MonthylBillTotal * 6;
                break;

            case 2:
                tempIncome = Income * 12;
                tempBillTotal = MonthylBillTotal * 12;
                break;

            case 3:
                tempIncome = Income * 24;
                tempBillTotal = MonthylBillTotal * 24;
                break;

            case 4:
                tempIncome = Income * 36;
                tempBillTotal = MonthylBillTotal * 36;
                break;

            case 5:
                tempIncome = Income * 48;
                tempBillTotal = MonthylBillTotal * 48;
                break;

            case 6:
                tempIncome = Income * 60;
                tempBillTotal = MonthylBillTotal * 60;
                break;
        }
        timestamp_label.Text = $"Last reported: {DateTime.Now.ToString("MM/dd hh:mm")}";

        if (tempIncome < tempBillTotal)
        {
            savings_label.Text = "$0.00";
            debt_label.Text = "$0.00";
            s_label.Text = $"Savings: ({SavingsPercent * 100}%)";
            d_label.Text = $"Debt Repayment: ({DebtPercent * 100}%)";
        }
        else
        {
            savings_label.Text = $"${((tempIncome - tempBillTotal) * SavingsPercent).ToString("N2")}";
            debt_label.Text = $"${((tempIncome - tempBillTotal) * DebtPercent).ToString("N2")}";
            s_label.Text = $"Savings: ({SavingsPercent * 100}%)";
            d_label.Text = $"Debt Repayment: ({DebtPercent * 100}%)";
        }
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        MonthylBillTotal = await DBHandler.GetBillTotal(UserID);
    }
}