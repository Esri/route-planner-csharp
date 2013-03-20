using System.Globalization;
using System.Threading;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides helpers for Vrp and Routing requests builders.
    /// </summary>
    internal static class RequestBuildingHelper
    {
        /// <summary>
        /// Gets direction language name for sending to ArcGIS server.
        /// </summary>
        /// <returns>Current direction language name.</returns>
        public static string GetDirectionsLanguage()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }

        /// <summary>
        /// Gets direction length units.
        /// </summary>
        /// <returns>NA network attribute units for current thread.</returns>
        public static NANetworkAttributeUnits GetDirectionsLengthUnits()
        {
            var ri = new RegionInfo(Thread.CurrentThread.CurrentCulture.LCID);
            return ri.IsMetric ? NANetworkAttributeUnits.esriNAUKilometers :
                                 NANetworkAttributeUnits.esriNAUMiles;
        }
    }
}
