using NETCore.Encrypt;
using Plugin.Maui.Biometric;
using System.Diagnostics;

namespace Budget_Buddy;

public partial class LoginScreen : ContentPage
{
    //string sourceFile = "SavedLogin.txt";
    //string path = string.Empty;
    public LoginScreen()
	{
		InitializeComponent();
        //path = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, sourceFile);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        TryLogin(Username_Entry.Text, Password_Entry.Text);
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        CreateAccount createAccount = new CreateAccount();
        await Navigation.PushAsync(createAccount);
    }

    private async void TryLogin(string username, string password)
    {
        login_button.IsEnabled = false;
        loading_icon.IsRunning = true;
        try
        {
            int id = await DBHandler.CheckUsernameAndPassword(username, EncryptProvider.Sha256(password));
            if (id == -1)
            {
                loading_icon.IsRunning = false;
                login_button.IsEnabled = true;
                await DisplayAlert("Error", "Username and password combination invalid.", "Okay");
                return;
            }
            else
            {
                await DBHandler.UpdateUserLastLogin(id);
                if (Save_Password_Checkbox.IsChecked)
                {
                    //SaveCredentials(username, password);
                }
                else
                {
                    //SaveCredentials("", "");
                }

                if (await DBHandler.IsNewAccount(id) || await DBHandler.GetBillTotal(id) == 0)
                {
                    loading_icon.IsVisible = false;
                    login_button.IsEnabled = true;
                    FirstTimeIncomePage firstTimeIncomePage = new FirstTimeIncomePage(id);
                    await Navigation.PushAsync(firstTimeIncomePage);
                }
                else
                {
                    loading_icon.IsVisible = false;
                    login_button.IsEnabled = true;
                    Dashboard dashboard = new Dashboard(id);
                    await Navigation.PushAsync(dashboard);
                }
            }
        }
        catch (Exception ex)
        {
            loading_icon.IsRunning = false;
            login_button.IsEnabled = true;
            await DisplayAlert("Error", "Username and password combination invalid.", "Okay");
            return;
        }

    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        Console.WriteLine("Test");
    }

    /*
    private async void AuthenticateAndLogin()
    {
        string savedUsername = string.Empty;
        string savedPassword = string.Empty;

        if (File.Exists(path))
        {
            loading_icon.IsRunning = true;
            login_button.IsEnabled = false;
            try
            {
                string loginCreds = File.ReadAllText(path);
                string[] itemList = loginCreds.Split(',');
                if (itemList.Length > 0)
                {
                    savedUsername = itemList[0];
                    savedPassword = itemList[1];
                    Save_Password_Checkbox.IsChecked = true;

                    Username_Entry.Text = savedUsername;

                    var result = await BiometricAuthenticationService.Default.AuthenticateAsync(new AuthenticationRequest()
                    {
                        Title = "Authenticate to login",
                        NegativeText = "Cancel authentication"
                    }, CancellationToken.None);

                    if (result.Status == BiometricResponseStatus.Success)
                    {
                        loading_icon.IsRunning = true;
                        TryLogin(savedUsername, savedPassword);
                    }
                    else
                    {

                        loading_icon.IsRunning = false;
                        login_button.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        AuthenticateAndLogin();
    }

    private void SaveCredentials(string username, string password)
    {
        StreamWriter stream = new StreamWriter(path, false);
        stream.Write($"{username},{password}");
        stream.Close();
    }
    */
}