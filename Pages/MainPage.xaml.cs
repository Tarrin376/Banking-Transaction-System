using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ATM_System.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Models.IPageFunctions
    {
        /// <summary>
        /// All of the static attributes that will be used throughout
        /// the execution of the MainPage class.
     
        /// --'CurrentButton'-- keeps track of the most recent button that was 
        /// pressed out of the 4 main buttons on the MainPage GUI.
        /// --'db'-- is used to allow direct connection to the 'DatabaseQueries' class
        /// so queries can be executed instantly.
        /// --'ActivityText'-- keeps track of the text that will be added to the
        /// 'recent activity' MainPage GUI list.
        /// --'ActivityButtons'-- is responsible of storing all of the 'recent activity'
        /// buttons so they can be accessed.
        /// --'CheckWithdraw'-- Is an instance of the 'UserAmountChecks' class so
        /// values entered by the user can be checked.
        /// </summary>
        private static string CurrentButton = string.Empty;
        private static List<string> ActivityText = new();
        private readonly Button[] ActivityButtons;

        /// <summary>
        /// When this constructor is called, the "ActivityButtons" list is populated with buttons 
        /// so the recent activities on the GUI can be updated.
        /// It will also call the "OutputUserData" to show the most up-to-date information about the user.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            ActivityButtons = new Button[] { Activity1, Activity2, Activity3, Activity4, Activity5 };
            OutputUserData();
            DepositMoney.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        /// <summary>
        /// Both implemented interfaces from the 'Models.IPageFunction' interface
        /// MouseLeftClick allows the user to drag the page around.
        /// ExitCurrentPage allows the user to close the current page.
        /// MinimizeCurrentPage allows the user to minimize the page during use.
        /// </summary>
        /// <param name="sender">Is the mouse object being passed into the method</param>
        /// <param name="e">Is the certain event that was raised</param>
        public void MouseLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        public void ExitCurrentPage(object sender, RoutedEventArgs e) =>
            Close();
        public void MinimizeCurrentPage(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        /// <summary>
        /// This 'OutputUserData' method is called every time the user's data changes.
        /// For example, if the user changes their withdraw limit, this will be called.
        /// It changes the user's information by changing a certain key's value in the
        /// 'UserDetails' dictionary contained in the 'DatabaseQueries' class.
        /// </summary>
        private void OutputUserData()
        {
            CardNumber.Text = Models.DatabaseQueries.UserDetails["CreditCard"];
            Email.Text = Models.DatabaseQueries.UserDetails["EmailAddress"];

            CurrentBalance.Text = string.Format("£{0:n}", 
                Models.DatabaseQueries.UserDetails["CurrentBalance"]);
            AmountWithDrawn.Text = string.Format("£{0:n}", 
                Models.DatabaseQueries.UserDetails["WithDrawn"]);
            AmountTransferred.Text = string.Format("£{0:n}", 
                Models.DatabaseQueries.UserDetails["Transferred"]);
            WithdrawLimitText.Text = string.Format("£{0:n}",
                Models.DatabaseQueries.UserDetails["WithdrawLimit"]);

            if (AmountBox.Text != string.Empty) AddRecentActivity(AmountWarning.Text);
            AmountBox.Text = CardNumberBox.Text = string.Empty;
        }

        /// <summary>
        /// This method is responsible for adding the most recent activity to the 
        /// 'recent activity' elements on the main page GUI. It will loop through each button
        /// checking the contents and overriding the content with a new activity if it is empty.
        /// I have used a list data structure called 'ActivityText' that works similarly to a 
        /// stack data structure.
        /// </summary>
        /// <param name="ActivityToAdd"></param>
        private void AddRecentActivity(string ActivityToAdd)
        {
            if (ActivityText.Count > 7) ActivityText.RemoveAt(ActivityText.Count - 1);
            ActivityText.Insert(0, ActivityToAdd);

            int Index = 0;
            foreach (Button button in ActivityButtons)
            {
                if (Index < ActivityText.Count)
                {
                    button.Content = $"Date: ({DateTime.UtcNow.ToLongDateString()})\nActivity: {ActivityText[Index]}.";
                    Index++;
                }
            }
        }

        /// <summary>
        /// The purpose of this method is to increase the thickness of the border of the button that was pressed by the user. 
        /// The buttons in the list are the main buttons that the user can press on the 'MainPage.xaml' GUI. This includes
        /// "Transfer, Withdraw, Deposit, Set withdraw limit" etc. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonActionsClick(object sender, RoutedEventArgs e)
        {
            Button[] ActionButtons = { DepositMoney, ExtractMoney, TransferMoney, WithDrawLimit };
            AmountWarning.Text = string.Empty;
            CurrentButton = (string)(sender as Button).Content;

            if (CurrentButton.Equals("Transfer money") == false)
            {
                if (CurrentButton.Equals("Deposit money")) MoneyLabel.Text = "Deposit";
                else if (CurrentButton.Equals("Withdraw money")) MoneyLabel.Text = "Withdraw";
                else MoneyLabel.Text = "Set limit";
                CardNumberText.Visibility = CardNumberBox.Visibility = Visibility.Hidden;
            }
            else
            {
                MoneyLabel.Text = "Transfer";
                CardNumberText.Visibility = CardNumberBox.Visibility = Visibility.Visible;
            }

            foreach (Button button in ActionButtons)
            {
                if ((string)button.Content == CurrentButton) button.BorderThickness = new Thickness(4);
                else button.BorderThickness = new Thickness(1);
            }
        }

        /// <summary>
        /// This method identifies which button the user has hovered/clicked when the submit button was clicked.
        /// The program will run specific methods depending on the button that the user clicked e.g. deposit 
        /// money button. The methods called with Asynchronously perform the action that the user requests.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SubmitButtonClick(object sender, RoutedEventArgs e)
        {
            ClearAllWarning.Text = string.Empty;
            if (CurrentButton.Equals("Deposit money")) DepositAttemptAsync();
            else if (CurrentButton.Equals("Withdraw money")) WithdrawAttemptAsync();
            else if (CurrentButton.Equals("Transfer money")) TransferAttemptAsync();
            else SettingWithdrawLimit();
        }

        /// <summary>
        /// This method is responsible for depositing money into the user's account. It does various checks
        /// on the user's amount input using the "CheckDepositAmount" in the "UserAmountChecks" class
        /// to make sure that they have enough and it is in a valid currency format.
        /// If the input is valid, the "DepositMoneyAsync" method inside of the "DatabaseQueries" class is called.
        /// This will return a value telling the program if the operation of depositing money was successful or not.
        /// </summary>
        private async void DepositAttemptAsync()
        {
            AmountWarning.Text = Models.UserAmountChecks.CheckDepositAmount(AmountBox.Text);
            if (AmountWarning.Text.Equals(string.Empty))
            {
                try
                {
                    var result = await new Models.DatabaseQueries().DepositMoneyAsync(Models.DatabaseQueries.UserDetails["CreditCard"], 
                        decimal.Parse(AmountBox.Text));
                    await Task.Delay(200); // Waits 200 milliseconds so the 'DepositeMoneyAsync' can finish.

                    if (result.Equals(true))
                    {
                        AmountWarning.Text = $"Successfully deposited £{AmountBox.Text}";
                        AmountWarning.Foreground = Brushes.Green;
                        ShowChangedBalance(AmountBox.Text, "Deposit");
                    }
                    else
                    {
                        AmountWarning.Text = "An error occured";
                        AmountWarning.Foreground = Brushes.Red;
                    }
                }
                catch (Exception UserOrServerError)
                {
                    ConnectionErrorPage ErrorPage = new();
                    ErrorPage.Show();
                    ErrorPage.CauseOfErrorMessage(UserOrServerError.Message);
                }
            }
            else AmountWarning.Foreground = Brushes.Red;
        }

        /// <summary>
        /// This method is responsible for allowing the user to withdraw money from their account. Multiple checks are 
        /// performed to verify the validity of the user's amount input using the "CheckWithdrawAmount" method in 
        /// the "UserAmountChecks". If the input is valid, the program will then attempt to perform the action
        /// using the "WithdrawMoneyAsync" method contained in the "DatabaseQueries" class.
        /// class.
        /// </summary>
        private async void WithdrawAttemptAsync()
        {
            AmountWarning.Text = Models.UserAmountChecks.CheckWithdrawAmount(AmountBox.Text,
                Models.DatabaseQueries.UserDetails["CurrentBalance"]); ;

            if (AmountWarning.Text.Equals(string.Empty))
            {
                try
                {
                    var result = await new Models.DatabaseQueries().WithdrawMoneyAsync(CardNumber.Text, 
                        decimal.Parse(AmountBox.Text));
                    await Task.Delay(200); // Waits 200 milliseconds so the 'WithdrawMoneyAsync' can finish.

                    if (result.Equals(true))
                    {
                        AmountWarning.Text = $"Successfully withdrawn £{AmountBox.Text}";
                        AmountWarning.Foreground = Brushes.Green;
                        ShowChangedBalance(AmountBox.Text, "Withdraw");
                    }
                    else
                    {
                        AmountWarning.Text = "An error occured";
                        AmountWarning.Foreground = Brushes.Red;
                    }
                }
                catch (Exception UserOrServerError)
                {
                    ConnectionErrorPage ErrorPage = new();
                    ErrorPage.Show();
                    ErrorPage.CauseOfErrorMessage(UserOrServerError.Message);
                }
            }
            else AmountWarning.Foreground = Brushes.Red;
        }
        
        /// <summary>
        /// This method is responsible for allowing the user to transfer money from their current bank account into another account on the
        /// system. It checks the user's credit card that they are going to send money to and the amount inputted using the "CheckTransferAmount"
        /// in the "UserAmountChecks" class. If the inputs are valid, the program will then attempt to perform the action using the 
        /// "TransferMoneyAsync" method contained in the "DatabaseQueries" class.
        /// </summary>
        private async void TransferAttemptAsync()
        {
            AmountWarning.Text = Models.UserAmountChecks.CheckTransferAmount(AmountBox.Text,
                Models.DatabaseQueries.UserDetails["CurrentBalance"], CardNumberBox.Text); 

            if (AmountWarning.Text.Equals(string.Empty))
            {
                try
                {
                    var result = await new Models.DatabaseQueries().TransferMoneyAsync(CardNumber.Text, CardNumberBox.Text, 
                        decimal.Parse(AmountBox.Text));
                    await Task.Delay(200); // Waits 200 milliseconds so the 'TransferMoneyAsync' can finish.

                    if (result.Equals(true))
                    {
                        AmountWarning.Text = $"£{AmountBox.Text} " +
                            $"to [{CardNumberBox.Text}] (transfer)";
                        AmountWarning.Foreground = Brushes.Green;
                        ShowChangedBalance(AmountBox.Text, "Transfer");
                    }
                    else
                    {
                        AmountWarning.Text = "Credit card number is invalid. Make sure you are connected to the internet!";
                        AmountWarning.Foreground = Brushes.Red;
                    }
                }
                catch (Exception UserOrServerError)
                {
                    ConnectionErrorPage ErrorPage = new();
                    ErrorPage.Show();
                    ErrorPage.CauseOfErrorMessage(UserOrServerError.Message);
                }
            }
            else AmountWarning.Foreground = Brushes.Red;
        }

        /// <summary>
        /// This method is responsible for allowing the user to set a withdraw limit. It checks the user's amount input using the 
        /// "CheckWithdrawLimit" method in the "UserAmountChecks" class. If the inputs are valid, the program will then attempt 
        /// to perform the action using the UpdateWithdrawLimit" method contained in the "DatabaseQueries" class.
        /// </summary>
        private async void SettingWithdrawLimit()
        {
            AmountWarning.Text = Models.UserAmountChecks.CheckWithdrawLimit(AmountBox.Text);
            if (AmountWarning.Text.Equals(string.Empty))
            {
                try
                {
                    var UpdateLimit = await new Models.DatabaseQueries().UpdateWithdrawLimit(CardNumber.Text, 
                        decimal.Parse(AmountBox.Text));
                    await Task.Delay(200); // Waits 200 milliseconds so the 'UpdateWithdrawLimit' can finish.

                    if (UpdateLimit.Equals(true))
                    {
                        AmountWarning.Text = $"Withdraw limit is now set to: £{AmountBox.Text}";
                        AmountWarning.Foreground = Brushes.Green;
                        ShowChangedBalance(AmountBox.Text);
                    }
                    else
                    {
                        AmountWarning.Text = "An error occured";
                        AmountWarning.Foreground = Brushes.Red;
                    }
                }
                catch (Exception UserOrServerError)
                {
                    ConnectionErrorPage ErrorPage = new();
                    ErrorPage.Show();
                    ErrorPage.CauseOfErrorMessage(UserOrServerError.Message);
                }
            }
            else AmountWarning.Foreground = Brushes.Red;
        }

        /// <summary>
        /// The purpose of this method is to constantly update the user's details such as their current balance, amount transferred,
        /// amount withdrawn and so on. This method is used to update the database to the most recent values so the user can perform 
        /// money related actions without issues. Inside of the "DatabaseQueries" class, a dictionary called "UserDetails" is responsible 
        /// for holding the most up-to-date information about the user. 
        /// 
        /// The "OutputUserData" method is then called at the end of the method so the data can be updated when the user performs a 
        /// money related action.
        /// </summary>
        /// <param name="Amount"></param>
        /// <param name="ActionType"></param>
        private void ShowChangedBalance(string Amount, string ActionType=null)
        {
            if (Amount == string.Empty)
            {
                AmountWarning.Text = string.Empty;
                return;
            }
            switch (ActionType)
            {
                case "Transfer":
                    Models.DatabaseQueries.UserDetails["CurrentBalance"] = Convert.ToString(
                        Math.Round(decimal.Parse(Models.DatabaseQueries.UserDetails["CurrentBalance"]) - decimal.Parse(Amount), 2));
                    Models.DatabaseQueries.UserDetails["Transferred"] = Convert.ToString(
                        Math.Round(decimal.Parse(Amount) + decimal.Parse(Models.DatabaseQueries.UserDetails["Transferred"])));
                    break;

                case "Deposit":
                    Models.DatabaseQueries.UserDetails["CurrentBalance"] = Convert.ToString(
                        Math.Round(decimal.Parse(Amount) + decimal.Parse(Models.DatabaseQueries.UserDetails["CurrentBalance"]), 2));
                    break;

                case "Withdraw":
                    Models.DatabaseQueries.UserDetails["WithDrawn"] = Convert.ToString(
                        Math.Round(decimal.Parse(Amount) + decimal.Parse(Models.DatabaseQueries.UserDetails["WithDrawn"]), 2));
                    Models.DatabaseQueries.UserDetails["CurrentBalance"] = Convert.ToString(
                        Math.Round(decimal.Parse(Models.DatabaseQueries.UserDetails["CurrentBalance"])
                        - decimal.Parse(Amount), 2));
                    break;

                default:
                    Models.DatabaseQueries.UserDetails["WithdrawLimit"] = Convert.ToString(Math.Round(decimal.Parse(Amount), 2));
                    break;
            }

            OutputUserData(); // This method is used to display the updated data for the user.
        }

        /// <summary>
        /// This method is responsible for clearing all of the activities displayed on the "MainPage.xaml" GUI when the user presses
        /// the "Clear all activities" button on the UI. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearActivities(object sender, RoutedEventArgs e) 
        {
            try
            {
                ActivityText = new List<string>();
                if (ActivityButtons.Where(x => x.Content.Equals("(No activity)")).Count() < ActivityButtons.Length)
                {
                    foreach (Button button in ActivityButtons) button.Content = "(No activity)";
                    ClearAllWarning.Text = string.Empty;
                }
                else
                {
                    ClearAllWarning.Text = "Recent activites are already empty.";
                }
            }
            catch (Exception)
            {
                ClearAllWarning.Text = "There was an error clearing your activity. Try again!";
            }
        }
    }
}
