using System;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GPRouteResult class.
    /// </summary>
    internal class GPRouteResult
    {
        public GPFeatureRecordSetLayer Routes { get; set; }
        public GPRecordSet Stops { get; set; }

        /// <summary>
        /// Gets or sets a reference to record-set with information about stops with violations.
        /// </summary>
        public GPRecordSet ViolatedStops { get; set; }

        /// <summary>
        /// Gets or sets a reference to feature record-set with driving directions.
        /// </summary>
        public GPFeatureRecordSetLayer Directions { get; set; }
    }
}
