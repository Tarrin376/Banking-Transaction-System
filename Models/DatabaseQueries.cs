using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net.NetworkInformation;

namespace ATM_System.Models
{
    public class DatabaseQueries
    {
        /// <summary>
        /// All of the fields that are used to construct the database connection string
        /// so the user can connect the the database. 
        /// </summary>
        private const string Server = "server=127.0.0.1;";
        private const string UserID = "uid=root;";
        private const string Password = "pwd=cremmy flemmy123;";
        private const string Database = "database=atmdb;";

        /// <summary>
        /// This is the connection string that is used to connect the the db.
        /// The 'Successfullogin' static field is used to check if the login 
        /// was successful or not.
        /// The 'SuccessfulSignUp' static field is used to check if the sign up
        /// was successful or not.
        /// The 'UserDetails' is used to keep a temporary record of the user's data.
        /// </summary>
        private static MySqlConnection ConnectionString;
        public static bool SuccessfulLogin = false;
        public static bool SuccessfulSignUp = true;
        public static Dictionary<string, string> UserDetails = new();

        internal DatabaseQueries() => ConnectToDatabase();

        /// <summary>
        /// The purpose of this method is to attempt to connect to the MySQL database.
        /// After the connection string is constructed, the database connection is then
        /// opened.
        /// </summary>
        public static void ConnectToDatabase()
        {
            try
            {
                ConnectionString = new($"{Server}{UserID}{Password}{Database}");
                ConnectionString.OpenAsync();
                new Ping().Send("www.google.com");
            }
            catch (MySqlException DatabaseError)
            {
                throw new Exception("- Database or Server error.", DatabaseError);
            }
            catch (PingException ConnectionError)
            {
                throw new Exception("- Internet Connection error.", ConnectionError);
            }
        }

        /// <summary>
        /// This method attempts to create an account for the user by calling the
        /// 'SignUpAttempt' function asynchronously.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="PinNumber"></param>
        /// <param name="Email"></param>
        public async Task AddNewAccountAsync(string CardNumber, string PinNumber, 
            string Email)
        {
            var attempt = await Task.Run(() => SignUpAttempt(CardNumber, PinNumber, Email));
            if (attempt.Equals(true))
            {
                await Task.Run(() => AddAccountToDB(CardNumber, PinNumber, Email));
            }
        }

