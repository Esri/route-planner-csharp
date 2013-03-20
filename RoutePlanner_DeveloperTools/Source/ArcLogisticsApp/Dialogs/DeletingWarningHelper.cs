using System;
using System.Windows;
using System.Diagnostics;
using System.Collections;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Deleting warning show dialog helper.
    /// </summary>
    internal class DeletingWarningHelper
    {
        #region Static public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show question dialog.
        /// </summary>
        /// <param name="objects">List of objects to deleting.</param>
        /// <param name="typeNameObjectResource">Type name object in string resources.</param>
        /// <param name="typeNameObjectsResource">Type name objects in string resources.</param>
        /// <remarks>Init label from predifined resource names.</remarks>
        public static bool Execute(IList objects, string typeNameObjectResource, string typeNameObjectsResource)
        {
            // init properties
            string nameObjectResource = null;
            string nameObjectsResource = null;
            if (string.IsNullOrEmpty(typeNameObjectResource) || string.IsNullOrEmpty(typeNameObjectsResource))
                _GetTextResourcesNameByType(objects[0].GetType(), ref nameObjectResource, ref nameObjectsResource);
            else
            {
                nameObjectResource = typeNameObjectResource;
                nameObjectsResource = typeNameObjectsResource;
            }

            string name = (1 == objects.Count) ? objects[0].ToString() : null;

            return _ShowDialog(name, nameObjectResource, nameObjectsResource);
        }

        /// <summary>
        /// Show question dialog.
        /// </summary>
        /// <param name="objects">List of objects to deleting.</param>
        /// <remarks>Init label from object type.</remarks>
        public static bool Execute(IList objects)
        {
            return Execute(objects, null, null);
        }

        /// <summary>
        /// Show question dialog.
        /// </summary>
        /// <param name="name">Name of object to deleting.</param>
        /// <param name="typeNameObjectResource">Type name object in string resources.</param>
        /// <param name="typeNameObjectsResource">Type name objects in string resources.</param>
        /// <remarks>Init label from predifined users.</remarks>
        public static bool Execute(string name, string typeNameObjectResource, string typeNameObjectsResource)
        {
            Debug.Assert(!string.IsNullOrEmpty(typeNameObjectResource));
            Debug.Assert(!string.IsNullOrEmpty(typeNameObjectsResource));

            return _ShowDialog(name, typeNameObjectResource, typeNameObjectsResource);
        }

        #endregion // Static public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets type name object and objects in string resources by object type.
        /// </summary>
        /// <param name="type">Object type.</param>
        /// <param name="typeNameObjectResource">Type name object in string resources.</param>
        /// <param name="typeNameObjectsResource">Type name objects in string resources.</param>
        static private void _GetTextResourcesNameByType(Type type,
                                                        ref string typeNameObjectResource,
                                                        ref string typeNameObjectsResource)
        {
            if (typeof(Barrier) == type)
            {
                typeNameObjectResource = "Barrier";
                typeNameObjectsResource = "Barriers";
            }

            else if (typeof(Driver) == type)
            {
                typeNameObjectResource = "Driver";
                typeNameObjectsResource = "Drivers";
            }

            else if (typeof(DriverSpecialty) == type)
            {
                typeNameObjectResource = "DriverSpecialty";
                typeNameObjectsResource = "DriverSpecialties";
            }

            else if (typeof(VehicleSpecialty) == type)
            {
                typeNameObjectResource = "VehicleSpecialty";
                typeNameObjectsResource = "VehicleSpecialties";
            }

            else if (typeof(FuelType) == type)
            {
                typeNameObjectResource = "FuelType";
                typeNameObjectsResource = "FuelTypes";
            }

            else if (typeof(Location) == type)
            {
                typeNameObjectResource = "Location";
                typeNameObjectsResource = "Locations";
            }

            else if (typeof(MobileDevice) == type)
            {
                typeNameObjectResource = "MobileDevice";
                typeNameObjectsResource = "MobileDevices";
            }

            else if (typeof(Order) == type)
            {
                typeNameObjectResource = "Order";
                typeNameObjectsResource = "Orders";
            }

            else if (typeof(Route) == type)
            {
                typeNameObjectResource = "DefaultRoute";
                typeNameObjectsResource = "DefaultRoutes";
            }

            else if (typeof(Stop) == type)
            {
                typeNameObjectResource = "Stop";
                typeNameObjectsResource = "Stops";
            }

            else if (typeof(Vehicle) == type)
            {
                typeNameObjectResource = "Vehicle";
                typeNameObjectsResource = "Vehicles";
            }

            else if (typeof(Zone) == type)
            {
                typeNameObjectResource = "Zone";
                typeNameObjectsResource = "Zones";
            }

            else if (typeof(TimeWindowBreak) == type || 
                typeof(DriveTimeBreak) == type || 
                typeof(WorkTimeBreak) == type)
            {
                typeNameObjectResource = "Break";
                typeNameObjectsResource = "Breaks";
            }
            else if (typeof(CustomOrderProperty) == type)
            {
                typeNameObjectResource = "CustomOrderProperty";
                typeNameObjectsResource = "CustomOrderProperties";
            }
            else
            {
                Debug.Assert(false); // NOTE: not supported
            }
        }

        /// <summary>
        /// Show question dialog.
        /// </summary>
        /// <param name="objectName">Name of object to deleting.</param>
        /// <param name="typeNameObjectResource">Type name object in string resources.</param>
        /// <param name="typeNameObjectsResource">Type name objects in string resources.</param>
        /// <returns>User choice "Yes" - true.</returns>
        static private bool _ShowDialog(string objectName, string typeNameObjectResource, string typeNameObjectsResource)
        {
            Debug.Assert(!string.IsNullOrEmpty(typeNameObjectResource));
            Debug.Assert(!string.IsNullOrEmpty(typeNameObjectsResource));

            string typeNameObject = ((string)App.Current.FindResource(typeNameObjectResource)).ToLower();
            string title = string.Format((string)App.Current.FindResource("DeletingDialogTitleFormat"), typeNameObject);

            string message = null;
            if (!string.IsNullOrEmpty(objectName))
                message = string.Format((string)App.Current.FindResource("DeletingDialogTextOne"), objectName, typeNameObject);
            else
            {
                string typeNameObjects = ((string)App.Current.FindResource(typeNameObjectsResource)).ToLower();
                message = string.Format((string)App.Current.FindResource("DeletingDialogTextSome"), typeNameObjects);
            }

            bool dontAsk = false;
            MessageBoxExButtonType result = MessageBoxEx.Show(App.Current.MainWindow, message, title,
                                                              System.Windows.Forms.MessageBoxButtons.YesNo,
                                                              MessageBoxImage.Question,
                                                              (string)App.Current.FindResource("DeletingDialogCheckBoxText"),
                                                              ref dontAsk);
            if (dontAsk)
            {   // update response
                Properties.Settings.Default.IsAllwaysAskBeforeDeletingEnabled = false;
                Properties.Settings.Default.Save();
            }

            return (MessageBoxExButtonType.Yes == result);
        }

        #endregion // Private methods
    }
}
 