using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget_Buddy.Models;
using Budget_Buddy.Viewmodels;
using Microsoft.Data;
using Microsoft.Data.SqlClient;

namespace Budget_Buddy
{
    public static class DBHandler
    {
        static string connectionString = "Server=budget-buddy.cn840u6ao3k2.us-east-1.rds.amazonaws.com;Database=BudgetBuddy;User Id=exoticekko;Password=HelplessCarp477&;Encrypt=False";
        static SqlConnection connection = new SqlConnection(connectionString);
        public static void TestConnection()
        {

            string insertString = "INSERT INTO Users VALUES (0, 'alecfletcher', 'password123')";
            SqlCommand sqlCommand = new SqlCommand(insertString, connection);
            sqlCommand.Connection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlCommand.Connection.Close();
        }

        public static List<int> CheckUsernameAndPassword(string username, string password)
        {
            List<int> result = new List<int>();
            if(username == null || password == null )
            {
                return result;
            }
            SqlCommand command = new SqlCommand($"SELECT UserID FROM Users WHERE Username = @InputUsername AND Password = @InputPassword COLLATE SQL_Latin1_General_CP1_CS_AS", connection);
            command.Parameters.AddWithValue("@InputUsername", username);
            command.Parameters.AddWithValue("@InputPassword", password);
            command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetInt32(0));
            }
            reader.Close();
            command.Connection.Close();
            return result;
        }

        public static void UpdateIncomeAndFrequency(int userId, int frequency, int income, DateTime recentPayday)
        {
            SqlCommand command = new SqlCommand("UPDATE UserPreferences SET Income = @income, PayFrequency = @frequency, CurrentPayday = @recentPayday WHERE UserID = @userId", connection);
            command.Parameters.AddWithValue("@income", income);
            command.Parameters.AddWithValue("@frequency", frequency);
            command.Parameters.AddWithValue("@recentPayday", recentPayday);
            command.Parameters.AddWithValue("@userId", userId);
            command.Connection.Open();
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static void UpdatePreferences(int userId, int savingsPercent, int debtPercent)
        {
            SqlCommand command = new SqlCommand("UPDATE UserPreferences SET SavingsPercent = @savepercent, DebtPercent = @debtpercent WHERE UserID = @userId", connection);
            command.Parameters.AddWithValue("@savepercent", savingsPercent);
            command.Parameters.AddWithValue("@debtpercent", debtPercent);
            command.Parameters.AddWithValue("@userId", userId);
            command.Connection.Open();
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static void AddBill(int userId, string billName, double billPrice, int billDay)
        {
            SqlCommand command = new SqlCommand("INSERT INTO Bills VALUES(null, @userid, @billdue, null, @billprice, @billname)", connection);
            command.Parameters.AddWithValue("@userid", userId);
            command.Parameters.AddWithValue("@billdue", billDay);
            command.Parameters.AddWithValue("@billprice", billPrice);
            command.Parameters.AddWithValue("@billname", billName);
            command.Connection.Open();
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static void ChangePaidStatus(int billId, bool checkStatus)
        {
            SqlCommand command = new SqlCommand("UPDATE Bills SET Paid = @paid WHERE Bills.BillID = @billid", connection);
            command.Parameters.AddWithValue("@paid", checkStatus);
            command.Parameters.AddWithValue("@billid", billId);
            command.Connection.Open();
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        //public static DashboardViewModel GenerateDashboard(int userId)
        //{

        //    SqlCommand command = new SqlCommand($"SELECT Income, DebtPercent, SavingsPercent,CurrentPayday FROM UserPreferences WHERE UserID = @userid;", connection);
        //    command.Parameters.AddWithValue("@userid", userId);
        //    command.Connection.Open();
        //    SqlDataReader reader = command.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        DashboardViewModel dashboardViewModel = new DashboardViewModel(Convert.ToDouble(reader[0].ToString(), Convert.ToInt32(reader[1].ToString(), Convert.ToInt32(reader[2].ToString(), (DateTime)reader[3]);

        //        dashboardViewModel.Income = Convert.ToDouble(reader[0].ToString());
        //        dashboardViewModel.DebtAmount = Convert.ToInt32(reader[1].ToString());
        //        dashboardViewModel.SavingsAmount = Convert.ToInt32(reader[2].ToString());
        //        dashboardViewModel.CurrentPayperiod = (DateTime)reader[3];
        //    }
        //    reader.Close();
        //    command.Connection.Close();

        //    return dashboardViewModel;
        //    //SqlCommand command = new SqlCommand("SELECT  WHERE Bills.BillID = @billid", connection);
        //    //command.Parameters.AddWithValue("@paid", checkStatus);
        //    //command.Parameters.AddWithValue("@billid", billId);
        //    //command.Connection.Open();
        //    //command.ExecuteNonQuery();
        //    //command.Connection.Close();

        //    ////income billTotal debt savings
        //    //DashboardViewModel dashboardViewModel = new DashboardViewModel()
        //}

        public static void GenerateBills(int userId, DateTime firstDay, DateTime lastDay)
        {
            Bill.BillList.Clear();
            SqlCommand command = new SqlCommand("SELECT DueDate, Price, BillName, Paid FROM Bills WHERE UserID = @userid AND DueDate >= @firstday AND DueDate <= @lastday", connection);

            if (firstDay.Month != lastDay.Month)
            {
                command = new SqlCommand("SELECT DueDate, Price, BillName, Paid FROM Bills WHERE UserID = @userid AND (DueDate >= @firstday OR DueDate <= @lastday)", connection);
            }
            command.Parameters.AddWithValue("@userid", userId);
            command.Parameters.AddWithValue("@firstday", firstDay.Day);
            command.Parameters.AddWithValue("@lastday", lastDay.Day);
            command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while(reader.Read())
            {
                Bill bill = new Bill(reader[2].ToString(), Convert.ToDouble(reader[1]), Convert.ToInt32(reader[0]), reader[3].ToString() == "1");
                Bill.BillList.Add(bill);
            }
            command.Connection.Close();
        }
    }
}