        /// <summary>
        /// This whole method is used to query the 'customer' database in MySQL to make
        /// sure that the user's credentials aren't already in the db.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="PinNumber"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
        private static bool SignUpAttempt(string CardNumber, string PinNumber, string Email)
        {
            using (MySqlCommand command = new
                ("SELECT CreditCard, EmailAddress FROM customer", ConnectionString))
            {
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString("CreditCard") == CardNumber 
                        || reader.GetString("EmailAddress") == Email) 
                        SuccessfulSignUp = false;
                }
                reader.Close();
            }
            return SuccessfulSignUp;
        }

        /// <summary>
        /// The purpose of this method is to add the account to the MySQL database
        /// if the credentials don't already exist.
        /// </summary>
        private static readonly Action<string, string, string> AddAccountToDB = 
            (string CardNumber, string PinNumber, string Email) =>
        {
            MySqlCommand AddAccount = new("INSERT INTO customer " +
                "(CreditCard, PinNumber, EmailAddress, CurrentBalance, WithDrawn, " +
                "Transferred, WithdrawLimit)" +
                $"VALUES ('{CardNumber}', '{PinNumber}', '{Email}', '0', '0', '0', '0')",
                ConnectionString);

            AddAccount.ExecuteReader();
        };

        /// <summary>
        /// This method asynchronously calls the 'LogInAttemp' method to see if the 
        /// user's login is currently in the database.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="PinNumber"></param>
        public async Task LoginRequestAsync(string CardNumber, string PinNumber) =>
            await Task.Run(() => LogInAttempt(CardNumber, PinNumber));
        
        /// <summary>
        /// This method queries the database and checks whether the user's credit card number
        /// and pin number are in the MySQL db. If they are, the user is logged in.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="PinNumber"></param>
        /// <returns></returns>
        private static void LogInAttempt(string CardNumber, string PinNumber)
        {
            try
            {
                using MySqlCommand command = new("SELECT * FROM customer", ConnectionString);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetString("CreditCard") == CardNumber
                        && reader.GetString("PinNumber") == PinNumber)
                    {
                        SuccessfulLogin = true;
                        UserDetails = new Dictionary<string, string>()
                        {
                            { "CreditCard",  reader.GetString("CreditCard") },
                            { "EmailAddress", reader.GetString("EmailAddress") },
                            { "CurrentBalance", reader.GetString("CurrentBalance") },
                            { "WithDrawn", reader.GetString("WithDrawn") },
                            { "Transferred", reader.GetString("Transferred") },
                            { "WithdrawLimit", reader.GetString("WithdrawLimit") }
                        };
                    }
                }
                reader.Close();
            }
            catch (Exception)
            {
                SuccessfulLogin = false;
            }
        }

        /// <summary>
        /// The purpose of this method and delegate is to deposit money into the user's account.
        /// It does multiple checks such as Making sure the connection and the credit
        /// card is valid etc. It will return true if the deposite was successful and false
        /// if it was not.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<bool> DepositMoneyAsync(string CardNumber, decimal Amount) =>
            await Task.Run(() => CheckDeposit(CardNumber, Amount));
        
        private readonly Func<string, decimal, bool> CheckDeposit = (string CardNumber, decimal Amount) =>
        {
            try
            {
                string UpdateBalance = $"UPDATE customer SET CurrentBalance = CurrentBalance + " 
                + $"'{Amount}' WHERE CreditCard = '{CardNumber}'";

                MySqlCommand Deposit = new(UpdateBalance, ConnectionString);
                Deposit.ExecuteNonQuery();
                Deposit.Dispose();

                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        };

        /// <summary>
        /// This method and delegate are used to transfer money from the user's account into another
        /// registered user on the program. It makes sure that the amount and the reciever
        /// card number is valid before querying the db asynchronously.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="RecieverCardNumber"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<bool> TransferMoneyAsync(string CardNumber, string RecieverCardNumber, decimal Amount) =>
            await Task.Run(() => CheckTransfer(CardNumber, RecieverCardNumber, Amount));
        
        private readonly Func<string, string, decimal, bool> CheckTransfer =
            (string SenderCardNumber, string RecieverCardNumber, decimal Amount) =>
        {
            try
            {
                bool FoundRecieverCard = false;
                MySqlCommand TransferMoney = new("SELECT CreditCard FROM customer", ConnectionString);
                MySqlDataReader reader = TransferMoney.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetString("CreditCard").Equals(RecieverCardNumber))
                    {
                        FoundRecieverCard = true;
                        break;
                    }
                }
                reader.Close();
                if (FoundRecieverCard == false) return false;

                TransferMoney.CommandText = $"UPDATE customer SET CurrentBalance = CurrentBalance + '{Amount}' " +
                    $"WHERE CreditCard = '{RecieverCardNumber}'";
                TransferMoney.ExecuteNonQuery();

                TransferMoney.CommandText = $"UPDATE customer SET CurrentBalance = CurrentBalance - '{Amount}', " +
                    $"Transferred = Transferred + '{Amount}' WHERE CreditCard = '{SenderCardNumber}'";
                TransferMoney.ExecuteNonQuery();
                TransferMoney.Dispose();

                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        };

        /// <summary>
        /// This method and delegate are used to withdraw money from the user's account. It will reduce
        /// the user's current balance and add the withdraw amount to the amount withdrawn
        /// GUI element. This is done by query and updating columns in the MySQL db.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<bool> WithdrawMoneyAsync(string CardNumber, decimal Amount) =>
            await Task.Run(() => CheckWithdraw(CardNumber, Amount));
        
        private readonly Func<string, decimal, bool> CheckWithdraw = (string CardNumber, decimal Amount) =>
        {
            try
            {
                string Withdraw = "UPDATE customer " +
                $"SET WithDrawn = WithDrawn + '{Amount}', " +
                $"CurrentBalance = CurrentBalance - '{Amount}' " +
                $"WHERE CreditCard = '{CardNumber}'";

                MySqlCommand WithdrawMoney = new(Withdraw, ConnectionString);
                WithdrawMoney.ExecuteNonQuery();
                WithdrawMoney.Dispose();

                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        };

        /// <summary>
        /// The purpose of this method and delegate is to update the user's current withdraw
        /// limit that they can set for themeselves. It does this by replacing the current
        /// column in the user account's row in the MySQL database. This value will then
        /// be used to restrict the user from withdrawing over a certain amount.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<bool> UpdateWithdrawLimit(string CardNumber, decimal Amount) =>
            await Task.Run(() => CheckLimit(CardNumber, Amount));
        
        private readonly Func<string, decimal, bool> CheckLimit = (string CardNumber, decimal Amount) =>
        {
            try
            {
                string UpdateLimitQuery = $"UPDATE customer SET WithdrawLimit = '{Amount}' " +
                $"WHERE CreditCard = '{CardNumber}'";

                MySqlCommand UpdateLimit = new(UpdateLimitQuery, ConnectionString);
                UpdateLimit.ExecuteNonQuery();
                UpdateLimit.Dispose();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        };
    }
}
