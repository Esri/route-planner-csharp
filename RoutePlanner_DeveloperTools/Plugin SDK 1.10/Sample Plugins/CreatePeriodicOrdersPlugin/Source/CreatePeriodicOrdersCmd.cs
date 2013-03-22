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
using System.Data;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;


using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace CreatePeriodicOrdersPlugin
{
    [CommandPlugIn(new string[1] { "PeriodicOrdersTaskWidgetCommands" })]
    public class CreatePeriodicOrdersCmd : AppCommands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            string statusMessage = "Adding periodic orders between " + m_application.CurrentDate.ToShortDateString() + " and " + m_application.CurrentDate.AddDays(PeriodicOrdersPage.numDays -1).ToShortDateString();
            m_application.Messenger.AddInfo(statusMessage);
            m_application.MainWindow.StatusBar.WorkingStatus = statusMessage;
            Mouse.OverrideCursor = Cursors.Wait;
            
            
            List<ESRI.ArcLogistics.DomainObjects.Order> oList = processOrders2(PeriodicOrdersPage.dt);

            int checkBoxCount = 0;
            int comboBoxCount = 0;
            int periodicity = 0;

            foreach (Order o in oList)
            {
                periodicity = PeriodicOrdersPage.comboBoxList[comboBoxCount].SelectedIndex +1;

                for (int i = 0; i < PeriodicOrdersPage.numDays; i++)
                {
                    if ((i / 7) % (periodicity) == 0)
                    {
                        DayOfWeek d = m_application.CurrentDate.AddDays(i).DayOfWeek;
                        if (PeriodicOrdersPage.checkBoxList[checkBoxCount + (int)d].IsChecked == true)
                        {
                            Order tempO = new Order(o.CapacitiesInfo, o.CustomPropertiesInfo);
                            o.CopyTo(tempO);
                            tempO.PlannedDate = m_application.CurrentDate.AddDays(i);
                            m_application.Project.Orders.Add(tempO);
                        }
                    }
                }
                checkBoxCount = checkBoxCount + 7;
                comboBoxCount++;
            }

            m_application.Project.Save();
            m_application.MainWindow.StatusBar.WorkingStatus = null;
            Mouse.OverrideCursor = null;
            statusMessage = "Completed adding Periodic Orders.";
            m_application.Messenger.AddInfo(statusMessage);
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
            get { return "CreatePeriodicOrdersPlugin.CreatePeriodicOrdersCmd"; }
        }

        public string Title
        {
            get { return "Add Periodic Orders"; }
        }

        public string TooltipText
        {
            get { return "Add imported orders periodically for selected days, starting with the selected date."; }
        }

        public App m_application = null;

        #endregion

        private List<ESRI.ArcLogistics.DomainObjects.Order> processOrders2(DataTable table)
        {
            List<ESRI.ArcLogistics.DomainObjects.Order> OrderList = new List<Order>();

            foreach (DataRow row in table.Rows)
            {

                // Create New empty Order
                CapacitiesInfo capInfo = m_application.Project.CapacitiesInfo;
                OrderCustomPropertiesInfo propInfo = m_application.Project.OrderCustomPropertiesInfo;
                ESRI.ArcLogistics.DomainObjects.Order resultOrder = new ESRI.ArcLogistics.DomainObjects.Order(capInfo, propInfo);

                OrderCustomPropertiesInfo orderPropertiesInfo = resultOrder.CustomPropertiesInfo;
                OrderCustomProperties orderProperties = resultOrder.CustomProperties;
                CapacitiesInfo orderCapacitiesInfo = resultOrder.CapacitiesInfo;
                Capacities orderCapacities = resultOrder.Capacities;
                bool geocodeProvided = false;
                bool geocodeCorrect = false;
                bool geocodedCorrectly = false;
                double tempD;
                DateTime TWdateTime;
                Double tempX = 0.0;
                Double tempY = 0.0;

                // Insert Order Information
                resultOrder.PlannedDate = m_application.CurrentDate;

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    try
                    {
                        switch (table.Columns[i].ColumnName)
                        {
                            #region Case Statements
                            case "Name": resultOrder.Name = row["Name"].ToString(); break;
                            case "Address": resultOrder.Address.AddressLine = row["Address"].ToString(); break;
                            case "City": resultOrder.Address.Locality3 = row["City"].ToString(); break;
                            case "State": resultOrder.Address.StateProvince = row["State"].ToString(); break;
                            case "Zip": resultOrder.Address.PostalCode1 = row["Zip"].ToString(); break;
                            case "Zip4": resultOrder.Address.PostalCode2 = row["Zip4"].ToString(); break;
                            case "Country": resultOrder.Address.Country = row["Country"].ToString(); break;

                            case "PlannedDate":
                                DateTime tempDT = new DateTime();
                                if (System.DateTime.TryParse(row["PlannedDate"].ToString(), out tempDT))
                                    resultOrder.PlannedDate = tempDT;
                                break;

                            case "Priority":
                                if (row["Priority"].ToString() == "High") resultOrder.Priority = OrderPriority.High;
                                else if (row["Priority"].ToString() == "Normal") resultOrder.Priority = OrderPriority.Normal;
                                break;

                            case "OrderType":
                                if (row["OrderType"].ToString() == "Pickup") resultOrder.Type = OrderType.Pickup;
                                else if (row["OrderType"].ToString() == "Delivery") resultOrder.Type = OrderType.Delivery;
                                break;

                            case "ServiceTime":
                                if (Double.TryParse(row["ServiceTime"].ToString(), out tempD))
                                    resultOrder.ServiceTime = tempD;
                                break;

                            case "TimeWindowStart":
                                string tempS = row["TimeWindowStart"].ToString();
                                if (DateTime.TryParse(tempS, out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow.From = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "TimeWindowFinish":
                                if (DateTime.TryParse(row["TimeWindowFinish"].ToString(), out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow.To = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "TimeWindow2Start":

                                if (DateTime.TryParse(row["TimeWindow2Start"].ToString(), out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow2.From = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow2.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "TimeWindow2Finish":

                                if (DateTime.TryParse(row["TimeWindow2Finish"].ToString(), out TWdateTime))
                                {
                                    if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                    {
                                        resultOrder.TimeWindow2.To = TWdateTime.TimeOfDay;
                                        resultOrder.TimeWindow2.IsWideOpen = false;
                                    }
                                }
                                break;

                            case "MaxViolationTime":
                                if (Double.TryParse(row["MaxViolationTime"].ToString(), out tempD))
                                    resultOrder.MaxViolationTime = tempD;
                                break;

                            case "VehicleSpecialties":
                                if (row["VehicleSpecialties"].ToString() != "")
                                {
                                    string[] stringSeparators = new string[] { ";", "," };
                                    string[] specialties = row["VehicleSpecialties"].ToString().Split(stringSeparators, StringSplitOptions.None);
                                    foreach (string s in specialties)
                                    {
                                        VehicleSpecialty vs = new VehicleSpecialty();
                                        vs.Name = s;
                                        foreach (VehicleSpecialty V in m_application.Project.VehicleSpecialties)
                                        {
                                            if (String.Compare(V.Name, vs.Name, true) == 0)
                                            {
                                                V.CopyTo(vs);
                                                m_application.Project.VehicleSpecialties.Remove(V);
                                                break;
                                            }
                                        }
                                        m_application.Project.VehicleSpecialties.Add(vs);
                                        resultOrder.VehicleSpecialties.Add(vs);
                                    }
                                }
                                break;

                            case "DriverSpecialties":
                                if (row["DriverSpecialties"].ToString() != "")
                                {
                                    string[] stringSeparators2 = new string[] { ";", "," };
                                    string[] specialties2 = row["DriverSpecialties"].ToString().Split(stringSeparators2, StringSplitOptions.None);
                                    foreach (string s in specialties2)
                                    {
                                        DriverSpecialty ds = new DriverSpecialty();
                                        ds.Name = s;

                                        foreach (DriverSpecialty D in m_application.Project.DriverSpecialties)
                                        {
                                            if (String.Compare(D.Name, ds.Name, true) == 0)
                                            {
                                                D.CopyTo(ds);
                                                m_application.Project.DriverSpecialties.Remove(D);
                                                break;
                                            }
                                        }
                                        m_application.Project.DriverSpecialties.Add(ds);
                                        resultOrder.DriverSpecialties.Add(ds);
                                    }
                                }
                                break;

                            case "X":
                                string x = row["X"].ToString();
                                if (x != "" && x != null)
                                    if (Double.TryParse(row["X"].ToString(), out tempX))
                                    {
                                        if (tempX >= -180.0 && tempX <= 180.0 && tempX != 0.0)
                                        {
                                            geocodeProvided = true;
                                            geocodeCorrect = true;
                                        }
                                        else if (tempX == 0.0)
                                            geocodeCorrect = true;
                                    }

                                break;

                            case "Y":
                                string y = row["Y"].ToString();
                                if (y != "" && y != null)
                                    if (Double.TryParse(row["Y"].ToString(), out tempY))
                                    {
                                        if (tempY >= -90.0 && tempY <= 90.0 && tempY != 0)
                                        {
                                            geocodeProvided = true;
                                            geocodeCorrect = true;
                                        }
                                        else if (tempY == 0.0)
                                            geocodeCorrect = true;
                                    }

                                break;
                            #endregion
                        }

                        #region Custom Order Properties and Capacities
                        if (orderProperties.Count > 0)
                        {
                            OrderCustomProperty orderPropertyInfoItem = null;
                            for (int j = 0; j < orderPropertiesInfo.Count; j++)
                            {
                                orderPropertyInfoItem = orderPropertiesInfo.ElementAt(j) as OrderCustomProperty;
                                string tempName = orderPropertyInfoItem.Name.Replace(" ", "");
                                if (tempName == table.Columns[i].ColumnName)
                                {
                                    orderProperties[j] = (row[table.Columns[i].ToString()].ToString());
                                    break;
                                }
                            }
                        }

                        if (orderCapacities.Count > 0)
                        {
                            CapacityInfo orderCapacityInfoItem = null;
                            for (int k = 0; k < orderCapacitiesInfo.Count; k++)
                            {
                                orderCapacityInfoItem = orderCapacitiesInfo.ElementAt(k);
                                string tempName = orderCapacityInfoItem.Name.Replace(" ", "");
                                if (tempName == table.Columns[i].ColumnName)
                                {
                                    if (Double.TryParse(row[table.Columns[i].ToString()].ToString(), out tempD))
                                        orderCapacities[k] = tempD;

                                    break;
                                }
                            }

                        }

                        #endregion Custom Order Properties and Capacities
                    }
                    catch (Exception e)
                    {
                        string statusMessage = " Distribute Orders encountered a problem: " + e.Message;
                        m_application.Messenger.AddError(statusMessage);
                    }
                }

                resultOrder.CustomProperties = orderProperties;
                resultOrder.Capacities = orderCapacities;

                if (geocodeProvided && geocodeCorrect)
                {
                    AddressCandidate candidate1 = new AddressCandidate();
                    ESRI.ArcLogistics.Geometry.Point p = new ESRI.ArcLogistics.Geometry.Point(tempX, tempY);
                    candidate1.GeoLocation = p;
                    candidate1.Score = 100;
                    candidate1.Address = resultOrder.Address;

                    resultOrder.GeoLocation = candidate1.GeoLocation;
                    geocodedCorrectly = true;

                }
                else
                {
                    try
                    {
                        AddressCandidate candidate = new AddressCandidate();
                        candidate = m_application.Geocoder.Geocode(resultOrder.Address);
                        if (candidate != null)
                        {
                            resultOrder.GeoLocation = candidate.GeoLocation;
                            geocodedCorrectly = true;
                        }
                        else
                        {
                            //TODO: Handle orders which were not geocoded!! 
                        }
                    }
                    catch (Exception )
                    {
                        //string statusMessage = "Distribute Orders encountered a problem while geocoding addresses: " + e.Message;
                        //m_application.Messenger.AddError(statusMessage);
                    }
                }

                // Add Order
                if (geocodedCorrectly)
                    OrderList.Add(resultOrder);
                else
                {
                    string statusMessage = "Problem while importing order: " + resultOrder.Name;
                    m_application.Messenger.AddError(statusMessage);
                    OrderList.Add(resultOrder);
                }

            }

            return (OrderList);

        }

       

    }
}
