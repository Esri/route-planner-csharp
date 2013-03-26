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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Import;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Pages.Wizards;
using ESRI.ArcLogistics.DomainObjects;

using AppPages = ESRI.ArcLogistics.App.Pages;

using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class contains helper method for application simplifying routine.
    /// </summary>
    internal class CommonHelpers
    {
        #region Public definitions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string XML_SETTINGS_INDENT_CHARS = "    ";

        #endregion // Public definitions

        #region Public helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method find in route.stops stop which associated object is equals to order and return it
        /// oteherwise return null
        /// </summary>
        /// <param name="route"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static Stop GetBoundStop(Route route, Order order)
        {
            Stop boundStop = null;

            foreach (Stop stop in route.Stops)
            {
                if (stop.AssociatedObject is Order && ((Order)stop.AssociatedObject).Equals(order))
                {
                    boundStop = stop;
                    break;
                }
            }

            return boundStop;
        }

        /// <summary>
        /// Get page help topic
        /// </summary>
        /// <param name="topicName">Topic name (use PagePaths const)</param>
        public static HelpTopic GetHelpTopic(string topicName)
        {
            HelpTopic topic = null;
            if (!string.IsNullOrEmpty(topicName))
            {
                HelpTopics topics = App.Current.HelpTopics;
                if (null != topics)
                    topic = topics.GetTopic(topicName);
            }

            return topic;
        }

        /// <summary>
        /// Seek in all application's directory with plug-ins and create list with file paths
        /// </summary>
        /// <remarks>Select all files with ".dll" and if it's a .Net assemblies</remarks>
        public static ICollection<string> GetAssembliesFiles()
        {
            // 1. Application's binary folder
            var appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var assemblies =
                new List<string>(_GetAssembliesFiles(appBaseDirectory));

            // 2. User’s folder
            var userPlugInsFolder = Settings.Default.UsersPlugInsFolder;
            if (!string.IsNullOrEmpty(userPlugInsFolder) &&
                // Compare directories with ignore case to load additional assemblies
                // from the different folder.
                (string.Compare(appBaseDirectory, userPlugInsFolder, true) != 0))
            {
                assemblies.AddRange(_GetAssembliesFiles(userPlugInsFolder));
            }

            return assemblies.AsReadOnly();
        }

        /// <summary>
        /// Add exception info to message window
        /// </summary>
        public static void AddServiceMessage(string service, Exception ex)
        {
            AuthenticationException exAuthentication = ex as AuthenticationException;
            if (ex is CommunicationException)
            {
                App.Current.Messenger.AddMessage(MessageType.Error,
                    FormatServiceCommunicationError(service, ex as CommunicationException));
            }
            else if (exAuthentication != null)
            {
                string format = (string)App.Current.FindResource("ServiceAuthError");
                string errorMessage = string.Format(format, service, exAuthentication.ServiceName);

                Link link = new Link((string)App.Current.FindResource("LicencePanelText"),
                                     Pages.PagePaths.LicensePagePath, LinkType.Page);

                App.Current.Messenger.AddMessage(MessageType.Error, errorMessage, link);
            }
        }

        /// <summary>
        /// Add routing service exception message info to message window
        /// </summary>
        public static void AddRoutingErrorMessage(Exception ex)
        {
            if (ex is LicenseException)
            {
                Link link = new Link((string)App.Current.FindResource("UpdateLicenseLinkText"),
                                     Licenser.Instance.UpgradeLicenseURL, LinkType.Url);
                App.Current.Messenger.AddMessage(MessageType.Error, ex.Message, link);
            }
            else if ((ex is AuthenticationException) || (ex is CommunicationException))
            {
                string service = (string)App.Current.FindResource("ServiceNameRouting");
                CommonHelpers.AddServiceMessage(service, ex);
            }
            else
                App.Current.Messenger.AddError(ex.Message);
        }

        /// <summary>
        /// Adds tracking service exception message info to the messenger.
        /// </summary>
        /// <param name="ex">Exception to be converted to the message.</param>
        public static void AddTrackingErrorMessage(Exception ex)
        {
            Debug.Assert(null != ex);

            if ((ex is AuthenticationException) || (ex is CommunicationException))
            {
                string service = Properties.Resources.ServiceNameTracking;
                AddServiceMessage(service, ex);
            }
            else
                App.Current.Messenger.AddError(ex.Message);
        }

        /// <summary>
        /// Add service exception message info to message window
        /// </summary>
        public static void AddServiceMessageWithDetail(string statusMessage, string service, Exception ex)
        {
            if (ex is AuthenticationException)
            {
                string detailString = string.Format((string)App.Current.FindResource("ServiceAuthError"),
                                                    service, (ex as AuthenticationException).ServiceName);

                Link link = new Link((string)App.Current.FindResource("LicencePanelText"),
                                     Pages.PagePaths.LicensePagePath, LinkType.Page);

                List<MessageDetail> details = new List<MessageDetail>();

                details.Add(new MessageDetail(MessageType.Error, detailString, link));
                App.Current.Messenger.AddError(statusMessage, details);
            }
            else if (ex is CommunicationException)
            {
                string detailString = FormatServiceCommunicationError(service,
                    ex as CommunicationException);

                List<MessageDetail> details = new List<MessageDetail>();
                details.Add(new MessageDetail(MessageType.Error, detailString));

                App.Current.Messenger.AddError(statusMessage, details);
            }
        }

        /// <summary>
        /// Workaround: for support true validation state in xceed grid - fill address of not geocoded object 
        /// with same values to help xceed grid react on property changing
        /// </summary>
        /// <param name="address">Address to refill with the same values</param>
        public static void FillAddressWithSameValues(Address address)
        {
            Address tempAddress = (Address)address.Clone();
            tempAddress.CopyTo(address);
        }

        /// <summary>
        /// Check is source http\https link
        /// </summary>
        /// <param name="source">Source link - file name, relative\absolute path, http\https link</param>
        public static bool IsSourceHttpLink(string source)
        {
            return (source.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                    source.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
        }

        public static string FormatServiceCommunicationError(string serviceName,
                                                             CommunicationException ex)
        {
            switch (ex.ErrorCode)
            {
                case CommunicationError.ServiceTemporaryUnavailable:
                    {
                        var format = (string)App.Current.FindResource("ServiceTemporaryUnavailable");
                        var message = string.Format(format, serviceName);
                        return message;
                    }

                case CommunicationError.ServiceResponseTimeout:
                    {
                        var format = App.Current.FindString("ServiceResponseTimeout");
                        var message = string.Format(format, serviceName);
                        return message;
                    }

                default:
                    {
                        var format = (string)App.Current.FindResource("ServiceConnectionError");
                        var message = string.Format(format, serviceName);

                        return FormatCommunicationError(message, ex);
                    }
            }
        }

        public static string FormatServerCommunicationError(string serverName,
                                                            CommunicationException ex)
        {
            string format = (string)App.Current.FindResource("ServerConnectionError");
            string msg = string.Format(format, serverName);

            return FormatCommunicationError(msg, ex);
        }

        public static string FormatCommunicationError(string message,
                                                      CommunicationException ex)
        {
            Debug.Assert(message != null);
            Debug.Assert(ex != null);

            if (ex.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
            {
                message += "\n";
                message += (string)App.Current.FindResource("ProxyAuthError");
            }

            return message;
        }

        /// <summary>
        /// Find route by mobile device, used by driver or vehicle.
        /// </summary>
        /// <param name="deviceToFound">Mobile device to found.</param>
        /// <param name="schedule">Schedule to search in.</param>
        /// <returns>Route if found, null otherwise.</returns>
        public static Route GetRouteByMobileDevice(MobileDevice deviceToFound, Schedule schedule)
        {
            if (schedule == null)
            {
                return null;
            }

            var routesWithDevice =
                from route in schedule.Routes
                let driver = route.Driver
                let vehicle = route.Vehicle
                where
                    driver != null && driver.MobileDevice == deviceToFound ||
                    vehicle != null && vehicle.MobileDevice == deviceToFound
                select route;

            return routesWithDevice.FirstOrDefault();
        }

        /// <summary>
        /// Get route stops sorted by sequence number.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>Route stops sorted by sequence number.</returns>
        public static List<Stop> GetSortedStops(Route route)
        {
            List<Stop> routeStops = new List<Stop>();
            routeStops.AddRange(route.Stops);

            // Do sort.
            routeStops.Sort(delegate(Stop s1, Stop s2)
            {
                return s1.SequenceNumber.CompareTo(s2.SequenceNumber);
            });

            return routeStops;
        }

        /// <summary>
        /// Check is all address fields is empty.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Is all fields empty.</returns>
        public static bool IsAllAddressFieldsEmpty(Address address)
        {
            bool isEmpty = string.IsNullOrEmpty(address.FullAddress) &&
                string.IsNullOrEmpty(address.Unit) &&
                string.IsNullOrEmpty(address.AddressLine) &&
                string.IsNullOrEmpty(address.Locality1) &&
                string.IsNullOrEmpty(address.Locality2) &&
                string.IsNullOrEmpty(address.Locality3) &&
                string.IsNullOrEmpty(address.CountyPrefecture) &&
                string.IsNullOrEmpty(address.PostalCode1) &&
                string.IsNullOrEmpty(address.PostalCode2) &&
                string.IsNullOrEmpty(address.StateProvince) &&
                string.IsNullOrEmpty(address.Country);

            return isEmpty;
        }

        /// <summary>
        /// Create name address record from order.
        /// </summary>
        /// <param name="order">Source order.</param>
        /// <param name="oldAddress">Address before geocoding.</param>
        /// <returns>Name address record.</returns>
        public static NameAddressRecord CreateNameAddressPair(Order order, Address oldAddress)
        {
            Debug.Assert(order != null);

            NameAddressRecord nameAddressRecord = new NameAddressRecord();
            nameAddressRecord.NameAddress = new NameAddress();

            NameAddress nameAddress = nameAddressRecord.NameAddress;
            nameAddress.Name = order.Name;

            Debug.Assert(order.GeoLocation.HasValue);

            nameAddressRecord.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(order.GeoLocation.Value.X, order.GeoLocation.Value.Y);

            Address matchedAddress;
            if (oldAddress == null)
            {
                nameAddress.Address = (Address)order.Address.Clone();

                // Make matched address empty.
                matchedAddress = new Address();
            }
            else
            {
                // Set old order address as source address.
                nameAddressRecord.NameAddress.Address = (Address)oldAddress.Clone();

                // Set current address as matched.
                matchedAddress = (Address)order.Address.Clone();
            }

            nameAddressRecord.MatchedAddress = matchedAddress;

            return nameAddressRecord;
        }

        /// <summary>
        /// Convert grid selection changed args to NotifyCollectionChangedEventArgs.
        /// </summary>
        /// <param name="selectionInfos">Grid selection infos.</param>
        /// <returns>Converted collection changed args.</returns>
        public static NotifyCollectionChangedEventArgs GetSelectionChangedArgsFromGrid(
            IList<SelectionInfo> selectionInfos)
        {
            Debug.Assert(selectionInfos != null);

            // Get collections of added and removed items.
            List<object> addedItems = new List<object>();
            List<object> removedItems = new List<object>();

            foreach (SelectionInfo selectionInfo in selectionInfos)
            {
                foreach (object obj in selectionInfo.AddedItems)
                {
                    addedItems.Add(obj);
                }

                foreach (object obj in selectionInfo.RemovedItems)
                {
                    removedItems.Add(obj);
                }
            }

            var args = GetSelectionChangedArgs(addedItems, removedItems);

            return args;
        }

        /// <summary>
        /// Gets NotifyCollectionChangedEventArgs instance for the specified
        /// collections of added and removed items.
        /// </summary>
        /// <param name="addedItems">Collection of added items.</param>
        /// <param name="removedItems">Collection of removed items.</param>
        /// <returns>A new instance of the collection changed args or null reference
        /// if both collections are empty.</returns>
        public static NotifyCollectionChangedEventArgs GetSelectionChangedArgs(
            IList addedItems,
            IList removedItems)
        {
            Debug.Assert(addedItems != null);
            Debug.Assert(removedItems != null);

            NotifyCollectionChangedEventArgs args = null;

            // Create collection event args.
            if (addedItems.Count > 0 && removedItems.Count == 0)
            {
                args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    addedItems,
                    0);
            }
            else if (addedItems.Count == 0 && removedItems.Count > 0)
            {
                args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removedItems,
                    0);
            }
            else if (addedItems.Count > 0 && removedItems.Count > 0)
            {
                args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    addedItems,
                    removedItems);
            }

            return args;
        }

        /// <summary>
        /// Get or create "one-time" import profile.
        /// </summary>
        /// <param name="type">Import object type.</param>
        /// <returns>Application "one-time" import profile.</returns>
        public static ImportProfile GetOneTimeProfile(ImportType type)
        {
            ImportProfile onFlyProfile = App.Current.ImportProfilesKeeper.GetOneTimeProfile(type);
            if (null == onFlyProfile)
            {   // create new one time profile
                onFlyProfile = new ImportProfile();
                onFlyProfile.Type = type;
                onFlyProfile.IsDefault = false;
                onFlyProfile.IsOnTime = true;
                string objectsName = GetImportObjectsName(onFlyProfile.Type);
                onFlyProfile.Name = _GetUniqueName(onFlyProfile.Type, objectsName);
            }

            return onFlyProfile;
        }

        /// <summary>
        /// Gets import objects name by import type.
        /// </summary>
        /// <param name="type">Import type.</param>
        /// <returns>Imported object name.</returns>
        public static string GetImportObjectsName(ImportType type)
        {
            string resourceObjsName = null;
            switch (type)
            {
                case ImportType.Orders:
                    resourceObjsName = "ImportOders";
                    break;

                case ImportType.Locations:
                    resourceObjsName = "ImportLocations";
                    break;

                case ImportType.Drivers:
                    resourceObjsName = "ImportDrivers";
                    break;

                case ImportType.Vehicles:
                    resourceObjsName = "ImportVehicles";
                    break;

                case ImportType.MobileDevices:
                    resourceObjsName = "ImportMobileDevices";
                    break;

                case ImportType.DefaultRoutes:
                    resourceObjsName = "ImportDefaultRoutes";
                    break;

                case ImportType.DriverSpecialties:
                    resourceObjsName = "ImportDriverSpecialties";
                    break;

                case ImportType.VehicleSpecialties:
                    resourceObjsName = "ImportVehicleSpecialties";
                    break;

                case ImportType.Barriers:
                    resourceObjsName = "ImportBarriers";
                    break;

                case ImportType.Zones:
                    resourceObjsName = "ImportZones";
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported type
                    break;
            }

            string result = App.Current.FindString(resourceObjsName);
            return result;
        }

        /// <summary>
        /// Starts fleet setup wizard.
        /// </summary>
        public static void StartFleetSetupWizard()
        {
            Project project = App.Current.Project;
            if (null != project)
            {
                if (Settings.Default.IsAutomaticallyShowFleetWizardEnabled &&
                    (0 == project.Locations.Count) && (0 == project.Vehicles.Count) &&
                    (0 == project.Drivers.Count) && (0 == project.DefaultRoutes.Count))
                {
                    var wizard = new FleetSetupWizard();
                    wizard.Start();
                }
            }
        }

        /// <summary>
        /// Get margin for button panel which needs to lie opposite to current item in datagridcontrol.
        /// </summary>
        /// <param name="currentItem">Current item.</param>
        /// <param name="dataGridControl">Data grid control.</param>
        /// <param name="defaultRowHeight">Default data grid control row heigth.</param>
        /// <returns>Margin for button panel to set it opposite to current item in datagridcontrol.</returns>
        public static Thickness GetItemContainerMargin(object currentItem, DataGridControlEx dataGridControl, double defaultRowHeight)
        {
            var row = dataGridControl.GetContainerFromItem(currentItem)
                as Xceed.Wpf.DataGrid.DataRow;

            var margin = new Thickness();
            if (null == row)
            {
                // Current item not yet set.
                margin.Top = dataGridControl.Margin.Top + defaultRowHeight + 1;
            }
            else
            {
                try
                {
                    // Get coords relatively to window.
                    var leftTopPoint = new Point(0, 0);
                    Point rowLeftTopPoint = row.PointFromScreen(leftTopPoint);

                    Point dataGridControlLeftTopPoint =
                        dataGridControl.PointFromScreen(leftTopPoint);

                    // Use diff of coords to set offset from top of DataGridControl and ButtonPanel parent grid.
                    margin.Top = dataGridControl.Margin.Top - rowLeftTopPoint.Y +
                        dataGridControlLeftTopPoint.Y;
                }
                catch
                {
                    // Workaround: Sometimes visual is not connected.
                    margin.Top = dataGridControl.Margin.Top + defaultRowHeight + 1;
                }
            }

            return margin;
        }

        /// <summary>
        /// Hide "PostalCode2" column.
        /// </summary>
        /// <param name="dataGridControl">Datagrid control to hide column.</param>
        public static void HidePostalCode2Column(DataGridControlEx dataGridControl)
        {
            string postalCode2ColumnName = AddressPart.PostalCode2.ToString();
            if (dataGridControl.Columns[postalCode2ColumnName] != null)
            {
                dataGridControl.Columns[postalCode2ColumnName].Visible = false;
            }
        }

        /// <summary>
        /// Creates read-only collection with one element.
        /// </summary>
        /// <param name="obj">Initialize element.</param>
        /// <returns>Created read-only collection with one element.</returns>
        public static ICollection<T> CreateCollectionWithOneObject<T>(T obj)
        {
            Debug.Assert(null != obj);

            var collection = (new List<T>()
                                {
                                    obj
                                }
                             ).AsReadOnly();
            return collection;
        }

        /// <summary>

        /// <summary>
        /// Creates barrier.
        /// Since barrier's start and end dates should be created in code same ways in code,
        /// create this helper to this.
        /// </summary>
        /// <param name="startDate">Start barrier date.</param>
        /// <returns>Created barrier with inited start and end dates.</returns>
        public static Barrier CreateBarrier(DateTime startDate)
        {
            Barrier barrier = new Barrier(startDate, startDate.AddDays(1));
            return barrier;
        }

        /// <summary>
        /// Checks ignore virtual locations.
        /// </summary>
        /// <param name="obj">Object to edited check (only Route supported).</param>
        /// <returns>TRUE if virtual locations ignored.</returns>
        public static bool IgnoreVirtualLocations(object obj)
        {
            bool needIgnore = true;

            var route = obj as Route;

            if (route != null && Properties.Settings.Default.WarnUserIfVirtualLocationDetected &&
                // If only one location is virtual.
                (((null == route.StartLocation) && (null != route.EndLocation)) ||
                  ((null == route.EndLocation) && (null != route.StartLocation))))
            {
                needIgnore = _ShowVirtualLocationDialog();
            }

            return needIgnore;
        }

        /// <summary>
        /// Load template from embedded resource by key.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <returns>Control template.</returns>
        public static ControlTemplate LoadTemplateFromResource(string key)
        {
            ControlTemplate controlTemplate = null;
            Stream stream = null;

            try
            {
                stream = Application.Current.GetType().Assembly.GetManifestResourceStream(key);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                controlTemplate = XamlReader.Load(xmlReader) as ControlTemplate;
            }
            catch (Exception ex)
            {
                Debug.Assert(false);
                Logger.Info(ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return controlTemplate;
        }

        /// <summary>
        /// Convert barrier type to string.
        /// </summary>
        /// <param name="barrier">Source barrier.</param>
        /// <returns>UI string.</returns>
        public static string ConvertBarrierEffect(Barrier barrier)
        {
            string result = string.Empty;

            if (barrier.BarrierEffect.BlockTravel)
            {
                // Is block travel.
                result = (string)App.Current.FindResource("BlockTravelString");
            }
            else
            {
                string barrierEffectStringFmt = (string)App.Current.FindResource("BarrierEffectStringFmt");
                if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)
                {
                    // Is delay point barrier.
                    string delayAtBarrierPointString = (string)App.Current.FindResource("DelayForString");
                    string minsString = (string)App.Current.FindResource("MinsString");

                    result = string.Format(barrierEffectStringFmt, delayAtBarrierPointString,
                        barrier.BarrierEffect.DelayTime.ToString(), minsString);
                }
                else if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Polyline ||
                    barrier.Geometry is ESRI.ArcLogistics.Geometry.Polygon)
                {
                    string timesString = (string)App.Current.FindResource("PercentString");
                    string alongIntersectedStreetString = string.Empty;

                    double speedFactorInPercent = barrier.BarrierEffect.SpeedFactorInPercent;
                    if (speedFactorInPercent >= 0)
                    {
                        // Is speed up polygon barrier.
                        alongIntersectedStreetString = (string)App.Current.FindResource("SpeedUpString");
                    }
                    else
                    {
                        // Is slowdown polygon barrier.
                        alongIntersectedStreetString = (string)App.Current.FindResource("SlowdownString");
                        speedFactorInPercent = -speedFactorInPercent;
                    }

                    result = string.Format(barrierEffectStringFmt, alongIntersectedStreetString,
                        speedFactorInPercent.ToString(), timesString);
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets current map layer.
        /// </summary>
        /// <returns>Current map layer.</returns>
        public static MapLayer GetCurrentLayer()
        {
            Debug.Assert(null != App.Current.Map);

            MapLayer currentMapLayer = null;
            foreach (MapLayer layer in App.Current.Map.Layers)
            {
                if (layer.IsVisible && layer.IsBaseMap)
                {
                    currentMapLayer = layer;
                    break; // result founded
                }
            }
            Debug.Assert(null != currentMapLayer);

            return currentMapLayer;
        }

        #endregion // Public helpers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static ICollection<string> _GetAssembliesFiles(string directoryPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(directoryPath));

            List<string> list = new List<string>();
            if (!Directory.Exists(directoryPath))
                return list;

            try
            {
                //do through all the files in the plugin directory
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    FileInfo file = new FileInfo(filePath);

                    if (!file.Extension.Equals(".dll"))
                        continue; // NOTE: preliminary check, must be ".dll"

                    try
                    {
                        Assembly pluginAssembly = Assembly.LoadFrom(filePath);
                        list.Add(filePath);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return list.AsReadOnly();
        }

        /// <summary>
        /// Generates unique name for import profile.
        /// </summary>
        /// <param name="type">Import type.</param>
        /// <param name="objectsName">Objects name title.</param>
        /// <returns>Unique import profile name.</returns>
        private static string _GetUniqueName(ImportType type, string objectsName)
        {
            string profileNameSimple = string.Format((string)App.Current.FindResource("ImportObjectsTitleFormat"), objectsName);
            string profileNameFull = string.Format((string)App.Current.FindResource("ProfilesEditPageOnTimeTitleFormat"), profileNameSimple);

            StringCollection profileNames = new StringCollection();
            ICollection<ImportProfile> profiles = App.Current.ImportProfilesKeeper.Profiles;
            foreach (ImportProfile currentProfile in profiles)
            {
                if (type == currentProfile.Type)
                    profileNames.Add(currentProfile.Name);
            }

            string profileName = profileNameFull;
            if (profileNames.Contains(profileName))
            {   // seek possible archive file name
                for (int count = 1; count < int.MaxValue; ++count)
                {
                    profileName = string.Format(NAME_FORMAT_DUBLICATE_FORMAT, profileNameFull, count);
                    if (!profileNames.Contains(profileName))
                        break; // NOTE: result founded. Exit.
                }

                if (profileNames.Contains(profileName))
                    throw new NotSupportedException(); // exception
            }

            return profileName;
        }

        /// <summary>
        /// Shows warning virtual location detected.
        /// </summary>
        /// <returns>TRUE if clicked 'OK'.</returns>
        private static bool _ShowVirtualLocationDialog()
        {
            string applicationName = App.Current.FindString("ApplicationTitle");
            string title = App.Current.GetString("VirtualLocationDialogTitleFmt", applicationName);

            bool dontAsk = false;
            MessageBoxExButtonType result =
                MessageBoxEx.Show(App.Current.MainWindow,
                                  App.Current.FindString("VirtualLocationDialogText"),
                                  title,
                                  System.Windows.Forms.MessageBoxButtons.OKCancel,
                                  MessageBoxImage.Warning,
                                  App.Current.FindString("VirtualLocationDialogCheckBoxText"),
                                  ref dontAsk);
            if (dontAsk)
            {   // update response
                Properties.Settings.Default.WarnUserIfVirtualLocationDetected = false;
                Properties.Settings.Default.Save();
            }

            return (MessageBoxExButtonType.Ok == result);
        }

        #endregion // Private helpers

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Profile name format for duplicate.
        /// </summary>
        private const string NAME_FORMAT_DUBLICATE_FORMAT = "{0} ({1})";

        #endregion // Private constants
    }
}
