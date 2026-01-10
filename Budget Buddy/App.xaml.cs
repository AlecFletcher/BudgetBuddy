using AndroidX.Navigation;

namespace Budget_Buddy
{
    public partial class App : Application
    {
        int UserID;
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginScreen());
            //MainPage = new NavigationPage(new PreferencesPage());
        }

        protected override void OnSleep()
        {
            try
            {
                DBHandler.DisconnectFromDB();
            }
            catch (Exception ex) { }
        }

        protected override void OnResume()
        {
            try
            {
                DBHandler.ConnectToDB();
            }
            catch(Exception ex) { }
        }
    }
}
