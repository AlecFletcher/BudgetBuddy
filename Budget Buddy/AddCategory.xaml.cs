using Budget_Buddy.Models;
using System.Threading.Tasks;

namespace Budget_Buddy;

public partial class AddCategory : ContentPage
{
    List<int?> IdsToBeDeleted = new List<int?>();
    List<string> CategoriesToBeAdded = new List<string>();
    List<Category> CategoriesToBeUpdated = new List<Category>();
	public AddCategory()
	{
		InitializeComponent();
        Category.TempCategories = Category.AllCategories;
		categoriesCollectionView.ItemsSource = Category.TempCategories;
        IdsToBeDeleted.Clear();
        CategoriesToBeAdded.Clear();
        CategoriesToBeUpdated.Clear();

    }

    private void Plus_Button_Clicked(object sender, EventArgs e)
    {
        Category.TempCategories.Add(new Category(""));
    }

    private void Trashcan_Clicked(object sender, EventArgs e)
    {
		ImageButton imageButton = sender as ImageButton;
		Category category = imageButton.BindingContext as Category;
        try
        {
            IdsToBeDeleted.Add(category.Id);
        }
        catch(Exception ex)
        {

        }
        try { CategoriesToBeUpdated.Remove(category); }
        catch(Exception ex)
        {

        }

        try { CategoriesToBeAdded.Remove(category.Name); }
        catch (Exception ex)
        {

        }
        Category.TempCategories.Remove(category);
    }

    private void Text_Changed(object sender, TextChangedEventArgs e)
    {
        Entry imageButton = sender as Entry;
        Category category = imageButton.BindingContext as Category;
        if (!CategoriesToBeUpdated.Contains(category) && category.Id != null)
        {
            CategoriesToBeUpdated.Add(category);
        }
    }

    private async void Save_Button_Clicked(object sender, EventArgs e)
    {

        if (IdsToBeDeleted.Count > 0)
        {
            await DBHandler.DeleteCategories(IdsToBeDeleted);
        }
        foreach (Category category in Category.TempCategories)
        {
            if (category.Id == null && category.Name != null)
            {
                CategoriesToBeAdded.Add(category.Name);
            }
        }
        if (CategoriesToBeAdded.Count > 0)
        {
            await DBHandler.AddCategory(Dashboard.UserID, CategoriesToBeAdded);
        }

        if (CategoriesToBeUpdated.Count > 0)
        {
            foreach (Category category in CategoriesToBeUpdated)
            {
                await DBHandler.UpdateCategory(category.Id, category.Name);
            }
        }

        await Navigation.PopAsync();
    }
}