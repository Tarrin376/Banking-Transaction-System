using System.Text.RegularExpressions;

namespace ATM_System.Models
{
    public class CreateAccount
    {
        /// <summary>
        /// Non-Static fields used to access the user's inputted
        /// information. This data is used to verify inputted 
        /// information into the program.
        /// </summary>
        private string CardNumber;
        private string PinNumber;
        private string Email;

        /// <summary>
        /// Constructor that is used to access the user's inputted information from the 
        /// User Interface.
        /// </summary>
        /// <param name="CardNumber">Is the credit or debit card number</param>
        /// <param name="PinNumber">Is the pin number from the user</param>
        /// <param name="Email">Is the email address from the user</param>
        internal CreateAccount(string CardNumber, string PinNumber="", string Email="")
        {
            CheckCardNumber = CardNumber;
            CheckPinNumber = PinNumber;
            CheckEmail = Email;
            
        }

        /// <summary>
        /// Credit/Debit card getter and setter.
        /// Verifies the validity of the user's input.
        /// </summary>
        public string CheckCardNumber
        {
            get => CardNumber;
            set
            {
                Regex CreditCardPattern = new(@"[0-9]{4}\s[0-9]{4}\s[0-9]{4}\s[0-9]{4}$");
                if (CreditCardPattern.IsMatch(value)) CardNumber = value;
                else CardNumber = "Invalid";
            }
        }

        /// <summary>
        /// Pin number getter and setter.
        /// Verifies the validity of the user's input.
        /// </summary>
        public string CheckPinNumber
        {
            get => PinNumber;
            set
            {
                Regex PinNumberPattern = new(@"[0-9]{6}$");
                if (PinNumberPattern.IsMatch(value)) PinNumber = value;
                else PinNumber = "Invalid";
            }
        }

        /// <summary>
        /// Email address getter and setter.
        /// Verifies the validity of the user's input.
        /// </summary>
        public string CheckEmail
        {
            get => Email;
            set
            {
                Regex EmailPattern = new(@"[a-zA-Z0-9.-]+@[a-zA-Z]+\.([a-z]+$|co.uk)");
                if (EmailPattern.IsMatch(value)) Email = value;
                else Email = "Invalid";
            }
        }
    }
}
