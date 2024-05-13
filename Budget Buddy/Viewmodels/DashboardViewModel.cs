using Budget_Buddy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy.Viewmodels
{
    public class DashboardViewModel
    {
        public double Income {  get; set; }
        public double IncomeAfterBills { get; set; }
        public double DebtAmount {  get; set; }
        public double SavingsAmount{  get; set; }
        public DateTime CurrentPayperiod { get; set; }

        public DashboardViewModel() { }
        public DashboardViewModel(double income, int debtpercent, int savingspercent, DateTime currentPayday, int payFrequency, List<Bill> billList)
        {
            this.Income = income;
            double billTotal = 0.00;
            foreach(Bill bill in billList)
            {
                billTotal += bill.Price;
            }
            if(billTotal > Income)
            {
                this.DebtAmount = 0;
                this.SavingsAmount = 0;
                this.CurrentPayperiod = currentPayday;
                return;
            }
            this.DebtAmount = (debtpercent / 100) * IncomeAfterBills;
            this.SavingsAmount = (savingspercent / 100) * IncomeAfterBills; ;
            this.CurrentPayperiod = currentPayday;
        }
    }
}
