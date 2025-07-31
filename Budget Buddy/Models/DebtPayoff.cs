using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_Buddy.Models
{
    public class DebtPayoff
    {
        public string Name { set; get; }
        public double PrincipalBalance { set; get; }
        public double PayoffTime { set; get; }

        public DebtPayoff(string name, double principalBalance, double payoffTime)
        {
            Name = name;
            PrincipalBalance = principalBalance;
            PayoffTime = payoffTime;
        }
    }
}
