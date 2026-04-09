using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Budget_Buddy.Models;

namespace Budget_Buddy.Models
{
    public class DashboardData
    {
        public string UserName { get; set; }
        public double Balance { get; set; }
        public bool SavingsPaid { get; set; }
        public bool DebtPaid { get; set; }
        public double SavingsPercent { get; set; }
        public double DebtPercent { get; set; }
        public double PrimaryIncomeAmount { get; set; }
        public DateTime PrimaryPayday { get; set; }
        public string PayFrequency { get; set; }
        public int PrimaryIncomeId { get; set; }
        public int SetDayOne { get; set; }
        public int SetDayTwo { get; set; }

        public ObservableCollection<Debt> Debts { get; set; } = new ObservableCollection<Debt>();
        public ObservableCollection<Bill> TempBills { get; set; } = new ObservableCollection<Bill>();
        public ObservableCollection<Bill> RecurringBills { get; set; } = new ObservableCollection<Bill>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Income> Incomes { get; set; } = new ObservableCollection<Income>();
        public ObservableCollection<Bill> CurrentPeriodBills { get; set; } = new ObservableCollection<Bill>();
    }
}
