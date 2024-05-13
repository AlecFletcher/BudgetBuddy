using System.Windows.Input;

namespace Budget_Buddy;

public partial class LoginScreen : ContentPage
{
    public LoginScreen()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {

        List<int> resultIdList = DBHandler.CheckUsernameAndPassword(Username_Entry.Text, Password_Entry.Text);

        if (resultIdList.Count() > 0)
        {
            Dashboard dashboard = new Dashboard(resultIdList[0]);
            await Navigation.PushAsync(dashboard);
        }
        else
        {
            await DisplayAlert("Error", "Username and password combination do not exist", "Okay");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        CreateAccount createAccount = new CreateAccount();
        await Navigation.PushAsync(createAccount);
    }
}