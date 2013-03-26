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
using System.Collections.Generic;

using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace EditCommonPropertiesPlugin
{
    // Add Task to Setup tab's Driver Panel's task widget.
    [CommandPlugIn(new string[1] { "DriversCommands" })]
    public class EditCommonDriverPropertiesCmd : AppCommands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
            if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Driver" == selector.SelectedItems[0].GetType().ToString())
            {
                EditCommonPropertiesPopup popup = new EditCommonPropertiesPopup(itemList, this);
                popup.ShowDialog();
            }
            else
                m_application.Messenger.AddError("No drivers selected.");

        }

        public void Initialize(App app)
        {
            m_application = app;
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "EditCommonPropertiesPlugin.EditCommonDriverPropertiescmd"; }
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

            string message = "Changing " + field + " to " + value + " for all seleced drivers.";
            App.Current.Messenger.AddInfo(message);

            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;

            for (int i = 0; i < selector.SelectedItems.Count; i++)
            {

                Driver driverRef = (Driver)selector.SelectedItems[i];
                double tempD;

                try
                {
                    switch (field)
                    {
                        #region Case Statements

                        case "DriverName": driverRef.Name = value; break;
                        case "FixedCost":
                            if (Double.TryParse(value.ToString(), out tempD))
                                driverRef.FixedCost = tempD;
                            break;

                        case "PerHourSalary":
                            if (Double.TryParse(value.ToString(), out tempD))
                                driverRef.PerHourSalary = tempD;
                            break;

                        case "PerHourOTSalary":
                            if (Double.TryParse(value.ToString(), out tempD))
                                driverRef.PerHourOTSalary = tempD;
                            break;

                        case "Specialties":
                            if (value != "")
                            {
                                string[] stringSeparators2 = new string[] { ";", "," };
                                string[] specialties2 = value.Split(stringSeparators2, StringSplitOptions.None);
                                foreach (string s in specialties2)
                                {
                                    DriverSpecialty ds = new DriverSpecialty();
                                    ds.Name = s;

                                    foreach (DriverSpecialty D in App.Current.Project.DriverSpecialties)
                                    {
                                        if (String.Compare(D.Name, ds.Name, true) == 0)
                                        {
                                            D.CopyTo(ds);
                                            App.Current.Project.DriverSpecialties.Remove(D);
                                            break;
                                        }
                                    }
                                    foreach (DriverSpecialty D in driverRef.Specialties)
                                    {
                                        if (String.Compare(D.Name, ds.Name, true) == 0)
                                        {
                                            D.CopyTo(ds);
                                            driverRef.Specialties.Remove(D);
                                            break;
                                        }
                                    }

                                    App.Current.Project.DriverSpecialties.Add(ds);
                                    driverRef.Specialties.Add(ds);
                                }
                            }
                            break;

                        case "MobileDevice":
                            foreach (MobileDevice m in App.Current.Project.MobileDevices)
                            {
                                if (m.Name == value)
                                    driverRef.MobileDevice = m;
                            }
                            break;

                        case "TimeBeforeOT":
                            if (Double.TryParse(value.ToString(), out tempD))
                                driverRef.TimeBeforeOT = tempD;
                            break;

                        case "Comment": driverRef.Comment = value; break;

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

        App m_application = null;
        List<string> itemList = new List<string> { "Driver Name", "Fixed Cost", "Per Hour Salary", "Per Hour OT Salary", "Specialties", "Mobile Device","Time Before OT", "Comment" };

    }
}
