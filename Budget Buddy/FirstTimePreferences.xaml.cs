using Budget_Buddy.Models;
using System.Security.Cryptography;

namespace Budget_Buddy;

public partial class FirstTimePreferences : ContentPage
{
    int UserID { get; set; }

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
        debt_scrollview.IsVisible = false;
        DebtPayoff_Picker.SelectedIndex = 0;
    }

    private void AdvancedSetup_RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        BasicSetup_RadioButton.IsChecked = !AdvancedSetup_RadioButton.IsChecked;
        AdvancedSetup_Grid.IsVisible = true;
        BasicSetup_Grid.IsVisible = false;
        debt_scrollview.IsVisible = true;
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (HasDebt_Checkbox.IsChecked)
        {
            HasDebt_Grid.IsVisible = true;
            debt_scrollview.IsVisible = true;
            return;
        }
        HasDebt_Grid.IsVisible = false;
        debt_scrollview.IsVisible = false;
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
            if (DebtPercent_Entry.Text == string.Empty)
            {
                await DisplayAlert("Error", "Debt percentage cannot be empty.", "Okay");
                return;
            }

            if (SavingsPercent_Entry.Text == string.Empty)
            {
                await DisplayAlert("Error", "Savings percentage cannot be empty.", "Okay");
                return;
            }

            if (DebtPercent_Entry.Text.Contains('.') || SavingsPercent_Entry.Text.Contains('.'))
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
        foreach(Debt entry in Debt.DebtList)
        {
            if(entry.Name == string.Empty)
            {
                await DisplayAlert("Error", "You have a debt entry with a blank name.", "Okay");
                return;
            }
            if(entry.DueDay < 1 || entry.DueDay > 28)
            {
                await DisplayAlert("Error", "An entry has an invalid date. It must be between 1 and 28.", "Okay");
                return;
            }
            if(entry.PrincipalBalance <= 0)
            {
                await DisplayAlert("Error", "An entry needs a principal balance.", "Okay");
                return;
            }
            if(entry.Price <= 0)
            {
                await DisplayAlert("Error", "An entry has an invalid payment amount.", "Okay");
                return;
            }
        }

        foreach(Debt debt in  Debt.DebtList)
        {
            await DBHandler.AddDebt(UserID, debt);
        }

        await DBHandler.UpdatePreferences(UserID, savingsPercent, debtPayoffPercent);
        FirstTimeBills firstTimeBills = new FirstTimeBills(UserID);
        await Navigation.PushAsync(firstTimeBills);
        
    }

    private void Add_Debt_Button_Clicked(object sender, EventArgs e)
    {
        Debt debt = new Debt("", 0, 0, 1,"");
        Debt.DebtList.Add(debt);
    }

    private async void Delete_Debt_Button_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Delete?", "Are you sure you want to delete this debt entry?", "Yes", "No");
        if (answer)
        {
            ImageButton imageButton = (ImageButton)sender;
            Debt debt = (Debt)imageButton.BindingContext;
            Debt.DebtList.Remove(debt);
        }
        return;
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        Debt.DebtList.Clear();
        await DBHandler.GenerateDebtList(UserID);
        debt_collectionview.ItemsSource = Debt.DebtList;
    }
}