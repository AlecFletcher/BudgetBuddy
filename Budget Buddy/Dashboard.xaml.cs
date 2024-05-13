using Budget_Buddy.Models;
using System.Diagnostics;

namespace Budget_Buddy;

public partial class Dashboard : ContentPage
{
	public Dashboard()
	{
		InitializeComponent();
    }

	public Dashboard(int UserID)
	{
		InitializeComponent();
        DBHandler.GenerateBills(UserID, new DateTime(2024,5,23), new DateTime(2024, 6, 5));

        bill_collectionview.ItemsSource = Bill.BillList;

    }

    private void paid_checkbox_changed(object sender, CheckedChangedEventArgs e)
    {
        
    }

    //Settings Icon
    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Clicked");
        PreferencesPage preferencesPage = new PreferencesPage(1);
        await Navigation.PushAsync(preferencesPage);
    }

    private void forward_arrow_clicked(object sender, EventArgs e)
    {
        
    }
}