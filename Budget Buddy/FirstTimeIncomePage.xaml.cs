namespace Budget_Buddy;

public partial class FirstTimeIncomePage : ContentPage
{
    int UserId { get; set; }
	public FirstTimeIncomePage()
	{
		InitializeComponent();
    }

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
            FirstTimePreferences firstTimePreferences = new FirstTimePreferences();
            await Navigation.PushAsync(firstTimePreferences);
        }
    }
}