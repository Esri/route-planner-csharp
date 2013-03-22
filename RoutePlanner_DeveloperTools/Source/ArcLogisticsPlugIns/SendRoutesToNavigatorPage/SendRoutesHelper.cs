/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Controls;

using Microsoft.Win32;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// Class that implements send routes functionality
    /// </summary>
    internal class SendRoutesHelper
    {
        #region Public methods

        /// <summary>
        /// Inits state
        /// </summary>
        /// <param name="app">Current application</param>
        /// <param name="sendRoutesPage">Page</param>
        /// <param name="resources">Resources</param>
        public void Initialize(App app, PageBase sendRoutesPage)
        {
            _app = app;
            _page = sendRoutesPage;

            _grfExporterConfig = GrfExporterSettingsConfig.Instance;
        }

        /// <summary>
        /// Execute send
        /// </summary>
        /// <param name="routesConfigs">Sent routes config collection</param>
        public void Execute(ICollection<SentRouteConfig> routesConfigs)
        {
            // Set page status to "Sending routes..."
            _app.MainWindow.StatusBar.SetStatus(_page, Properties.Resources.SendingRoutes);

            // Get properties to export
            IList<string> propertyNames = new List<string>(Order.GetPropertyNames(_app.Project.CapacitiesInfo,
                _app.Project.OrderCustomPropertiesInfo, _app.Geocoder.AddressFields)); ;

            int totalSended = 0;
            int totalSendedSuccessfully = 0;
            List<MessageDetail> details = new List<MessageDetail>();

            // List of all selected route's export warning messages.
            List<string> exportWarningMessages = new List<string>();

            foreach (SentRouteConfig sendedRouteConfig in routesConfigs)
            {
                // If route checked to be exported - export it
                if (sendedRouteConfig.IsChecked)
                {
                    GrfExportResult exportWarnings = new GrfExportResult();

                    bool result = _ProcessSendedRouteConfig(sendedRouteConfig.Route, details, 
                        propertyNames, ref exportWarnings);

                    // Increase total sent routes counrer
                    totalSended++;

                    // Increase successfully sent routes counter
                    if (result)
                        totalSendedSuccessfully++;

                    // If there was messages during export then add unique
                    // route's export result messages to list of export results warning messages.
                    foreach(string message in exportWarnings.Warnings)
                        if (!exportWarningMessages.Contains(message))
                            exportWarningMessages.Add(message);
                }
            }

            // Show result in message window.
            _ShowResults(totalSended, totalSendedSuccessfully, exportWarningMessages, details);

        }

        #endregion

        #region private methods

        /// <summary>
        /// Show result message in message window.
        /// </summary>
        /// <param name="totalSended">Number of routes that must be send.</param>
        /// <param name="totalSendedSuccessfully">Number of successfully sended routes.</param>
        /// <param name="exportWarningMessages">List of warning strings.</param>
        /// <param name="details">List of MessageDetails.</param>
        private void _ShowResults(int totalSended, int totalSendedSuccessfully, 
            List<string> exportWarningMessages, List<MessageDetail> details)
        {
            // Get message.
            string sendedMessage;
            if (totalSendedSuccessfully == 0)
                sendedMessage = Properties.Resources.FailedToSendRoutes;
            else if (totalSendedSuccessfully == totalSended)
                sendedMessage = Properties.Resources.AllRoutesSent;
            else
                sendedMessage = string.Format(Properties.Resources.RoutesSent,
                    totalSendedSuccessfully, totalSended);

            // Set page status.
            _app.MainWindow.StatusBar.SetStatus(_page, sendedMessage);

            // If we have some export results, add warning.
            if (exportWarningMessages.Count != 0)
            {
                // Put export results messages on the top of the details.
                foreach (var message in exportWarningMessages)
                    details.Insert(0, new MessageDetail(MessageType.Warning, message));

                // Add warning to message window.
                _app.Messenger.AddWarning(sendedMessage, details);
            }
            // If all routes sended successfully - add message.
            else if (totalSended == totalSendedSuccessfully)
                _app.Messenger.AddMessage(sendedMessage, details);
            // If not all routes were sended - add error.
            else
                _app.Messenger.AddError(sendedMessage, details);
        }

        /// <summary>
        /// Process sending route
        /// </summary>
        /// <param name="route">Sent route</param>
        /// <param name="details">Results message details</param>
        /// <param name="selectedPropertyNames">Names of selected properties</param>
        /// <returns>True if successfully sent</returns>
        private bool _ProcessSendedRouteConfig(Route route, IList<MessageDetail> details,
            IList<string> selectedPropertyNames, ref GrfExportResult exportResult)
        {
            bool result = false;
            Link link = null;
            string message = string.Empty;
            try
            {
                message = _DoSend(route, selectedPropertyNames, ref exportResult);
                result = true;
            }
            catch (SettingsException ex)
            {
                Logger.Error(ex);
                message = string.Format(Properties.Resources.SendingFailed,
                    route.Name, ex.Message);
            }
            catch (IOException ex)
            {
                Logger.Error(ex);
                message = string.Format(Properties.Resources.SendingFailed,
                    route.Name, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex);
                message = string.Format(Properties.Resources.SendingFailed,
                    route.Name, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(ex);
                message = string.Format(Properties.Resources.SendingFailed,
                    route.Name, ex.Message);
            }
            catch (SmtpException ex)
            {
                Logger.Error(ex);
                string innerMessage = Properties.Resources.MailConnectionError;
                message = string.Format(Properties.Resources.SendingFailed,
                    route.Name, innerMessage);
            }
            catch (MailerSettingsException ex)
            {
                Logger.Error(ex);
                message = string.Format(Properties.Resources.SendingFailed,
                    route.Name, ex.Message);

                link = new Link(Properties.Resources.ExportToNavigatorLink,
                                     ExportToNavigatorPreferencesPagePath, LinkType.Page);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                if (ex is AuthenticationException || ex is CommunicationException)
                {
                    string service = (string)_app.FindResource("ServiceNameRouting");
                    message = AddServiceMessageWithDetail(service, ex);

                    if (ex is AuthenticationException)
                    {
                        link = new Link((string)_app.FindResource("LicencePanelText"),
                           ESRI.ArcLogistics.App.Pages.PagePaths.LicensePagePath, LinkType.Page);
                    }
                }
                else
                {
                    string innerMessage = Properties.Resources.UnknownError;
                    message = string.Format(Properties.Resources.SendingFailed,
                        route.Name, innerMessage);
                }
            }

            MessageDetail messageDetail;

            // If route sent successfully - add information details
            // If route was not sent - add error details
            if (result)
            {
                messageDetail = new MessageDetail(MessageType.Information, message);
            }
            else
            {
                if (link == null)
                {
                    messageDetail = new MessageDetail(MessageType.Error, message);
                }
                else
                {
                    messageDetail = new MessageDetail(MessageType.Error, message, link);
                }
            }

            details.Add(messageDetail);

            return result;
        }

        /// <summary>
        /// Add service exception message info to message window
        /// </summary>
        internal static string AddServiceMessageWithDetail(string service, Exception ex)
        {
            string detailString = string.Empty;

            // NOTE tgoryagina: Fix CR147546.
            if (string.Equals(service, VRP_ROUTING_STRING))
                service = ROUTING_STRING;

            if (ex is AuthenticationException)
            {
                AuthenticationException exAuthentication = ex as AuthenticationException;
                string format = (string)App.Current.FindResource("ServiceAuthError");
                detailString = string.Format(format, service, exAuthentication.ServiceName);
            }
            else if (ex is CommunicationException)
            {
                string format = (string)App.Current.FindResource("ServiceConnectionError");
                string msg = string.Format(format, service);

                detailString = FormatCommunicationError(msg, ex as CommunicationException);
            }

            return detailString;
        }

        internal static string FormatCommunicationError(string message,
            CommunicationException ex)
        {
            Debug.Assert(message != null);
            Debug.Assert(ex != null);

            if (ex.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
            {
                message += Environment.NewLine;
                message += (string)App.Current.FindResource("ProxyAuthError");
            }

            return message;
        }

        /// <summary>
        /// Make send
        /// </summary>
        /// <param name="route">Route for sending</param>
        /// <param name="selectedPropertyNames">Names of selected properties</param>
        /// <returns>Error message</returns>
        private string _DoSend(Route route, IList<string> selectedPropertyNames, ref GrfExportResult exportResult)
        {
            string message = string.Empty;

            // Generate filename
            DateTime routeDate = route.StartTime.Value.Date;
            string filename = string.Format(FILENAME_FORMAT, routeDate.ToString("yyyyMMdd"),
                route.Name, DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            // Add extention to filename
            filename += (_grfExporterConfig.RouteGrfCompression) ? GZExt : GRFExt;

            // Get mobile device
            MobileDevice mobileDevice = route.Driver.MobileDevice;
            if (mobileDevice == null)
                mobileDevice = route.Vehicle.MobileDevice;

            // React on empty mobile device
            if (mobileDevice == null)
            {
                string errorMessage = Properties.Resources.MobileDeviceEmpty;
                throw new SettingsException(errorMessage);
            }

            // Do send depends on sync type
            switch (mobileDevice.SyncType)
            {
                case SyncType.ActiveSync:
                    exportResult = _SendToActiveSync(route, filename, selectedPropertyNames, mobileDevice);
                    message = string.Format(Properties.Resources.SuccessfullySentToDevice,
                                route.Name, mobileDevice.Name);
                    break;
                case SyncType.EMail:
                    exportResult = _SendToEMail(route, filename, selectedPropertyNames, mobileDevice);
                    message = string.Format(Properties.Resources.SuccessfullyMailed,
                        route.Name, mobileDevice.EmailAddress);
                    break;
                case SyncType.Folder:
                    exportResult =_SendToFolder(route, filename, selectedPropertyNames, mobileDevice);
                    message = string.Format(Properties.Resources.SuccessfullySentToFolder,
                        route.Name, mobileDevice.SyncFolder);
                    break;
                case SyncType.None:
                    {
                        // React on empty sync type
                        string errorMessage = (mobileDevice == route.Driver.MobileDevice) ? 
                            Properties.Resources.SyncTypeNotSelectedForDriverDevice : 
                            Properties.Resources.SyncTypeNotSelectedForVehicleDevice;
                        throw new SettingsException(errorMessage);
                    }
                default:
                    {
                        throw new SettingsException( Properties.Resources.SyncTypeIsNotSupported);
                    }
            }

            return message;
        }

        /// <summary>
        /// Save GRF file to folder
        /// </summary>
        /// <param name="route">Route to export</param>
        /// <param name="filename">GRF Filename</param>
        /// <param name="selectedPropertyNames">Names of selected properties</param>
        /// <param name="mobileDevice">Mobile device</param>
        private GrfExportResult _SendToFolder(Route route, string filename, IList<string> selectedPropertyNames, MobileDevice mobileDevice)
        {
            if (string.IsNullOrEmpty(mobileDevice.SyncFolder))
                throw new SettingsException( Properties.Resources.FolderPathEmpty);

            string filePath = Path.Combine(mobileDevice.SyncFolder, filename);

            return GrfExporter.ExportToGRF(filePath, route, _app.Project, _app.Geocoder, _app.Solver,
                selectedPropertyNames, _grfExporterConfig.RouteGrfCompression);

        }

        /// <summary>
        /// Send GRF by E-Mail
        /// </summary>
        /// <param name="route">Route to export</param>
        /// <param name="filename">GRF Filename</param>
        /// <param name="selectedPropertyNames">Names of selected properties</param>
        /// <param name="mobileDevice">Mobile device</param>
        private GrfExportResult _SendToEMail(Route route, string filename, IList<string> selectedPropertyNames, MobileDevice mobileDevice)
        {
            string filePath = "";
            try
            {
                if (string.IsNullOrEmpty(mobileDevice.EmailAddress))
                    throw new SettingsException(Properties.Resources.MailAddressEmpty);

                string tempFolderPath = Environment.GetEnvironmentVariable("TEMP");
                filePath = Path.Combine(tempFolderPath, filename);

                DateTime routeDate = route.StartTime.Value.Date;

                var exportResults = GrfExporter.ExportToGRF(filePath, route, _app.Project, _app.Geocoder,
                    _app.Solver, selectedPropertyNames, _grfExporterConfig.RouteGrfCompression);

                // Fill E-Mail message
                string subject = string.Format(Properties.Resources.GRFLetterSubjectFormat,
                    route.Vehicle.Name, routeDate.ToString("yyyyMMdd"));
                string[] attachments = new string[1];
                attachments[0] = filePath;

                // Try to send E-Mail message
                if (_mailer == null)
                {
                    if (_mailerException != null)
                        throw _mailerException;
                    else
                    {
                        try
                        {
                            _mailer = new Mailer(_grfExporterConfig);
                        }
                        catch (Exception ex)
                        {
                            _mailerException = ex;
                            throw;
                        }
                    }
                }

                _mailer.Send(mobileDevice.EmailAddress, mobileDevice.EmailAddress, subject, "", attachments, filePath);

                return exportResults;
            }
            finally
            {
                _DeleteFile(filePath);
            }
        }

        /// <summary>
        /// Send GRF to device via active sync
        /// </summary>
        /// <param name="route">Route to export</param>
        /// <param name="filename">GRF filename</param>
        /// <param name="selectedPropertyNames">Names of selected properties</param>
        /// <param name="mobileDevice">Mobile device</param>
        private GrfExportResult _SendToActiveSync(Route route, string filename, IList<string> selectedPropertyNames, MobileDevice mobileDevice)
        {
            if (string.IsNullOrEmpty(mobileDevice.ActiveSyncProfileName))
                throw new SettingsException(Properties.Resources.ActiveSyncProfileEmpty);

            // Try to find device name in registry
            string deviceKeyPath = _FindDeviceKeyPath(mobileDevice);
            bool deviceFound = deviceKeyPath.Length > 0;

            string syncPath = string.Empty;
            if (deviceFound)
            {
                string syncDeviceKeyPath = Path.Combine(deviceKeyPath, MobileDevice.PARTNER_SYNC_PATH);

                RegistryKey partnerRegKey = Registry.CurrentUser.OpenSubKey(syncDeviceKeyPath);

                // Get synchronization path for device
                syncPath = (string)partnerRegKey.GetValue(MobileDevice.BRIEFCASEPATH);
                if (string.IsNullOrEmpty(syncPath))
                {
                    string message = string.Format(Properties.Resources.SyncPathAbsent,
                        mobileDevice.ActiveSyncProfileName);
                    throw new SettingsException(message);
                }

                if (syncPath[0] == '\\')
                    syncPath = syncPath.Remove(0, 1);
            }
            else
            {
                string message = string.Format(Properties.Resources.DeviceNotFound,
                    mobileDevice.ActiveSyncProfileName);
                throw new SettingsException(message);
            }

            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string deviceSyncFullPath = Path.Combine(documentsFolder, syncPath);
            string filePath = Path.Combine(deviceSyncFullPath, filename);

            return GrfExporter.ExportToGRF(filePath, route, _app.Project, _app.Geocoder,
                    _app.Solver, selectedPropertyNames, false);
        }

        /// <summary>
        /// Find key path in registry for mobile device
        /// </summary>
        /// <param name="mobileDevice">Mobile device</param>
        /// <returns>Registry key</returns>
        private string _FindDeviceKeyPath(MobileDevice mobileDevice)
        {
            string deviceKeyPath = string.Empty;

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(MobileDevice.PARTNERS_KEY_PATH);
            if (regKey != null)
            {
                string[] partnerNames = regKey.GetSubKeyNames();
                foreach (string partnerName in partnerNames)
                {
                    string partnerKey = Path.Combine(MobileDevice.PARTNERS_KEY_PATH, partnerName);
                    RegistryKey partnerRegKey = Registry.CurrentUser.OpenSubKey(partnerKey);
                    if (partnerRegKey != null)
                    {
                        string partnerDisplayName = (string)partnerRegKey.GetValue(MobileDevice.PARTNERS_DISPLAYNAME);
                        if (partnerDisplayName.Equals(mobileDevice.ActiveSyncProfileName, StringComparison.OrdinalIgnoreCase))
                        {
                            deviceKeyPath = partnerKey;
                            partnerRegKey.Close();
                            break;
                        }
                    }

                    partnerRegKey.Close();
                }

                regKey.Close();
            }

            return deviceKeyPath;
        }

        private void _DeleteFile(string filePath)
        {
            try
            {
                // delete file in case it was not sended
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Private constants

        private const string FILENAME_FORMAT = "{0}_{1}_{2}";

        private const string GRFExt = ".grf";
        private const string GZExt = ".grf.gz";

        /// <summary>
        /// Preferences for plugin page name
        /// </summary>
        private const string ExportToNavigatorPreferencesPagePath = @"Preferences\ExportToNavigatorSettings";

        /// <summary>
        /// "VRP/Routing" string
        /// </summary>
        private const string VRP_ROUTING_STRING = "VRP/Routing";

        /// <summary>
        /// "Routing" string
        /// </summary>
        private const string ROUTING_STRING = "Routing";

        #endregion

        #region Private members

        private App _app;
        private PageBase _page;
        private Mailer _mailer;
        private Exception _mailerException;
        private GrfExporterSettingsConfig _grfExporterConfig;

        #endregion
    }
}
