using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy.Models
{
    public class Bill
    {
        public static ObservableCollection<Bill> BillList = new ObservableCollection<Bill>();
        public string Name { get; set; }
        public double Price { get; set; }
        public int DueDay { get; set; }
        public bool Paid {  get; set; }

        public Bill(string name, double price, int dueDay, bool paid)
        {
            Name = name;
            Price = price;
            DueDay = dueDay;
            Paid = paid;
        }
    }
}
