using NETCore.Encrypt;
using System.Diagnostics;
namespace Budget_Buddy;

public partial class CreateAccount : ContentPage
{
	public CreateAccount()
	{
		InitializeComponent();
	}

    private async void Add_Account_Button_Clicked(object sender, EventArgs e)
    {
		if(username_label.Text.Length < 6 || username_label.Text.Contains(" "))
		{
			await DisplayAlert("Username Error", "Username must be at least 6 characters long with no spaces.", "Okay");
			return;
		}
		if(username_label.Text.Length > 25)
		{
            await DisplayAlert("Username Error", "Username cannot be greater than 25 characters.", "Okay");
            return;
        }
        if (password_label.Text.Length < 6)
		{
            await DisplayAlert("Password Error", "Password must be at least 6 characters long.", "Okay");
            return;
        }
        if (password_label.Text.Length > 25)
        {
            await DisplayAlert("Password Error", "Password cannot be greater than 25 characters.", "Okay");
            return;
        }
        if(first_name_label.Text == string.Empty)
        {
            await DisplayAlert("Name Error", "Please input your first name.", "Okay");
            return;
        }

        string username = username_label.Text;
        if (await DBHandler.DoesUsernameExist(username))
        {
            await DisplayAlert("Error", "Username already exists.", "Okay");
            return;
        }
        string password = EncryptProvider.Sha256(password_label.Text);
        string firstName = first_name_label.Text;

        await DBHandler.AddUser(username, password, firstName);
        await DisplayAlert("Success!", "Account has been created! Log in to continue!", "Okay");
        await Navigation.PopToRootAsync();
    }
}