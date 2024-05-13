namespace Budget_Buddy
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginScreen());
        }
    }
}
