using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ATM_System.Pages
{
    /// <summary>
    /// Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Models.IPageFunctions
    {
        public SignUpPage() => InitializeComponent();

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
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
        public void ExitCurrentPage(object sender, RoutedEventArgs e)
        {
            Close();
            LoginPage.SignUpPageOpened = false;
        }
        public void MinimizeCurrentPage(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;
       
        /// <summary>
        /// The purpose below is to check that the user's inputs are valid.
        /// This is done by refering the 'CreateAccount' class to make sure
        /// that the values are valid using Regular Expressions.
        /// The information is then added to the Database if the card number 
        /// doesn't already exist.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignUpClick(object sender, RoutedEventArgs e)
        {
            Models.CreateAccount NewUser =  new (CreditCardNumber.Text, PinNumber.Password, EmailAddress.Text);

            List<string> Credentials = new List<string>()
            {
                NewUser.CheckCardNumber,
                NewUser.CheckPinNumber,
                NewUser.CheckEmail
            };

            CheckCredentials(NewUser);
            if (Credentials.All(x => x != "Invalid") && Terms.IsChecked == true)
            {
                TermsWarning.Content = string.Empty;
                AddAccountAttemptAsync(Credentials[0], Credentials[1], Credentials[2]);
            }
            if (Terms.IsChecked == false)
            {
                TermsWarning.Content = "Please check this box to proceed";
                TermsWarning.Foreground = Brushes.Orange;
            }
            else TermsWarning.Content = string.Empty;
        }

        /// <summary>
        /// This method queries the database and checks whether the card number or the email
        /// address inputted by the user already exists in the db. If it doesn't, an account
        /// is successfully created. If it does exist, a message is outputted to the user.
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <param name="PinNumber"></param>
        /// <param name="Email"></param>
        private async void AddAccountAttemptAsync(string CardNumber, string PinNumber, string Email)
        {
            try
            {
                await new Models.DatabaseQueries().AddNewAccountAsync(CardNumber, PinNumber, Email);
                await Task.Delay(200);

                if (Models.DatabaseQueries.SuccessfulSignUp == true)
                {
                    LoginPage.SignUpPageOpened = false;
                    Close();
                    new SuccessfullSignUp().Show();
                }
                else
                {
                    InvalidSignUp.Text = "Email or credit card number already exists!";
                    Models.DatabaseQueries.SuccessfulSignUp = true;
                }
            }
            catch (Exception UserOrServerError)
            {
                Close();
                ConnectionErrorPage ErrorPage = new();
                ErrorPage.Show();
                ErrorPage.CauseOfErrorMessage(UserOrServerError.Message);
            }
        }

        /// <summary>
        /// This method is used to verify that the user's credentials are
        /// valid. It does this by accessing the 'CreateAccount' class using
        /// getters and setters.
        /// </summary>
        /// <param name="CurrentUser">
        /// Is an instance of the 'CreateAccount' class. It defines a single user
        /// signing up.
        /// </param>
        private void CheckCredentials(Models.CreateAccount CurrentUser)
        {
            if (CurrentUser.CheckCardNumber == "Invalid")
            {
                CardWarning.Content = "Invalid Credit/Debit card";
                CardWarning.Foreground = Brushes.Orange;
            }
            else
            {
                CardWarning.Content = string.Empty;
            }
            if (CurrentUser.CheckEmail == "Invalid")
            {
                EmailWarning.Content = "Invalid Email Address";
                EmailWarning.Foreground = Brushes.Orange;
            }
            else
            {
                EmailWarning.Content = string.Empty;
            }
            if (CurrentUser.CheckPinNumber == "Invalid")
            {
                PinWarning.Content = "Invalid Pin Number";
                PinWarning.Foreground = Brushes.Orange;
            }
            else
            {
                PinWarning.Content = string.Empty;
            }
        }
    }
}
