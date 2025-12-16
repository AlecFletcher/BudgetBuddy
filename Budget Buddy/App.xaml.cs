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
            DBHandler.DisconnectFromDB();
        }

        protected async override void OnResume()
        {
            DBHandler.ConnectToDB();
        }
    }
}
