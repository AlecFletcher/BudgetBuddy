using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy.Models
{
    public class Income
    {
        public static ObservableCollection<Income> AllIncomes = new ObservableCollection<Income>();
        public int UserId { get; set; }
        public int IncomeId { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public DateTime? PayDate { get; set; }
        public int? PayFrequency { get; set; }
        public int? DayOne { get; set; }
        public int? DayTwo { get; set; }
        public bool IsPrimary { get; set; }
        public bool InvertedIsPrimary { get; set; }

        public Income(string name, double amount, DateTime? date, bool isPrimary)
        {
            Name = name;
            Amount = amount;
            IsPrimary = isPrimary;
            InvertedIsPrimary = !isPrimary;
            PayDate = date;
            DayOne = null;
            DayTwo = null;
        }

        public Income(int id, string name, double amount, DateTime? date, bool isPrimary)
        {
            IncomeId = id;
            Name = name;
            Amount = amount;
            IsPrimary = isPrimary;
            InvertedIsPrimary = !isPrimary;
            PayDate = date;
        }

        public Income(string name, double amount, int payFrequency, bool isPrimary, DateTime payDate)
        {
            Name = name;
            Amount = amount;
            PayFrequency = payFrequency;
            IsPrimary = true;
            InvertedIsPrimary = !isPrimary;
            PayDate = payDate;
        }


        public Income(int id, string name, double amount, int payFrequency, bool isPrimary, DateTime payDate)
        {
            IncomeId = id;
            Name = name;
            Amount = amount;
            PayFrequency = payFrequency;
            IsPrimary = true;
            InvertedIsPrimary = !isPrimary;
            PayDate = payDate;
        }

        public Income(string name, double amount, int setDayOne, int setDayTwo, bool isPrimary, DateTime payDate)
        {
            Name = name;
            Amount = amount;
            DayOne = setDayOne;
            DayTwo = setDayTwo;
            IsPrimary = true;
            InvertedIsPrimary = !isPrimary;
            PayDate = payDate;
        }

        public Income(string name, double amount, bool isPrimary)
        {
            Name = name;
            Amount = amount;
            IsPrimary = isPrimary;
            InvertedIsPrimary = !isPrimary;
            PayDate = null;
        }
    }
}
