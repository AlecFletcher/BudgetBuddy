using Budget_Buddy.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Budget_Buddy;

public partial class BillDebtReport : ContentPage
{
	int UserID { get; set; }
	ObservableCollection<Bill> OriginalList = new ObservableCollection<Bill>();
    List<Bill> tempBills = new List<Bill>();

    public BillDebtReport(int userId)
    {
        InitializeComponent();
        UserID = userId;

    }


    private void search_text_changed(object sender, TextChangedEventArgs e)
    {
		string enteredText = search_entry.Text;
        tempBills = OriginalList.Where(i => i.Name.ToLower().Contains(enteredText.ToLower())).ToList();
        bills_collectionview.ItemsSource = tempBills;
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        OriginalList = await DBHandler.GetAllBillsAndDebts(UserID);
        tempBills = OriginalList.ToList();
        bills_collectionview.ItemsSource = tempBills;
    }
}