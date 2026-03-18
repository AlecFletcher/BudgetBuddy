using Budget_Buddy.Models;

namespace Budget_Buddy;

public partial class AddCategory : ContentPage
{
	public AddCategory()
	{
		InitializeComponent();
        Category.TempCategories = Category.AllCategories;
		categoriesCollectionView.ItemsSource = Category.TempCategories;
	}

    private void Plus_Button_Clicked(object sender, EventArgs e)
    {
        Category.AllCategories.Add(new Category(""));
    }

    private void Trashcan_Clicked(object sender, EventArgs e)
    {
		ImageButton imageButton = sender as ImageButton;
		Category category = imageButton.BindingContext as Category;
        Category.TempCategories.Remove(category);

		//Run DBHandler script to remove all instances of this category name
    }
}