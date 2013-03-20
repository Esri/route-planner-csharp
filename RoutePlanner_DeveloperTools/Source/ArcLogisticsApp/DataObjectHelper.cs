using System;
using System.Diagnostics;
using System.Collections;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// ScheduleHelper class.
    /// </summary>
    internal class DataObjectHelper
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static DataObject GetPrescribedObject(Type type, Guid id)
        {
            if (null == App.Current)
                return null;
            Project project = App.Current.Project;
            if (null == project)
                return null;

            DataObject obj = null;
            if (type == typeof(Barrier))
                obj = project.Barriers.SearchById(id);

            else if (type == typeof(Driver))
                obj = _GetObject(id, project.Drivers.GetEnumerator());

            else if (type == typeof(DriverSpecialty))
                obj = _GetObject(id, project.DriverSpecialties.GetEnumerator());

            else if (type == typeof(FuelType))
                obj = _GetObject(id, project.FuelTypes.GetEnumerator());

            else if (type == typeof(Location))
                obj = _GetObject(id, project.Locations.GetEnumerator());

            else if (type == typeof(MobileDevice))
                obj = _GetObject(id, project.MobileDevices.GetEnumerator());

            else if (type == typeof(Order))
                obj = project.Orders.SearchById(id);

            else if (type == typeof(Route))
                obj = project.Schedules.SearchRoute(id);

            else if (type == typeof(Schedule))
                obj = project.Schedules.SearchById(id);

            else if (type == typeof(Vehicle))
                obj = _GetObject(id, project.Vehicles.GetEnumerator());

            else if (type == typeof(VehicleSpecialty))
                obj = _GetObject(id, project.VehicleSpecialties.GetEnumerator());

            else if (type == typeof(Zone))
                obj = _GetObject(id, project.Zones.GetEnumerator());

            else
            {
                Debug.Assert(false); // NOTE: not supported
            }

            return obj;
        }

        #endregion // Public methods

        #region Private function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static private DataObject _GetObject(Guid id, IEnumerator enumerator)
        {
            DataObject obj = null;

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                DataObject currentObj = (DataObject)enumerator.Current;
                if (id.Equals(currentObj.Id))
                {
                    obj = currentObj;
                    break;
                }
            }

            return obj;
        }

        #endregion // Private function
    }
}
