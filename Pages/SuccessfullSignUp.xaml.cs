using System.Windows;
using System.Windows.Input;

namespace ATM_System.Pages
{
    /// <summary>
    /// Interaction logic for SuccessfullSignUp.xaml
    /// </summary>
    public partial class SuccessfullSignUp
    {
        public SuccessfullSignUp() => InitializeComponent();

        /// <summary>
        /// Both implemented interfaces from the 'Models.IPageFunction' interface
        /// MouseLeftClick allows the user to drag the page around.
        /// ExitCurrentPage allows the user to close the current page.
        /// </summary>
        /// <param name="sender">Is the mouse object being passed into the method</param>
        /// <param name="e">Is the certain event that was raised</param>
        public void MouseLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        public void ExitCurrentPage(object sender, RoutedEventArgs e) => Close();
    }
}
