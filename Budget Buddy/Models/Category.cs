using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy.Models
{
    public class Category
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public static ObservableCollection<Category> AllCategories { get; set; } = new ObservableCollection<Category>();
        public static ObservableCollection<Category> TempCategories { get; set; } = new ObservableCollection<Category>();
        public Category(int id, int userid, string name)
        {
            Id = id;
            UserId = userid;
            Name = name;
        }

        public Category(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public Category(string name)
        {
            Id = null;
            Name = name;
        }
    }
}
