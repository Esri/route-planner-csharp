using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Interaction logic for ProxyAutenticationDialog.xaml
    /// </summary>
    internal partial class AuthenticationDlg : Window
    {
        #region Constructors

        public AuthenticationDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Password text
        /// </summary>
        public string Password
        {
            get
            {
                return passwordBox.Password;
            }
            set
            {
                passwordBox.Password = value;
            }
        }

        /// <summary>
        /// Username text
        /// </summary>
        public string Username
        {
            get
            {
                return usernameTextBox.Text;
            }
            set
            {
                usernameTextBox.Text = value;
            }
        }

        /// <summary>
        /// Remember me flag value
        /// </summary>
        public bool RememberMe
        {
            get
            {
                return rememberMeCheckBox.IsChecked.HasValue && rememberMeCheckBox.IsChecked.Value;
            }
            set
            {
                rememberMeCheckBox.IsChecked = value;
            }
        }

        /// <summary>
        /// Text string content
        /// </summary>
        public string Text
        {
            get
            {
                return captionTextBlock.Text;
            }
            set
            {
                captionTextBlock.Text = value;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Closes dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // user canceled
            DialogResult = false; 
            Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // user is OK
            DialogResult = true;
            Close();
        }

        private void usernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            passwordBox.IsEnabled = (usernameTextBox.Text.Length > 0);
            okButton.IsEnabled = (passwordBox.Password.Length > 0 && passwordBox.IsEnabled);
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            okButton.IsEnabled = (passwordBox.Password.Length > 0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            usernameTextBox.Focus();
        }

        #endregion
    }
}
