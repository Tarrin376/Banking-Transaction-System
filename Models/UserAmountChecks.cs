using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATM_System.Models
{
    public static class UserAmountChecks
    {
        /// <summary>
        /// These private fields are used to define the user's current
        /// maximum values when performing various operations such as depositing,
        /// withdrawing, transferring and setting a withdraw limit. 
        /// </summary>
        private readonly static int MaximumLimit = 2000;
        private readonly static int MaximumDeposit = 5000;
        private readonly static int MaximumTransfer = 3000;

        /// <summary>
        /// This property is used to update the maximum withdraw value from the user.
        /// Every time the limit is updated or when the user re-loads the program, it will
        /// update or remember the current limit.
        /// </summary>
        private static decimal MaxmimumWithdraw
        {
            get
            {
                if (decimal.Parse(DatabaseQueries.UserDetails["WithdrawLimit"]).Equals(0)) return MaximumLimit;
                else return decimal.Parse(DatabaseQueries.UserDetails["WithdrawLimit"]);
            }
        }

        /// <summary>
        /// This method is used to check the user's input into the deposite box.
        /// It makes sure that the input is not over the current 'MaximumDeposit' value.
        /// The return value will be a string stating if the user's value was valid or not
        /// and it will be used by the 'MainPage.xaml.cs' class in the 'DepositAttemptAsync'
        /// method.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        internal static string CheckDepositAmount(string amount) => GeneralValueChecks(amount, MaximumDeposit);
 
        /// <summary>
        /// The purpose of this method is to check the user's input into the withdrawn box.
        /// It will return a value stating whether the user's input was valid or not which
        /// will then be used by the 'MainPage.xaml.cs' class in the 'WithdrawAttemptAsync'
        /// method.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="CurrentBalance"></param>
        /// <returns></returns>
        internal static string CheckWithdrawAmount(string amount, string CurrentBalance)
        {
            string ValueCheck = GeneralValueChecks(amount, MaxmimumWithdraw);
            if (ValueCheck != string.Empty) return ValueCheck;

            if (decimal.Parse(amount) > decimal.Parse(DatabaseQueries.UserDetails["CurrentBalance"])) 
               return $"Withdraw amount '{amount}' exceeds current balance in your account";
            return string.Empty;
        }

        /// <summary>
        /// The purpose of this method is to check the user's input into the transfer money box.
        /// It will return a value stating whether the user's input into the box was valid or not.
        /// This value will then be used in the 'MainPage.xaml.cs' class in the 'TransferAttemptAsync'
        /// method.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="CurrentBalance"></param>
        /// <param name="CardNumber"></param>
        /// <returns></returns>
        internal static string CheckTransferAmount(string amount, string CurrentBalance,
            string CardNumber)
        {
            string ValueCheck = GeneralValueChecks(amount, MaximumTransfer);
            if (ValueCheck != string.Empty) return ValueCheck;

            if (decimal.Parse(amount) > decimal.Parse(CurrentBalance)) return $"Transfer amount £'{amount}' exceeds current balance in your account.";
            if (CardNumber == DatabaseQueries.UserDetails["CreditCard"]) return $"You cannot send money to your own card.";

            if (new CreateAccount(CardNumber).CheckCardNumber == "Invalid") return $"Invalid card number: '{CardNumber}'.";
            return string.Empty;
        }

        /// <summary>
        /// This method is used to check the user's input in the set limit box on the main page
        /// GUI. It will return a value that will be used by the 'MainPage.xaml.cs' in the
        /// 'SettingWithdrawLimit' method. The return value is used to check if the user's 
        /// input was valid or not.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        internal static string CheckWithdrawLimit(string amount)
        {
            string ValueCheck = GeneralValueChecks(amount, MaximumLimit);
            if (ValueCheck != string.Empty) return ValueCheck;

            DatabaseQueries.UserDetails["WithdrawLimit"] = amount;
            return string.Empty;
        }

        /// <summary>
        /// The purpose of this method is to check general cases for real-life currency. For example,
        /// checking the currency format etc.
        /// This function runs in 0(1) time due to simple if-statements only being used.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="Maximum"></param>
        /// <returns></returns>
        private static string GeneralValueChecks(string amount, decimal Maximum)
        {
            if (decimal.TryParse(amount, out _) == false || decimal.Parse(amount) <= 0) return $"The value ' {amount} ' is not in a currency format.";
            if (decimal.Parse(amount) > Maximum) return $"The value '£{amount}' is over your £{Maximum} limit.";
            return string.Empty;
        }
    }
}
