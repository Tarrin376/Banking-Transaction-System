using System.Windows;

namespace ATM_System.Pages
{
    /// <summary>
    /// Interaction logic for ConnectionErrorPage.xaml
    /// </summary>
    public partial class ConnectionErrorPage
    {
        public ConnectionErrorPage() => InitializeComponent();

        public void ExitCurrentPage(object sender, RoutedEventArgs e) =>
            Close();

        public void CauseOfErrorMessage(string ErrorMessage) =>
            CauseOfIssue.Text = ErrorMessage;
    }
}
