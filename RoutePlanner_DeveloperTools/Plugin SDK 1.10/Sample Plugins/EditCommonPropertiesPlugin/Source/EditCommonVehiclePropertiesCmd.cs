using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace EditCommonPropertiesPlugin
{
    // Add Task to Setup tab's Vehicles Panel's task widget.
    [CommandPlugIn(new string[1] { "VehiclesCommands" })]
    public class EditCommonVehiclePropertiesCmd : AppCommands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
            if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Vehicle" == selector.SelectedItems[0].GetType().ToString())
            {
                EditCommonPropertiesPopup popup = new EditCommonPropertiesPopup(itemList, this);
                popup.ShowDialog();
            }
            else
                m_application.Messenger.AddError("No vehicles selected.");


        }

        public void Initialize(App app)
        {
            m_application = app;
            CapacitiesInfo capInfo = App.Current.Project.CapacitiesInfo;
            
            foreach (CapacityInfo c in capInfo)
                itemList.Add(c.Name);

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
            get { return "EditCommonPropertiesPlugin.EditCommonVehiclePropertiescmd"; }
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
            
            string message = "Changing " + field + " to " + value + " for all seleced vehicles.";
            App.Current.Messenger.AddInfo(message);

            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;

            for (int i = 0; i < selector.SelectedItems.Count; i++)
            {

                Vehicle vehicleRef = (Vehicle)selector.SelectedItems[i];
                
                CapacitiesInfo orderCapacitiesInfo = vehicleRef.CapacitiesInfo;
                Capacities orderCapacities = vehicleRef.Capacities;

                double tempD;
               

                try
                {
                    switch (field)
                    {
                        #region Case Statements
                        
                        case "VehicleName": vehicleRef.Name = value; break;
                        case "FixedCost":
                            if (Double.TryParse(value.ToString(), out tempD))
                                vehicleRef.FixedCost = tempD;
                            break;

                        case "FuelEconomy":
                            if (Double.TryParse(value.ToString(), out tempD))
                                vehicleRef.FuelEconomy = tempD;
                            break;

                        case "FuelType":
                            foreach( FuelType f in App.Current.Project.FuelTypes)
                            {
                                if (f.Name == value)
                                    vehicleRef.FuelType = f;
                            }
                            break;

                        case "Specialties":
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

                                    foreach (VehicleSpecialty V in vehicleRef.Specialties)
                                    {
                                        if (String.Compare(V.Name, vs.Name, true) == 0)
                                        {
                                            V.CopyTo(vs);
                                            vehicleRef.Specialties.Remove(V);
                                            break;
                                        }
                                    }

                                    App.Current.Project.VehicleSpecialties.Add(vs);
                                    vehicleRef.Specialties.Add(vs);
                                }
                            }
                            break;

                        case "MobileDevice":
                            foreach( MobileDevice m in App.Current.Project.MobileDevices)
                            {
                                if (m.Name == value)
                                    vehicleRef.MobileDevice = m;
                            }
                            break;

                        case "Comment": vehicleRef.Comment = value; break;
                           
                        #endregion

                    }// End Switch

                    #region Custom order capacities

                    
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
                                    vehicleRef.Capacities[k] = tempD;

                                break;
                            }
                        }

                    }

                    #endregion custom order capacities

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
        List<string> itemList = new List<string> { "Vehicle Name", "Fixed Cost", "Fuel Economy", "Fuel Type", "Specialties", "Mobile Device", "Comment" };

    }
}
