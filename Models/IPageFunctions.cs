using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ATM_System.Models
{
    /// <summary>
    /// This interface have been implemented on all of the classes in the 
    /// 'Pages' folder. It allows each page to have basic custom functions
    /// such as closing and minimizing the window.
    /// </summary>
    internal interface IPageFunctions
    {
        /// <summary>
        /// This method is implemented to allow the user to click anywhere on the page
        /// with the left click mouse button which will drag the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MouseLeftClick(object sender, MouseButtonEventArgs e);
        void ExitCurrentPage(object sender, RoutedEventArgs e);
        void MinimizeCurrentPage(object sender, RoutedEventArgs e);
    }
}
