using Budget_Buddy.Models;

namespace Budget_Buddy;

public partial class AddCategory : ContentPage
{
	public AddCategory()
	{
		InitializeComponent();
		Bill.TempCategories = Bill.AllCategories;
		categoriesCollectionView.ItemsSource = Bill.TempCategories;
	}

    private void Plus_Button_Clicked(object sender, EventArgs e)
    {
		Bill.AllCategories.Add(new string(""));
    }

    private void Trashcan_Clicked(object sender, EventArgs e)
    {
		ImageButton imageButton = sender as ImageButton;
		string categoryName = imageButton.BindingContext as string;
		Bill.TempCategories.Remove(categoryName);

		//Run DBHandler script to remove all instances of this category name
    }
}