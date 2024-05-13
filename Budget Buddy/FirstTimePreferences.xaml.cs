using System.Security.Cryptography;

namespace Budget_Buddy;

public partial class FirstTimePreferences : ContentPage
{
    int UserID { get; set; }
	public FirstTimePreferences()
	{
		InitializeComponent();
	}

    public FirstTimePreferences(int userId)
    {
        InitializeComponent();
        UserID = userId;
    }

    private void BasicSetup_RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        AdvancedSetup_RadioButton.IsChecked = !BasicSetup_RadioButton.IsChecked;
        AdvancedSetup_Grid.IsVisible = false;
        BasicSetup_Grid.IsVisible = true;
        DebtPayoff_Picker.SelectedIndex = 0;
    }

    private void AdvancedSetup_RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        BasicSetup_RadioButton.IsChecked = !AdvancedSetup_RadioButton.IsChecked;
        AdvancedSetup_Grid.IsVisible = true;
        BasicSetup_Grid.IsVisible = false;
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (HasDebt_Checkbox.IsChecked)
        {
            HasDebt_Grid.IsVisible = true;
            return;
        }
        HasDebt_Grid.IsVisible = false;
        return;
    }

    private void HasSavings_Checkbox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (HasSavings_Checkbox.IsChecked)
        {
            DebtOptions_Grid.IsVisible = true;
        }
        else
        {
            DebtOptions_Grid.IsVisible = false;
            HasDebt_Checkbox.IsChecked = false;
            HasDebt_Grid.IsVisible = false;
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        int savingsPercent = 40;
        int debtPayoffPercent = 0;
        if(BasicSetup_RadioButton.IsChecked)
        {
            if (!HasSavings_Checkbox.IsChecked)
            {
                savingsPercent = 90;
                debtPayoffPercent = 0;
            }
            else if(HasDebt_Checkbox.IsChecked)
            {
                if(DebtPayoff_Picker.SelectedIndex == -1)
                {
                    await DisplayAlert("Error", "Please select how urgently you would like to pay off your debt", "Okay");
                    return;
                }
                debtPayoffPercent = 90 - (DebtPayoff_Picker.SelectedIndex * 20);
                savingsPercent = (DebtPayoff_Picker.SelectedIndex * 15);
            }
        }
        else
        {
            if(DebtPercent_Entry.Text.Contains('.') || SavingsPercent_Entry.Text.Contains('.'))
            {
                await DisplayAlert("Error", "Values cannot have a decimal", "Okay");
                return;
            }

            if(DebtPercent_Entry.Text == string.Empty)
            {
                debtPayoffPercent = 0;
            }
            else
            {
                debtPayoffPercent = int.Parse(DebtPercent_Entry.Text);
            }

            if(SavingsPercent_Entry.Text == string.Empty)
            {
                savingsPercent = 0;
            }
            else
            {
                savingsPercent = int.Parse(SavingsPercent_Entry.Text);
            }

            if(debtPayoffPercent < 0)
            {
                await DisplayAlert("Error", "Debt percentage cannot be less than 0", "Okay");
                return;

            }

            if(savingsPercent < 0)
            {
                await DisplayAlert("Error", "Savings percentage cannot be less than 0", "Okay");
                return;
            }

            int sum = debtPayoffPercent + savingsPercent;
            if(sum > 100)
            {
                await DisplayAlert("Error", $"Values cannot add up to over 100%. Value is currently at {sum}%.", "Okay");
                return;
            }
        }
        Console.WriteLine(debtPayoffPercent.ToString());
        Console.WriteLine(savingsPercent.ToString());
        //DBHandler.UpdatePreferences(UserID, savingsPercent, debtPayoffPercent);
        FirstTimeBills firstTimeBills = new FirstTimeBills();
        await Navigation.PushAsync(firstTimeBills);
        
    }

}