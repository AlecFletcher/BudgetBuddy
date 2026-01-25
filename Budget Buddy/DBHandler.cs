using Budget_Buddy.Models;
using Bumptech.Glide.Load;
using Npgsql;
using NpgsqlTypes;
using System.Collections.ObjectModel;
using static Java.Util.Jar.Attributes;

namespace Budget_Buddy
{
    public static class DBHandler
    {
        static string postgresConnectionString = "Host=budgetbuddy.cn840u6ao3k2.us-east-1.rds.amazonaws.com,5432;Database=BudgetBuddy;Username=CRacomBiNiTzmArC;Password=rt7h+4lA>fhFK4n3;Database=budgetbuddy;";
        static NpgsqlDataSource dataSource = NpgsqlDataSource.Create(postgresConnectionString);

        public static void DisconnectFromDB()
        {
            NpgsqlConnection.ClearAllPools();
        }

        public static void ConnectToDB()
        {
            postgresConnectionString = "Host=budgetbuddy.cn840u6ao3k2.us-east-1.rds.amazonaws.com,5432;Database=BudgetBuddy;Username=CRacomBiNiTzmArC;Password=rt7h+4lA>fhFK4n3;Database=budgetbuddy;";
            dataSource = NpgsqlDataSource.Create(postgresConnectionString);
        }

        #region User Queries

