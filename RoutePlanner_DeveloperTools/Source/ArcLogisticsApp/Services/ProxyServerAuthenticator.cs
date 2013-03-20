using System;
using System.Net;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.App.Services
{
    /// <summary>
    /// Class authenticate default web proxy that is used for HTTP connections.
    /// NOTE: The class is not thread-safe. Use it only from the main UI thread.
    /// </summary>
    static class ProxyServerAuthenticator
    {
        #region public static methods
        /// <summary>
        /// Method shows user a dialog that asks proxy server credentials
        /// </summary>
        /// <param name="proxyConfigurationService">The reference to the proxy configuration
        /// service object to set credentials for.</param>
        /// <returns>Returns true in case user entered proxy server settings and pressed OK or false if he pressed Cancel.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="proxyConfigurationService"/>
        /// is a null reference.</exception>
        static public bool AskAndSetProxyCredentials(
            IProxyConfigurationService proxyConfigurationService)
        {
            if (proxyConfigurationService == null)
            {
                throw new ArgumentNullException("proxyConfigurationService");
            }

            // create dialog
            AuthenticationDlg dlg = new AuthenticationDlg();

            // set title and caption
            dlg.Title = (string)Application.Current.FindResource("ProxyAuthenticationRequiredTitle");
            dlg.Text = _FormatDialogText();
            
            // set presaved username
            var settings = proxyConfigurationService.Settings;
            if (settings.UseAuthentication)
            {
                dlg.Username = settings.Username;
                dlg.RememberMe = true;
            }

            // save override cursor and set it to null, so user won't see wait cursor during work with the dialog
            Cursor overrideCursor = null;
            if (Mouse.OverrideCursor != null)
            {
                overrideCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = null;
            }

            // show dialog
            Nullable<bool> result = dlg.ShowDialog();

            // restore override cursor if needed
            if (overrideCursor != null)
                Mouse.OverrideCursor = overrideCursor;

            // if user pressed OK
            if (result.HasValue && result.Value)
            {
                // save credentials to user settings storage
                var username = dlg.Username;
                var password = dlg.Password;
                if (!dlg.RememberMe)
                {
                    username = null;
                    password = null;
                }

                settings.UseAuthentication = dlg.RememberMe;
                settings.Username = username;
                settings.Password = password;

                proxyConfigurationService.Update();

                // save username and password in local cache to show them next time
                _credentials.UserName = dlg.Username;
                _credentials.Password = dlg.Password;

                // set credentials to default web proxy
                _SetDefaultProxyServerCredentials(_credentials.UserName, _credentials.Password);

                return true;
            }

            return false;
        }

        #endregion

        #region private static methods
        /// <summary>
        /// Returns prompt text to show in the dialog.
        /// </summary>
        static private string _FormatDialogText()
        {
            string text = null;

            // get default proxy
            IWebProxy defaultProxy = WebRequest.DefaultWebProxy;
            if (defaultProxy != null)
            {
                // get proxy address for some hard coded address: we support only a single proxy server.
                Uri proxyUri = defaultProxy.GetProxy(new Uri(ANY_ADDRESS));
                if (proxyUri != null)
                {
                    string proxyAddress = proxyUri.Host;
                    string proxyPort = proxyUri.Port.ToString();

                    // format text
                    string textFormat = (string)Application.Current.FindResource("ProxyAuthenticationDlgText");
                    text = string.Format(textFormat, proxyAddress, proxyPort);
                }
            }

            return text;
        }

        static private void _SetDefaultProxyServerCredentials(string username, string password)
        {
            WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(username, password);
        }

        #endregion

        #region private static fields

        private const string ANY_ADDRESS = "http://microsoft.com";

        // cached credentials
        static private NetworkCredential _credentials = new NetworkCredential();

        #endregion
    }
}
