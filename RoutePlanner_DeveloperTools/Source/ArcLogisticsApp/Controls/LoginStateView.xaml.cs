using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for LoginStateView.xaml
    /// </summary>
    internal partial class LoginStateView : UserControl
    {
        public LoginStateView()
        {
            InitializeComponent();
        }

        #region protected methods
        /// <summary>
        /// Performs UI specific view initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var vm = this.DataContext as LoginStateViewModel;
            if (vm == null)
            {
                return;
            }

            var host = vm.LoginViewHost;
            if (host != null && !host.IsUsernameControlFocused)
            {
                this.Dispatcher.BeginInvoke(
                    (Action)_FocusOnUsernameControl,
                    DispatcherPriority.Normal);
                host.IsUsernameControlFocused = true;
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Handles password changes.
        /// </summary>
        /// <param name="sender">The event sender (i.e. PasswordBox instance).</param>
        /// <param name="e">The event arguments.</param>
        private void _LicensePasswordPasswordChanged(object sender, RoutedEventArgs e)
        {
            var element = sender as PasswordBox;
            if (element == null)
            {
                return;
            }

            var vm = element.DataContext as LoginStateViewModel;
            if (vm == null)
            {
                return;
            }

            vm.Password = element.Password;
        }

        /// <summary>
        /// Sets focus on the username control.
        /// </summary>
        private void _FocusOnUsernameControl()
        {
            this.UsernameControl.Focus();
            Keyboard.Focus(this.UsernameControl);
        }
        #endregion
    }
}
