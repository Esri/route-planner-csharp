using System;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace EditCommonPropertiesPlugin
{
    // Add Task to Orders View
    [CommandPlugIn(new string[1] { "UnassignedOrdersRoutingCommands" })]
    public class EditCommonOrderPropertiesCmd : AppCommands.ICommand, INotifyPropertyChanged
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
            if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Order" == selector.SelectedItems[0].GetType().ToString())
            {
                EditCommonPropertiesPopup popup = new EditCommonPropertiesPopup(itemList, this);
                popup.ShowDialog();
            }
            else
                m_application.Messenger.AddError("No orders selected.");
            
        }

        public void Initialize(App app)
        {
            m_application = app;
            CapacitiesInfo capInfo = App.Current.Project.CapacitiesInfo;
            OrderCustomPropertiesInfo propInfo = App.Current.Project.OrderCustomPropertiesInfo;

            foreach (CapacityInfo c in capInfo)
                itemList.Add(c.Name);

            foreach (OrderCustomProperty c in propInfo)
                itemList.Add(c.Name);

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
            get { return "EditCommonPropertiesPlugin.EditCommonOrderPropertiescmd"; }
        }

        public string Title
        {
            get { return "Edit Selected"; }
        }

        public string TooltipText
        {
            get { return "Click to edit all selected rows together."; }
        }

        #endregion

        public static void makeEdit(string field, string value)
        {
            field = field.Replace(" ", "");
            
            string message = "Changing " + field + " to " + value + " for all seleced orders.";
            App.Current.Messenger.AddInfo(message);

            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
            
            for(int i= 0;i<selector.SelectedItems.Count;i++)
            {
                // orderRef is a reference to the selected order [i]
                Order orderRef = (ESRI.ArcLogistics.DomainObjects.Order)selector.SelectedItems[i];
                
                OrderCustomPropertiesInfo orderPropertiesInfo = orderRef.CustomPropertiesInfo;
                OrderCustomProperties orderProperties = orderRef.CustomProperties;
                CapacitiesInfo orderCapacitiesInfo = orderRef.CapacitiesInfo;
                Capacities orderCapacities = orderRef.Capacities;

                double tempD;
                DateTime TWdateTime;
                
                try
                {
                    switch (field)
                    {
                        #region Case Statements
                        case "Name": orderRef.Name = value; break;
                        case "Address": orderRef.Address.AddressLine = value; break;
                        case "City": orderRef.Address.Locality3 = value; break;
                        case "State": orderRef.Address.StateProvince = value; break;
                        case "Zip": orderRef.Address.PostalCode1 = value; break;
                        case "Zip4": orderRef.Address.PostalCode2 = value; break;
                        case "Country": orderRef.Address.Country = value; break;

                        case "PlannedDate":
                            DateTime tempDT = new DateTime();
                            if (System.DateTime.TryParse(value, out tempDT))
                                orderRef.PlannedDate = tempDT;
                            break;

                        case "Priority":
                            if (value == "High") orderRef.Priority = OrderPriority.High;
                            else if (value == "Normal") orderRef.Priority = OrderPriority.Normal;
                            break;

                        case "OrderType":
                            if (value == "Pickup") 
                                orderRef.Type = OrderType.Pickup;
                            else if (value == "Delivery") 
                                orderRef.Type = OrderType.Delivery;
                            break;

                        case "ServiceTime":
                            if (Double.TryParse(value.ToString(), out tempD))
                                orderRef.ServiceTime = tempD;
                            break;

                       case "TimeWindowStart":
                            string tempS = value;
                            if (DateTime.TryParse(tempS, out TWdateTime))
                            {
                                if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                {
                                    orderRef.TimeWindow.From = TWdateTime.TimeOfDay;
                                    orderRef.TimeWindow.IsWideOpen = false;
                                }
                            }
                            break;

                        case "TimeWindowFinish":
                            if (DateTime.TryParse(value, out TWdateTime))
                            {
                                if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                {
                                    orderRef.TimeWindow.To = TWdateTime.TimeOfDay;
                                    orderRef.TimeWindow.IsWideOpen = false;
                                }
                            }
                            break;

                        case "TimeWindow2Start":

                            if (DateTime.TryParse(value, out TWdateTime))
                            {
                                if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                {
                                    orderRef.TimeWindow2.From = TWdateTime.TimeOfDay;
                                    orderRef.TimeWindow2.IsWideOpen = false;
                                }
                            }
                            break;

                        case "TimeWindow2Finish":

                            if (DateTime.TryParse(value, out TWdateTime))
                            {
                                if (TWdateTime.TimeOfDay != TimeSpan.Zero)
                                {
                                    orderRef.TimeWindow2.To = TWdateTime.TimeOfDay;
                                    orderRef.TimeWindow2.IsWideOpen = false;
                                }
                            }
                            break;

                        case "MaxViolationTime":
                            if (Double.TryParse(value, out tempD))
                                orderRef.MaxViolationTime = tempD;
                            break;

                        case "VehicleSpecialties":
                            if (value != "")
                            {
                                string[] stringSeparators = new string[] { ";", "," };
                                string[] specialties = value.Split(stringSeparators, StringSplitOptions.None);
                                foreach (string s in specialties)
                                {
                                    VehicleSpecialty vs = new VehicleSpecialty();
                                    vs.Name = s;
                                    foreach (VehicleSpecialty V in App.Current.Project.VehicleSpecialties)
                                    {
                                        if (String.Compare(V.Name, vs.Name, true) == 0)
                                        {
                                            V.CopyTo(vs);
                                            App.Current.Project.VehicleSpecialties.Remove(V);
                                            break;
                                        }
                                    }
                                    
                                    foreach (VehicleSpecialty V in orderRef.VehicleSpecialties)
                                    {
                                        if (String.Compare(V.Name, vs.Name, true) == 0)
                                        {
                                            V.CopyTo(vs);
                                            orderRef.VehicleSpecialties.Remove(V);
                                            break;
                                        }
                                    }

                                    App.Current.Project.VehicleSpecialties.Add(vs);
                                    orderRef.VehicleSpecialties.Add(vs);
                                }
                            }
                            break;

                        case "DriverSpecialties":
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
                                    foreach (DriverSpecialty D in orderRef.DriverSpecialties)
                                    {
                                        if (String.Compare(D.Name, ds.Name, true) == 0)
                                        {
                                            D.CopyTo(ds);
                                            orderRef.DriverSpecialties.Remove(D);
                                            break;
                                        }
                                    }

                                    App.Current.Project.DriverSpecialties.Add(ds);
                                    orderRef.DriverSpecialties.Add(ds);
                                }
                            }
                            break;
                        //end of case statements
                        #endregion 
                    }

                    #region Custom order properties and capacities

                    if (orderProperties.Count > 0)
                    {
                        OrderCustomProperty orderPropertyInfoItem = null;
                        for (int j = 0; j < orderPropertiesInfo.Count; j++)
                        {
                            orderPropertyInfoItem = orderPropertiesInfo.ElementAt(j) as OrderCustomProperty;
                            string tempName = orderPropertyInfoItem.Name.Replace(" ", "");
                            if (tempName == field)
                            {
                                orderRef.CustomProperties[j] = value;
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
                            if (tempName == field)
                            {
                                if (Double.TryParse(value, out tempD))
                                    orderRef.Capacities[k] = tempD;
                                    
                                break;
                            }
                        }

                    }
                    // End custom order properties and capacities
                    #endregion 

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
        
        List<string> itemList = new List<string> { "Name", "Planned Date", "Order Type", "Priority", "Service Time", "Time Window Start", "Time Window Finish", "Time Window2 Start", "Time Window2 Finish", "Vehicle Specialties", "Driver Specialties", "Max Violation Time", "Address", "City", "State", "Zip", "Zip4", "Country", "X", "Y" };

        private void selected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (App.Current.MainWindow.CurrentPage.ToString() == "ESRI.ArcLogistics.App.Pages.OptimizeAndEditPage")
            {
                ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;


                if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Order" == selector.SelectedItems[0].GetType().ToString())
                {
                    IsEnabled = true;
                }
                else
                    IsEnabled = false;
            }
            else
                IsEnabled = false;
        }

        Boolean _isEnabled = false;
        Boolean initialized = false;
        public event PropertyChangedEventHandler PropertyChanged;


        
    }
}
