using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy.Models
{

    public class CalendarDay
    {

        public DateTime Date { get; set; }

        public ObservableCollection<Bill> Bills { get; set; } = new();
        public ObservableCollection<Income> Incomes { get; set; } = new();

        public CalendarDay(DateTime date)
        {
            Date = date;
        }
    }
}
