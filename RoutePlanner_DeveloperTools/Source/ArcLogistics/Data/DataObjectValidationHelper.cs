using System.Collections;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class which raises property changed event to renew validation.
    /// </summary>
    internal static class DataObjectValidationHelper
    {
        /// <summary>
        /// Raise propepty changed event for property 'name' for objects, which property is the same
        /// as parametr.
        /// </summary>
        /// <param name="collection">IEnumerable collection which items must be checked.</param>
        /// <param name="name">Name.</param>
        internal static void RaisePropertyChangedForDuplicate(IEnumerable collection,
            string name) 
        {
            // Check that collection not null.
            Debug.Assert(collection != null);

            foreach (var dataObject in collection)
                if ((dataObject as ISupportName).Name == name)
                    (dataObject as IForceNotifyPropertyChanged).RaisePropertyChangedEvent("Name");
        }

        /// <summary>
        /// Raise property changed event for <see cref="Route.Driver"/> property for routes
        /// with drivers contained in the specified collection of drivers.
        /// </summary>
        /// <param name="collection">IEnumerable collection with routes.</param>
        /// <param name="drivers">Collection of drivers to raise event for.</param>
        internal static void RaisePropertyChangedForRoutesDrivers(IEnumerable collection,
            params Driver[] drivers)
        {
            Debug.Assert(collection != null);
            Debug.Assert(drivers != null);

            if (drivers.Length == 0)
            {
                return;
            }

            var driversToFind = drivers.ToHashSet();
            foreach (Route route in collection)
            {
                if (driversToFind.Contains(route.Driver))
                {
                    var notifier = (IForceNotifyPropertyChanged)route;
                    notifier.RaisePropertyChangedEvent(Route.PropertyNameDriver);
                }
            }
        }

        /// <summary>
        /// Raise property changed event for <see cref="Route.Vehicle"/> property for routes
        /// with vehicles contained in the specified collection of vehicles.
        /// </summary>
        /// <param name="collection">IEnumerable collection with routes.</param>
        /// <param name="vehicles">Collection of vehicles to raise event for.</param>
        internal static void RaisePropertyChangedForRoutesVehicles(IEnumerable collection,
            params Vehicle[] vehicles)
        {
            Debug.Assert(collection != null);
            Debug.Assert(vehicles != null);

            if (vehicles.Length == 0)
            {
                return;
            }

            var vehiclesToFind = vehicles.ToHashSet();
            foreach (Route route in collection)
            {
                if (vehiclesToFind.Contains(route.Vehicle))
                {
                    var notifier = (IForceNotifyPropertyChanged)route;
                    notifier.RaisePropertyChangedEvent(Route.PropertyNameVehicle);
                }
            }
        }

        /// <summary>
        /// Raise propepty changed event for property 'Breaks' for routes, which property is the same
        /// as parameter.
        /// </summary>
        /// <param name="collection">IEnumerable collection with routes.</param>
        /// <param name="breaks">Breaks.</param>
        internal static void RaisePropertyChangedForRoutesBreaks(IEnumerable collection,
            Breaks breaks)
        {
            Debug.Assert(collection != null);

            foreach (Route route in collection)
                (route as IForceNotifyPropertyChanged).RaisePropertyChangedEvent(Route.PropertyNameBreaks);
        }
    }
}
