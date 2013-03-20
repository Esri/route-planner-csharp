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
using System.Net;
using System.Net.Mail;
using System.Xaml;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.DomainObjects;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// Interaction logic for MailAuthorisationDlg.xaml
    /// </summary>
    internal partial class MailAuthorisationDlg : Window
    {
        #region constructors

        private MailAuthorisationDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Ask password from user
        /// </summary>
        /// <param name="client">SMTP client</param>
        /// <param name="mailerSettingsConfig">Mailer settings</param>
        /// <returns>True if password was set</returns>
        public static bool Execute(SmtpClient client, GrfExporterSettingsConfig grfExporterSettingsConfig)
        {
            MailAuthorisationDlg dlg = new MailAuthorisationDlg();

            dlg.Owner = App.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dlg.Server.Text = grfExporterSettingsConfig.ServerAddress;
            dlg.UserName.Text = grfExporterSettingsConfig.UserName;
            dlg.Password.Focus();

            bool? res = dlg.ShowDialog();
            if (res.Value == true)
            {
                client.Credentials = new NetworkCredential(dlg.UserName.Text, dlg.Password.Password);
                if (dlg.RememberPassword.IsChecked.Value)
                {
                    grfExporterSettingsConfig.RememberPassword = true;
                    grfExporterSettingsConfig.Password = dlg.Password.Password;
                }
            }

            return res.Value;
        }

        #endregion

        #region private methods

        /// <summary>
        /// React on OK button was clicked
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// React on Close button was clicked
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
 