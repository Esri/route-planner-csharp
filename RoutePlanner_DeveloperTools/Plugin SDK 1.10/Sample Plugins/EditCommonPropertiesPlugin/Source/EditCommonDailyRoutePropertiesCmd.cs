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
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace EditCommonPropertiesPlugin
{
    // Add Task to Routes View.
    [CommandPlugIn(new string[1] { "RoutesRoutingCommands" })]
    public class EditCommonDailyRoutePropertiesCmd : AppCommands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
            if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Route" == selector.SelectedItems[0].GetType().ToString())
            {
                EditCommonPropertiesPopup popup = new EditCommonPropertiesPopup(itemList, this);
                popup.ShowDialog();
            }
            else
                m_application.Messenger.AddError("No routes selected.");

        }

        public void Initialize(App app)
        {
            m_application = app;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);
        }

        void Current_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content.ToString() == "ESRI.ArcLogistics.App.Pages.OptimizeAndEditPage" && !initialized)
            {
                // Subscribe to the page's selection changed event
                ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
                ((INotifyCollectionChanged)selector.SelectedItems).CollectionChanged += new NotifyCollectionChangedEventHandler(selected_CollectionChanged);
                initialized = true;
            }
        }

        private void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.Navigated += new System.Windows.Navigation.NavigatedEventHandler(Current_Navigated);
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;

                // Notify about property change.
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
            }
        }

        public KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "EditCommonPropertiesPlugin.EditCommonDailyRoutePropertiescmd"; }
        }

        public string Title
        {
            get { return "Edit Selected"; }
        }

        public string TooltipText
        {
            get { return "Click to edit all selected rows together"; }
        }

        #endregion

        public static void makeEdit(string field, string value)
        {
            field = field.Replace(" ", "");

            string message = "Changing " + field + " to " + value + " for all seleced routes.";
            App.Current.Messenger.AddInfo(message);

            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;

            for (int i = 0; i < selector.SelectedItems.Count; i++)
            {

                Route routeRef = (Route)selector.SelectedItems[i];
                double tempD;
                DateTime TWdateTime;

                try
                {
                    switch (field)
                    {
                        #region Case Statements

                        case "RoutesName": routeRef.Name = value; break;

                        case "Visible":
                            if ((String.Compare(value, "Yes", true) == 0) || (String.Compare(value, "Y", true) == 0) || (String.Compare(value, "True", true) == 0))
                                routeRef.IsVisible = true;
                            else if ((String.Compare(value, "NO", true) == 0) || (String.Compare(value, "N", true) == 0) || (String.Compare(value, "False", true) == 0))
                                routeRef.IsVisible = false;
                            break;

                        case "Locked":
                            if ((String.Compare(value, "Yes", true) == 0) || (String.Compare(value, "Y", true) == 0) || (String.Compare(value, "True", true) == 0))
                                routeRef.IsLocked = true;
                            else if ((String.Compare(value, "NO", true) == 0) || (String.Compare(value, "N", true) == 0) || (String.Compare(value, "False", true) == 0))
                                routeRef.IsLocked = false;
                            break;

                        case "Vehicle":
                            foreach (Vehicle v in App.Current.Project.Vehicles)
                            {
                                if (v.Name == value)
                                    routeRef.Vehicle = v;
                            }
                            break;

                        case "Driver":
                            foreach (Driver d in App.Current.Project.Drivers)
                            {
                                if (d.Name == value)
                                    routeRef.Driver = d;
                            }
                            break;


                        case "StartTimeWindowStart":
                            if (DateTime.TryParse(value, out TWdateTime))
                            {
                                if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                {
                                    routeRef.StartTimeWindow.From = TWdateTime.TimeOfDay;
                                    routeRef.StartTimeWindow.IsWideOpen = false;
                                }
                            }
                            break;

                        case "StartTimeWindowFinish":
                            if (DateTime.TryParse(value, out TWdateTime))
                            {
                                if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                {
                                    routeRef.StartTimeWindow.To = TWdateTime.TimeOfDay;
                                    routeRef.StartTimeWindow.IsWideOpen = false;
                                }
                            }
                            break;

                        case "StartLocation":
                            foreach (Location l in App.Current.Project.Locations)
                            {
                                if (l.Name == value)
                                    routeRef.StartLocation = l;
                            }
                            break;

                        case "TimeAtStart":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.TimeAtStart = tempD;
                            break;

                        case "EndLocation":
                            foreach (Location l in App.Current.Project.Locations)
                            {
                                if (l.Name == value)
                                    routeRef.EndLocation = l;
                            }
                            break;

                        case "TimeAtEnd":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.TimeAtEnd = tempD;
                            break;

                        case "RenewalLocation":
                            if (value != "")
                            {
                                string[] stringSeparators2 = new string[] { ";", "," };
                                string[] locations2 = value.Split(stringSeparators2, StringSplitOptions.None);
                                foreach (string s in locations2)
                                {
                                    bool locationFound = false;
                                    bool addToRoute = true;
                                    Location l = new Location();
                                    l.Name = s;

                                    foreach (Location L in App.Current.Project.Locations)
                                    {
                                        if (String.Compare(L.Name, l.Name, true) == 0)
                                        {
                                            L.CopyTo(l);
                                            locationFound = true;
                                            break;
                                        }
                                    }
                                    foreach (Location L in routeRef.RenewalLocations)
                                    {
                                        if (String.Compare(L.Name, l.Name, true) == 0)
                                        {
                                            addToRoute = false;
                                            break;
                                        }
                                    }

                                    if (locationFound && addToRoute)
                                        routeRef.RenewalLocations.Add(l);
                                }
                            }
                            break;

                        case "TimeAtRenewal":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.TimeAtRenewal = tempD;
                            break;

                        case "MaxOrders":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.MaxOrders = (int)tempD;
                            break;

                        case "MaxTravelDistance":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.MaxTravelDistance = tempD;
                            break;

                        case "MaxTravelDuration":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.MaxTravelDuration = tempD;
                            break;

                        case "MaxTotalDuration":
                            if (Double.TryParse(value.ToString(), out tempD))
                                routeRef.MaxTotalDuration = tempD;
                            break;

                        case "Zones":
                            if (value != "")
                            {
                                string[] stringSeparators = new string[] { ";", "," };
                                string[] zones = value.Split(stringSeparators, StringSplitOptions.None);
                                foreach (string s in zones)
                                {
                                    bool zoneFound = false;
                                    bool addToRoute = true;
                                    Zone z = new Zone();
                                    z.Name = s;

                                    foreach (Zone Z in App.Current.Project.Zones)
                                    {
                                        if (String.Compare(Z.Name, z.Name, true) == 0)
                                        {
                                            Z.CopyTo(z);
                                            zoneFound = true;
                                            break;
                                        }
                                    }
                                    foreach (Zone Z in routeRef.Zones)
                                    {
                                        if (String.Compare(Z.Name, z.Name, true) == 0)
                                        {
                                            addToRoute = false;
                                            break;
                                        }
                                    }

                                    if (zoneFound && addToRoute)
                                        routeRef.Zones.Add(z);
                                }
                            }
                            break;

                        case "HardZones":
                            if ((String.Compare(value, "Yes", true) == 0) || (String.Compare(value, "Y", true) == 0) || (String.Compare(value, "True", true) == 0))
                                routeRef.HardZones = true;
                            else if ((String.Compare(value, "NO", true) == 0) || (String.Compare(value, "N", true) == 0) || (String.Compare(value, "False", true) == 0))
                                routeRef.HardZones = false;
                            break;

                        case "Comment": routeRef.Comment = value; break;

                        #endregion

                    }// End Switch
                }
                catch (Exception e)
                {
                    message = "Error: " + e.Message;
                    App.Current.Messenger.AddError(message);
                }
            }
            App.Current.Project.Save();

        }

        private void selected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (App.Current.MainWindow.CurrentPage.ToString() == "ESRI.ArcLogistics.App.Pages.OptimizeAndEditPage")
            {
                ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
                if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Route" == selector.SelectedItems[0].GetType().ToString())
                {
                    IsEnabled = true;
                }
                else
                    IsEnabled = false;
            }
            else
                IsEnabled = false;
        }

        App m_application = null;
        List<string> itemList = new List<string> { "Locked", "Visible", "Routes Name", "Vehicle", "Driver", "Max Orders", "Start Time Window Start", "Start Time Window Finish", "Max Travel Distance", "Max Travel Duration", "Max Total Duration", "Start Location", "Time At Start", "End Location", "Time At End", "Renewal Locations", "Time At Renewal", "Zones", "Hard Zones", "Comment" };

        Boolean _isEnabled = false;
        Boolean initialized = false;
        public event PropertyChangedEventHandler PropertyChanged;

        
    }
}