        public static async Task AddUser(string Username, string Password, string Name)
        {
            await using (var command = dataSource.CreateCommand("INSERT INTO Users (username, password, firstname) VALUES(@username, @password, @name); INSERT INTO UserPreferences (savingspercent, debtpercent, savingsdollaramount, debtdollaramount, currentbalance, savingspaid, debtpaid) VALUES(null, null, 0, 0, 0, false, false);"))
            {
                command.Parameters.AddWithValue("@username", Username);
                command.Parameters.AddWithValue("@password", Password);
                command.Parameters.AddWithValue("@name", Name);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateUserLastLogin(int userId)
        {
            await using (var command = dataSource.CreateCommand("UPDATE Users SET last_login = '{" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "}' WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await command.ExecuteNonQueryAsync();
            }
        }
        #endregion
        public static async Task<int> CheckUsernameAndPassword(string username, string password)
        {
            int result = -1;
            if (username == null || password == null)
            {
                return result;
            }

            await using (var cmd = dataSource.CreateCommand("SELECT UserID FROM Users WHERE LOWER(Username) = LOWER(($1)) AND Password = ($2)"))
            {
                cmd.Parameters.AddWithValue(username);
                cmd.Parameters.AddWithValue(password);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result = reader.GetInt32(0);
                    }
                }
            }
            return result;
        }

        public static async Task AddIncome(int userid, bool isRecurring, DateTime payDate, string payFrequency, bool isPrimary, int? setDayOne, int? setDayTwo)
        {
            await using (var command = dataSource.CreateCommand("INSERT INTO Incomes (userid, isrecurring, paydate, payfrequency, isprimary, setdayone, setdaytwo) VALUES(@userid, @isrecurring, @paydate, @payfrequency, @isprimary @setdayone, @setdaytwo,);"))
            {
                command.Parameters.AddWithValue("@userid", userid);
                command.Parameters.AddWithValue("@isrecurring", isRecurring);
                command.Parameters.AddWithValue("@paydate", payDate);
                command.Parameters.AddWithValue("@payfrequency", payFrequency);
                command.Parameters.AddWithValue("@isprimary", isPrimary);
                if (setDayOne == null || setDayTwo == null)
                {
                    command.Parameters.AddWithValue("@setdayone", "null");
                    command.Parameters.AddWithValue("@setdaytwo", "null");
                }
                else
                {
                    command.Parameters.AddWithValue("@setdayone", setDayOne);
                    command.Parameters.AddWithValue("@setdaytwo", setDayTwo);
                }


                    await command.ExecuteNonQueryAsync();
            }
        }

        //UPDATE TO PUDATE BASED ON ID --DONE CHANGED
        public static async Task UpdatePayDay(int incomeid, DateTime currentPayday)
        {
            await using (var cmd = dataSource.CreateCommand("UPDATE incomes SET paydate = ($1) WHERE incomeid = ($2)"))
            {
                cmd.Parameters.AddWithValue(currentPayday);
                cmd.Parameters.AddWithValue(incomeid);
                cmd.ExecuteNonQuery();
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE CHANGED
        public static async Task UpdateIncomeAndFrequency(int userId, string frequency, int income, DateTime recentPayday)
        {
            await using (var cmd = dataSource.CreateCommand("UPDATE Incomes SET amount = ($1), payfrequency = ($2), paydate = ($3) WHERE UserID = ($4)"))
            {
                cmd.Parameters.AddWithValue(income);
                cmd.Parameters.AddWithValue(frequency);
                cmd.Parameters.AddWithValue(recentPayday);
                cmd.Parameters.AddWithValue(userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static async Task UpdatePreferences(int userId, int savingsPercent, int debtPercent)
        {
            await using (var command = dataSource.CreateCommand("UPDATE UserPreferences SET SavingsPercent = @savepercent, DebtPercent = @debtpercent WHERE UserID = @userId"))
            {
                command.Parameters.AddWithValue("@savepercent", savingsPercent);
                command.Parameters.AddWithValue("@debtpercent", debtPercent);
                command.Parameters.AddWithValue("@userId", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task AddBill(int userId, string billName, double billPrice, int billDay)
        {
            await using (var command = dataSource.CreateCommand("INSERT INTO Bills (userid, duedate, price, billname, paid, principalbalance) VALUES (@userid, @billdue, CAST(@billprice AS numeric), @billname, false, null)"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@billdue", billDay);
                command.Parameters.AddWithValue("@billprice", billPrice);
                command.Parameters.AddWithValue("@billname", billName);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task GenerateBills(int userId, DateTime firstDay, DateTime lastDay)
        {
            Bill.BillList.Clear();
            Bill.TempBillList.Clear();
            Bill.RecurringBillList.Clear();
            if (firstDay.Month != lastDay.Month)
            {
                await using (var command = dataSource.CreateCommand("SELECT BillID, BillName, Price, DueDate, Paid FROM Bills WHERE UserID = @userid AND (DueDate >= @firstday OR DueDate <= @lastday) ORDER BY DueDate ASC"))
                {
                    command.Parameters.AddWithValue("@userid", userId);
                    command.Parameters.AddWithValue("@firstday", firstDay.Day);
                    command.Parameters.AddWithValue("@lastday", lastDay.Day);
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Bill bill = new Bill(reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetInt32(3), reader.GetBoolean(4));
                            Bill.BillList.Add(bill);
                        }
                    }
                }
            }
            else
            {
                await using (var command = dataSource.CreateCommand("SELECT BillID, BillName, Price, DueDate, Paid FROM Bills WHERE UserID = @userid AND DueDate >= @firstday AND DueDate <= @lastday ORDER BY DueDate ASC"))
                {
                    command.Parameters.AddWithValue("@userid", userId);
                    command.Parameters.AddWithValue("@firstday", firstDay.Day);
                    command.Parameters.AddWithValue("@lastday", lastDay.Day);
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Bill bill = new Bill(reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetInt32(3), reader.GetBoolean(4));
                            Bill.BillList.Add(bill);
                        }
                    }
                }
            }
            await using (var command = dataSource.CreateCommand("SELECT BillID, BillName, Price, Paid FROM TempBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Bill bill = new Bill(reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetBoolean(3));
                        Bill.TempBillList.Add(bill);
                    }
                }
            }

            await using (var command = dataSource.CreateCommand("SELECT BillID, BillName, Price, Paid FROM RecurringBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Bill bill = new Bill(reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetBoolean(3));
                        Bill.RecurringBillList.Add(bill);
                    }
                }
            }
        }


        //UPDATE TO ACCOUNT FOR NEW TABLE --CHANGED DONE
        public static async Task<DateTime> GetPayday(int userId)
        {
            DateTime currentPayday = new DateTime();
            await using (var command = dataSource.CreateCommand($"SELECT paydate FROM incomes WHERE UserID = @userid AND isprimary = true"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    currentPayday = (DateTime)reader[0];
                }
                return currentPayday;
            }
        }


        //UPDATE TO ACCOUNT FOR NEW TABLE
        /*
        public static async Task UpdatePayFrequencyForSetDays(int userId)
        {
            int FirstDay;
            int SecondDay;
            DateTime NewPayday;
            DateTime NextPayday;
            int PayFrequency;

            await using (var command = dataSource.CreateCommand($"SELECT firstsetpayday, secondsetpayday FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    FirstDay = Convert.ToInt32(reader[0]);
                    SecondDay = Convert.ToInt32(reader[1]);
                }
            }

            if (DateTime.Now.Day < SecondDay && DateTime.Now.Day > FirstDay)
            {
                NewPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, FirstDay);
                NextPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, SecondDay);
            }
            else
            {
                NewPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, SecondDay);
                NextPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, FirstDay);
            }

            bool DayChanged = false;

            if((int)NewPayday.DayOfWeek == 0)
            {
                NewPayday = NewPayday.AddDays(-2);
                DayChanged = true;
            }
            if((int)NewPayday.DayOfWeek == 6)
            {
                NewPayday = NewPayday.AddDays(-1);
                DayChanged = true;
            }
            if ((int)NextPayday.DayOfWeek == 0)
            {
                NextPayday = NextPayday.AddDays(-2);
                DayChanged = true;
            }
            if ((int)NextPayday.DayOfWeek == 6)
            {
                NextPayday = NextPayday.AddDays(-1);
                DayChanged = true;
            }

            if (DayChanged && NextPayday.Day <= DateTime.Now.Day)
            {
                DateTime NewPaydayTemp = NewPayday;
                NewPayday = NextPayday;
                
                //Modified payday is beginning of month
                if(NewPayday.Day - FirstDay <= 2)
                {
                    NextPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, SecondDay);
                }
                else
                {
                    NextPayday = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, FirstDay);
                }

            }

            Console.WriteLine("Next Payday:" + NextPayday.ToString());
            PayFrequency = (int)Math.Abs(Math.Floor((NextPayday - NewPayday).TotalDays)) - 1;

            Console.WriteLine("Pay Frequency: " + PayFrequency.ToString());

            

            await using (var command = dataSource.CreateCommand("UPDATE UserPreferences SET firstsetpayday = @FirstDay, secondsetpayday = @SecondDay, currentpayday = @CurrentPayday, payfrequency = @PayFrequency WHERE UserID = @userId"))
            {
                command.Parameters.AddWithValue("@FirstDay", FirstDay);
                command.Parameters.AddWithValue("@SecondDay", SecondDay);
                command.Parameters.AddWithValue("@FirstDay", NewPayday);
                command.Parameters.AddWithValue("@PayFrequency", PayFrequency);
                command.Parameters.AddWithValue("@CurrentPayday", NewPayday);
                command.Parameters.AddWithValue("@userId", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        */


        //UPDATE TO ACCOUNT FOR NEW TABLE --CHANGED DONE
        public static async Task SetBiMonthlyPaydays(int userId, int firstDay, int secondDay)
        {
            int FirstDay = 0;
            int SecondDay = 0;

            if(firstDay < secondDay)
            {
                FirstDay = firstDay;
                SecondDay = secondDay;  
            }
            else
            {
                FirstDay = secondDay;
                SecondDay = firstDay;
            }
            await using (var command = dataSource.CreateCommand("UPDATE incomes SET setdayone = @FirstDay, setdaytwo = @SecondDay, payfrequency = 'BiMonthly' WHERE UserID = @userId"))
            {
                command.Parameters.AddWithValue("@FirstDay", FirstDay);
                command.Parameters.AddWithValue("@SecondDay", SecondDay);
                command.Parameters.AddWithValue("@userId", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE
        public static async Task<string> GetPayFrequency(int userId)
        {
            string result = "";
            await using (var command = dataSource.CreateCommand($"SELECT PayFrequency FROM incomes WHERE UserID = @userid AND isprimary = true"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    result = Convert.ToString(reader[0]);
                }
                return result;
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE
        public static async Task<List<int>> GetSetDays(int userId)
        {
            List<int> result = new List<int>();
            await using (var command = dataSource.CreateCommand($"SELECT setdayone, setdaytwo FROM Incomes WHERE UserID = @userid AND isprimary = true"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    result.Add(Convert.ToInt32(reader[0]));
                    result.Add(Convert.ToInt32(reader[1]));
                }
                return result;
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE
        public static async Task<List<double>> GetUserPreferences(int userId)
        {
            List<double> result = new List<double>();
            await using (var command = dataSource.CreateCommand($"SELECT incomes.amount, userpreferences.savingspercent, userpreferences.debtpercent, incomes.incomeid from incomes INNER JOIN userpreferences ON userpreferences.userid = incomes.userid where incomes.userid = @userid and isprimary = true "))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    result.Add(Convert.ToInt32(reader[0]));
                    result.Add(Convert.ToInt32(reader[1]));
                    result.Add(Convert.ToInt32(reader[2]));
                    result.Add(Convert.ToInt32(reader[3]));
                }
                return result;
            }
        }

        public static async Task<double> GetBillTotal(int userId)
        {
            int billTotal = 0;

            await using (var cmd = dataSource.CreateCommand("SELECT Price FROM Bills WHERE UserID = @userid"))
            {
                cmd.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        billTotal += Convert.ToInt32(reader[0]);
                    }
                }
            }
            return billTotal;
        }

        public static async Task<ObservableCollection<Bill>> GetAllBillsList(int userId)
        {
            ObservableCollection<Bill> bills = new ObservableCollection<Bill>();
            await using (var command = dataSource.CreateCommand($"SELECT BillID, BillName, Price, DueDate, Paid FROM Bills WHERE UserID = @userid AND PrincipalBalance IS NULL ORDER BY DueDate ASC"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                Bill.BillList.Clear();
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Bill bill = new Bill(Convert.ToInt32(reader[0]), reader[1].ToString(), Convert.ToDouble(reader[2]), Convert.ToInt32(reader[3]), reader[4].ToString() == "1");
                        bills.Add(bill);
                    }
                }

                return bills;
            }

        }

        public static async Task RemoveBill(Bill bill)
        {
            await using (var command = dataSource.CreateCommand("DELETE FROM Bills WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@billid", bill.BillID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateBIll(Bill bill)
        {
            await using (var command = dataSource.CreateCommand("UPDATE Bills SET DueDate = @duedate, Price = CAST(@price AS NUMERIC), BillName = @billname WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@duedate", bill.DueDay);
                command.Parameters.AddWithValue("@price", bill.Price);
                command.Parameters.AddWithValue("@billname", bill.Name);
                command.Parameters.AddWithValue("@billId", bill.BillID);
                await command.ExecuteNonQueryAsync();
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE
        public static async Task<int> GetIncome(int userId)
        {
            await using (var command = dataSource.CreateCommand("SELECT amount from Incomes WHERE UserID = @userid and isprimary = true"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    int value = Convert.ToInt32(reader[0]);

                    return value;
                }
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE
        public static async Task<int> GetMonthlyIncome(int userId)
        {
            await using (var command = dataSource.CreateCommand("select amount, payfrequency from incomes where userid = @userid and isprimary = true"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    int value = Convert.ToInt32(reader[0]);
                    string payFrequency = Convert.ToString(reader[1]);
                    int payMultiplier = 1;
                    switch (payFrequency)
                    {
                        case "Weekly":
                            payMultiplier = 4;
                                break;

                        case "BiWeekly":
                            payMultiplier = 2;
                            break;

                        case "BiMonthly":
                            payMultiplier = 2;
                            break;

                        case "Monthly":
                            payMultiplier = 1;
                            break;
                    }

                    value = value * payMultiplier;
                    return value;
                }
            }
        }

        //UPDATE TO ACCOUNT FOR NEW TABLE --DONE
        public static async Task UpdatePayFrequencyIndex(int userId, string payfrequency)
        {
            await using (var command = dataSource.CreateCommand("UPDATE incomes SET payfrequency = '@payfrequency' WHERE UserId = @userid AND isprimary = true"))
            {
                command.Parameters.AddWithValue("@payfrequency", payfrequency);
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> GetDebtPercent(int userId)
        {
            int result = 0;
            await using (var command = dataSource.CreateCommand($"SELECT DebtPercent FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while(await reader.ReadAsync())
                    {
                        result = Convert.ToInt32(reader[0]);
                    }

                }
                return result;
            }
        }

        public static async Task<int> GetSavingsPercent(int userId)
        {
            int result = 0;
            await using (var command = dataSource.CreateCommand($"SELECT SavingsPercent FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    result = Convert.ToInt32(reader[0]);
                    return result;
                }
            }
        }

        public static async Task UpdateDebtAndSavingsPercent(int userId, int savingsPercent, int debtPercent)
        {
            await using (var command = dataSource.CreateCommand("UPDATE UserPreferences SET DebtPercent = @debtpercent, SavingsPercent = @savingspercent WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@debtpercent", debtPercent);
                command.Parameters.AddWithValue("@savingspercent", savingsPercent);

                await command.ExecuteNonQueryAsync();
            } 
        }

        public static async Task UpdatePaidStatus(int? billId, bool isChecked)
        {
            await using (var command = dataSource.CreateCommand("UPDATE Bills SET Paid = @paid WHERE Bills.BillID = @billid"))
            {
                command.Parameters.AddWithValue("@paid", isChecked);
                command.Parameters.AddWithValue("@billid", billId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task ResetSavingsAndDebtPaid(int userId)
        {
            await using (var command = dataSource.CreateCommand("UPDATE UserPreferences SET savingspaid = false, debtpaid = false WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task RefreshPaidBills(int userId)
        {
            await using (var command = dataSource.CreateCommand("UPDATE Bills SET Paid = false WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> IsNewAccount(int userId)
        {
            bool result = false;

            await using (var cmd = dataSource.CreateCommand("SELECT Income, PayFrequency, SavingsPercent, DebtPercent, CurrentPayday FROM UserPreferences WHERE UserID = ($1)"))
            {
                cmd.Parameters.AddWithValue(userId);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (reader[0] == null || reader[1] == null || reader[2] == null || reader[3] == null || reader[4] == null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
                return result;
            }
        }

        public static async Task AddDebt(int userId, Debt debt)
        {
            await using (var command = dataSource.CreateCommand("INSERT INTO Bills (userid, duedate, price, billname, paid, principalbalance) VALUES (@userid, @billdue, CAST(@billprice AS numeric), @billname, false, @principal)"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@billdue", debt.DueDay);
                command.Parameters.AddWithValue("@billprice", NpgsqlDbType.Numeric, debt.Price);
                command.Parameters.AddWithValue("@billname", debt.Name);
                command.Parameters.AddWithValue("@principal", debt.PrincipalBalance);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveDebt(int? debtId)
        {
            await using (var command = dataSource.CreateCommand("DELETE FROM Bills WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@billid", debtId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateDebt(Debt debt)
        {
            await using (var command = dataSource.CreateCommand("UPDATE Bills SET DueDate = @duedate, Price = CAST(@price AS NUMERIC), BillName = @billname, PrincipalBalance = @principalbalance WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@duedate", debt.DueDay);
                command.Parameters.AddWithValue("@price", debt.Price);
                command.Parameters.AddWithValue("@billname", debt.Name);
                command.Parameters.AddWithValue("@billId", debt.BillID);
                command.Parameters.AddWithValue("@principalbalance", debt.PrincipalBalance);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<ObservableCollection<Debt>> GenerateDebtList(int userId)
        {
            ObservableCollection<Debt> debts = new ObservableCollection<Debt>();
            await using (var command = dataSource.CreateCommand($"SELECT BillID, BillName, Price, DueDate, PrincipalBalance FROM Bills WHERE UserID = @userid AND PrincipalBalance IS NOT NULL"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Debt debt = new Debt(Convert.ToInt32(reader[0]), reader[1].ToString(), Convert.ToDouble(reader[2]), Convert.ToInt32(reader[3]), Convert.ToDouble(reader[4]));
                        debts.Add(debt);
                    }
                }
                return debts;
            }
        }

        public static async Task<bool> DoesUsernameExist(string userName)
        {
            await using (var command = dataSource.CreateCommand($"SELECT Username FROM Users WHERE Username = @username"))
            {
                command.Parameters.AddWithValue("@username", userName);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        public static async Task<string> GetNameOfUser(int userId)
        {
            string result = "";
            await using (var command = dataSource.CreateCommand($"SELECT FirstName FROM Users WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    result = reader[0].ToString();
                    return result;
                }
            }
        }

        public static async Task<ObservableCollection<Bill>> GetAllBillsAndDebts(int userId)
        {
            Bill.BillList.Clear();
            ObservableCollection<Bill> bills = new ObservableCollection<Bill>();
            await using (var command = dataSource.CreateCommand($"SELECT BillID, BillName, Price, DueDate, Paid FROM Bills WHERE UserID = @userid ORDER BY DueDate ASC"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Bill bill = new Bill(Convert.ToInt32(reader[0]), reader[1].ToString(), Convert.ToDouble(reader[2]), Convert.ToInt32(reader[3]), reader[4].ToString() == "1");
                        bills.Add(bill);
                    }
                }
            }

            await using (var command = dataSource.CreateCommand($"SELECT BillID, BillName, Price, Paid FROM TempBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Bill bill = new Bill(reader.GetInt32(0), "(Temp bill) " + reader.GetString(1), reader.GetDouble(2), reader.GetBoolean(3));
                        bills.Add(bill);
                    }
                }
            }

            await using (var command = dataSource.CreateCommand($"SELECT BillID, BillName, Price, Paid FROM RecurringBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Bill bill = new Bill(reader.GetInt32(0), "(Recurring bill) " + reader.GetString(1), reader.GetDouble(2), reader.GetBoolean(3)); 
                        bills.Add(bill);
                    }
                }
            }
            return bills;
        }

        public static async Task UpdateBillAndDebtDollarAmount(int userId, double remainingBalance, double debtPercent, double savingsPercent)
        {

            double debtAmount = Math.Round(remainingBalance * debtPercent, 2);
            double savingsAmount = Math.Round(remainingBalance * savingsPercent, 2);
            await using (var command = dataSource.CreateCommand("UPDATE userpreferences SET savingsdollaramount = @savingsamount, debtdollaramount = @debtamount WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@savingsamount", savingsAmount);
                command.Parameters.AddWithValue("@debtamount", debtAmount);
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<double> GetDebtDollarAmount(int userId)
        {
            await using (var command = dataSource.CreateCommand($"SELECT debtdollaramount FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    double result = reader.GetDouble(0);
                    if(result < 0)
                    {
                        return 0;
                    }
                    return result;
                }
            }
        }

        public static async Task<double> GetSavingsDollarAmount(int userId)
        {
            await using (var command = dataSource.CreateCommand($"SELECT savingsdollaramount FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    double result = reader.GetDouble(0);
                    if (result < 0)
                    {
                        return 0;
                    }
                    return result;
                }
            }
        }

        public static async Task UpdateBalance(int userId, double balance)
        {
            double balanceToAdd = Math.Round(balance, 2);
            if(balanceToAdd < 0)
            {
                balanceToAdd = 0;
            }
            await using (var command = dataSource.CreateCommand("UPDATE userpreferences SET currentbalance = @balance WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@balance", balanceToAdd);
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<double> GetBalance(int userId)
        {
            await using (var command = dataSource.CreateCommand($"SELECT currentbalance FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    double result = reader.GetDouble(0);
                    if (result < 0)
                    {
                        return 0;
                    }
                    return result;
                }
            }
        }

        public static async Task UpdateDebtPaid(int userId, bool status)
        {
            await using (var command = dataSource.CreateCommand("UPDATE UserPreferences SET debtPaid = @currentstatus WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@currentstatus", status);
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateSavingsPaid(int userId, bool status)
        {
            await using (var command = dataSource.CreateCommand("UPDATE UserPreferences SET savingspaid = @currentstatus WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@currentstatus", status);
                command.Parameters.AddWithValue("@userid", userId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> GetDebtPaid(int userId)
        {
            await using (var command = dataSource.CreateCommand($"SELECT debtpaid FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    return reader.GetBoolean(0);
                }
            }
        }

        public static async Task<bool> GetSavingsPaid(int userId)
        {
            await using (var command = dataSource.CreateCommand($"SELECT savingspaid FROM UserPreferences WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    return reader.GetBoolean(0);
                }
            }
        }

        #region Temp Bills
        public static async Task AddTempBill(int userId, string billName, double billPrice)
        {
            await using (var command = dataSource.CreateCommand("INSERT INTO TempBills (userid, price, billname, paid) VALUES (@userid, CAST(@billprice AS numeric), @billname, false)"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@billprice", billPrice);
                command.Parameters.AddWithValue("@billname", billName);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateRecurringBIll(Bill bill)
        {
            await using (var command = dataSource.CreateCommand("UPDATE RecurringBills SET Price = CAST(@price AS NUMERIC), BillName = @billname WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@price", bill.Price);
                command.Parameters.AddWithValue("@billname", bill.Name);
                command.Parameters.AddWithValue("@billId", bill.BillID);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateTempBillPaidStatus(int? billId, bool isChecked)
        {
            await using (var command = dataSource.CreateCommand("UPDATE TempBills SET Paid = @paid WHERE BillID = @billid"))
            {
                command.Parameters.AddWithValue("@paid", isChecked);
                command.Parameters.AddWithValue("@billid", billId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<ObservableCollection<Bill>> GetAllTempBills(int userId)
        {
            ObservableCollection<Bill> bills = new ObservableCollection<Bill>();
            await using (var command = dataSource.CreateCommand($"SELECT BillId, Billname, price, paid FROM TempBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        Bill bill = new Bill(reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetBoolean(3));
                        bills.Add(bill);
                    }
                }
            }
            return bills;
        }
        #endregion


        #region Recurring Bills
        public static async Task AddRecurringBill(int userId, string billName, double billPrice)
        {
            await using (var command = dataSource.CreateCommand("INSERT INTO RecurringBills (userid, price, billname, paid) VALUES (@userid, CAST(@billprice AS numeric), @billname, false)"))
            {
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@billprice", billPrice);
                command.Parameters.AddWithValue("@billname", billName);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveRecurringBill(Bill bill)
        {
            await using (var command = dataSource.CreateCommand("DELETE FROM RecurringBills WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@billid", bill.BillID);

                await command.ExecuteNonQueryAsync();
            }
        }
        public static async Task UpdateTempBIll(Bill bill)
        {
            await using (var command = dataSource.CreateCommand("UPDATE TempBills SET Price = CAST(@price AS NUMERIC), BillName = @billname WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@price", bill.Price);
                command.Parameters.AddWithValue("@billname", bill.Name);
                command.Parameters.AddWithValue("@billId", bill.BillID);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveTempBill(Bill bill)
        {
            await using (var command = dataSource.CreateCommand("DELETE FROM TempBills WHERE BillID = @billId"))
            {
                command.Parameters.AddWithValue("@billid", bill.BillID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAllTempBills(int userId)
        {
            await using (var command = dataSource.CreateCommand("DELETE FROM TempBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdateRecurringBillPaidStatus(int? billId, bool isChecked)
        {
            await using (var command = dataSource.CreateCommand("UPDATE RecurringBills SET Paid = @paid WHERE BillID = @billid"))
            {
                command.Parameters.AddWithValue("@paid", isChecked);
                command.Parameters.AddWithValue("@billid", billId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<ObservableCollection<Bill>> GetAllRecurringBills(int userId)
        {
            ObservableCollection<Bill> bills = new ObservableCollection<Bill>();
            await using (var command = dataSource.CreateCommand($"SELECT BillId, Billname, price, paid FROM RecurringBills WHERE UserID = @userid"))
            {
                command.Parameters.AddWithValue("@userid", userId);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        Bill bill = new Bill(reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2), reader.GetBoolean(3));
                        bills.Add(bill);
                    }
                }
            }
            return bills;
        }
        #endregion
    }
}
