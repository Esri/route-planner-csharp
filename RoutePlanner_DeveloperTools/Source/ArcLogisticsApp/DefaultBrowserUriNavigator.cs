using System;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.App.IUriNavigator"/> interface
    /// by opening it with a default browser.
    /// </summary>
    internal sealed class DefaultBrowserUriNavigator : IUriNavigator
    {
        #region IUriNavigator members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Navigates to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to navigate to.</param>
        public void NavigateToUri(string uri)
        {
            Debug.Assert(!string.IsNullOrEmpty(uri));

            try
            {
                Mouse.OverrideCursor = Cursors.AppStarting;
                Process.Start(new ProcessStartInfo(uri));
            }
            catch (Win32Exception ex)
            {
                // NOTE:
                // System.ComponentModel.Win32Exception is a known exception that occurs when Firefox is default browser.
                // It actually opens the browser but STILL throws this exception so we can just ignore it. If not this exception,
                // then attempt to open the URL in IE instead.
                if (ex.ErrorCode != -2147467259) // in system not found internet browser
                {
                    // sometimes throws exception so we have to just ignore
                    // this is a common .NET bug that no one online really has a great reason for so now we just need to try to open
                    // the URL using IE if we can.
                    try
                    {
                        Process.Start(new ProcessStartInfo(INTERNET_EXPLORER_NAME, uri));
                    }
                    catch (Exception ex2)
                    {
                        // still nothing we can do so just show the error to the user here.
                        Logger.Error(ex2);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion // IUriNavigator members

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string INTERNET_EXPLORER_NAME = "IExplore.exe";

        #endregion // Private consts
    }
}
