namespace Budget_Buddy;

public partial class FirstTimeIncomePage : ContentPage
{
    int UserId { get; set; }

    public FirstTimeIncomePage(int userId)
    {
        InitializeComponent();
        UserId = userId;
        PayFrequency_Picker.SelectedIndex = 0;
        this.UserId = userId;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        int income;
        bool allFieldsAreFull = true;
        try 
            { 
            income = int.Parse(Income_Entry.Text);
            Income_Entry.BackgroundColor = Color.Parse("#ffffff");
        }

        catch (Exception ex) 
            { 
            Income_Entry.BackgroundColor = Color.Parse("#ff8f8f");
            await DisplayAlert("Error", "Please input a number for your income.", "Okay");
            allFieldsAreFull = false;
            return;
        }

        if (Payday_Datepicker.Date > DateTime.Today)
        {
            Payday_Datepicker.BackgroundColor = Color.Parse("#ff8f8f");
            await DisplayAlert("Error", "Recent payday cannot be later than today", "Okay");
            allFieldsAreFull = false;
            return;
        }

        if (allFieldsAreFull)
        {
            int payFrequency = PayFrequency_Picker.SelectedIndex;
            string payFrequencyString = "";
            switch (payFrequency)
            {
                case 0:
                    payFrequencyString = "Weekly";
                    break;
                case 1:
                    payFrequencyString = "BiWeekly";
                    break;
                case 2:
                    payFrequencyString = "Monthly";
                    break;
                case 3:
                    payFrequencyString = "BiMonthly";
                    break;
            }
            int currentIncome = (int)Math.Round(Convert.ToDouble(Income_Entry.Text), 0);
            await DBHandler.UpdateIncomeAndFrequency(UserId, payFrequencyString, currentIncome, Payday_Datepicker.Date);
            FirstTimePreferences firstTimePreferences = new FirstTimePreferences(UserId);
            await Navigation.PushAsync(firstTimePreferences);
        }
    }
}