using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace ATM_System.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Models.IPageFunctions
    {
        internal static bool SignUpPageOpened = false;

        public LoginPage() => InitializeComponent();

        /// <summary>
        /// IMPLEMENTED INTERFACES from the 'Models.IPageFunction' interface.
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
        /// This method opens the 'SignUpPage.xaml' file so the user can create an account.
        /// You can find the file in Pages -> SignUpPage.xaml and SignUpPage.cs (code behind).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignUpClick(object sender, RoutedEventArgs e)
        {
            if (SignUpPageOpened == false)
            {
                new SignUpPage().Show();
                SignUpPageOpened = true;
            }
        }

        /// <summary>
        /// This method is used to allow the user to contact support if they forgot their password.
        /// The user can do this by clicking the hyperlink called 'Forgot Password' on the
        /// 'LoginPage.xaml' file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ForgotPasswordClick(object sender, RequestNavigateEventArgs e)
        {
            Func<RequestNavigateEventArgs, TextBlock, Task> GetURLResult = 
                (RequestNavigateEventArgs e, TextBlock FailedURL) => Task.Run(() => RequestURL(e, FailedURL));
           
            await GetURLResult(e, FailedURLRequest);
        }

        /// <summary>
        /// This code is used by the 'ForgotPasswordClick' event handler function to make
        /// sure that the user is connected to the internet before requesting the page.
        /// This is done by using the 'System.Net.NetworkInformation' namespace and pinging
        /// Google.com
        /// </summary>
        readonly Action<RequestNavigateEventArgs, TextBlock> RequestURL = 
            (RequestNavigateEventArgs e, TextBlock FailedURL) =>
        {
            try
            {
                new Ping().Send("www.google.com");
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
                e.Handled = true;

                Application.Current.Dispatcher.Invoke(() => FailedURL.Text = "");
            }
            catch (PingException)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FailedURL.Text = "Page failed to load." +
                    " Make sure that you are connected to the internet";
                });
            }
        };

        /// <summary>
        /// The purpose of this method is to check whether the login boxes have been filled
        /// in correctly. It then calls the 'CheckLoginInformationAsync' method to query
        /// the database and check if the user's inputs are in the MySQL db. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            if (CardNumberBox.Text == string.Empty) BlankCardNumber.Content = "Please fill in this box.";
            else BlankCardNumber.Content = string.Empty;

            if (PINNumberBox.Password == string.Empty) BlankPinNumber.Content = "Please fill in this box";
            else BlankPinNumber.Content = string.Empty;

            if (CardNumberBox.Text != string.Empty && PINNumberBox.Password != string.Empty)
            {
                Models.CreateAccount CheckInputs = new(CardNumberBox.Text, PINNumberBox.Password);
                if (CheckInputs.CheckCardNumber != "Invalid" && CheckInputs.CheckPinNumber != "Invalid") CheckLoginInformationAsync();
                else InvalidLogin.Text = "Card or pin number is not in a valid bank format!";
            }
        }

        /// <summary>
        /// This method is used to query the MySQL database and check whether the user's inputs
        /// exist in the database. It asynchronously queries the database and shows the main page
        /// if the login credentials are found. If they are not, a message is displayed to the
        /// user.
        /// </summary>
        private async void CheckLoginInformationAsync()
        {
            try
            {
                await new Models.DatabaseQueries().LoginRequestAsync(CardNumberBox.Text, PINNumberBox.Password);
                await Task.Delay(200);

                if (Models.DatabaseQueries.SuccessfulLogin == true)
                {
                    new MainPage().Show();
                    InvalidLogin.Text = string.Empty;
                    CardNumberBox.Text = PINNumberBox.Password = null;
                    Models.DatabaseQueries.SuccessfulLogin = false;
                }
                else
                {
                    InvalidLogin.Text = "Card combination not found!";
                }
            }
            catch (Exception UserOrServerError)
            {
                ConnectionErrorPage ErrorPage = new();
                ErrorPage.Show();
                ErrorPage.CauseOfErrorMessage(UserOrServerError.Message);
            }
        }

        /// <summary>
        /// These methods are used to display contact information when the icons such as the
        /// Gmail, GitHub and Twitter icons are hovered over. When they are not hovered over,
        /// the contact information will disappear.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitterHover(object sender, MouseEventArgs e) => TwitterAccount.Content = "@Tarrin37925904";
        private void TwitterLeave(object sender, MouseEventArgs e) => TwitterAccount.Content = string.Empty;
        private void GitHubHover(object sender, MouseEventArgs e) => GitHubAccount.Content = "Tarrin376";
        private void GitHubLeave(object sender, MouseEventArgs e) => GitHubAccount.Content = string.Empty;
        private void GmailHover(object sender, MouseEventArgs e) => Gmail.Content = "tarrinc3@gmail.com";
        private void GmailLeave(object sender, MouseEventArgs e) => Gmail.Content = string.Empty;
    }
}
