using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Budget_Buddy.Models
{
    public class Bill
    {
        public static ObservableCollection<Bill> BillList = new ObservableCollection<Bill>();
        public static ObservableCollection<Bill> TempBillList = new ObservableCollection<Bill>();
        public static ObservableCollection<Bill> RecurringBillList = new ObservableCollection<Bill>();
        public int? BillID {  get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int DueDay { get; set; }
        public bool Paid {  get; set; }
        public bool Recurring { get; set; }
        public string? Category { get; set; }
        public Bill()
        {

        }
        public Bill(int id, string name, double price, int dueDay, bool paid, string? category)
        {
            BillID = id;
            Name = name;
            Price = price;
            DueDay = dueDay;
            Paid = paid;
            Category = category;
        }

        public Bill(string name, double price, int dueDay, bool paid, string? category)
        {
            BillID = null;
            Name = name;
            Price = price;
            DueDay = dueDay;
            Paid = paid;
            Category = null;
        }

        public Bill(int id, string name, double price, bool paid, string? category)
        {
            BillID = id;
            Name = name;
            Price = price;
            Paid = paid;
            Category = category;
        }

        public Bill(string name, double price)
        {
            Name = name;
            Price = price;
        }

    }

    public class Debt : Bill
    {
        public static ObservableCollection<Debt> DebtList = new ObservableCollection<Debt>();
        public double PrincipalBalance { get; set; }
        public Debt(string name, double balanceDue, double minimumPayment, int dueday)
        {
            BillID = null;
            Name = name;
            PrincipalBalance = balanceDue;
            Price = minimumPayment;
            DueDay = dueday;
        }

        public Debt(int debtId, string name, double minimumPayment, int dueday, double balanceDue )
        {
            BillID = debtId;
            Name = name;
            PrincipalBalance = balanceDue;
            Price = minimumPayment;
            DueDay = dueday;
        }
    }
}
