using System;

namespace ESRI.ArcLogistics.Routing.Json
{
   /// <summary>
    /// RouteSolveRequest class.
    /// </summary>
    internal class RouteSolveRequest
    {
        /// <summary>
        /// Stops.
        /// </summary>
        [QueryParameter(Name = "stops")]
        public RouteStopsRecordSet Stops { get; set; }

        /// <summary>
        /// Point barriers.
        /// </summary>
        [QueryParameter(Name = "Barriers")]
        public RouteRecordSet PointBarriers { get; set; }

        /// <summary>
        /// Polyline barriers.
        /// </summary>
        [QueryParameter(Name = "PolylineBarriers")]
        public RouteRecordSet PolylineBarriers { get; set; }

        /// <summary>
        /// Polygon barriers.
        /// </summary>
        [QueryParameter(Name = "PolygonBarriers")]
        public RouteRecordSet PolygonBarriers { get; set; }

        /// <summary>
        /// Return directions.
        /// </summary>
        [QueryParameter(Name = "returnDirections")]
        public bool ReturnDirections { get; set; }

        /// <summary>
        /// Return routes.
        /// </summary>
        [QueryParameter(Name = "returnRoutes")]
        public bool ReturnRoutes { get; set; }

        /// <summary>
        /// Return stops.
        /// </summary>
        [QueryParameter(Name = "returnStops")]
        public bool ReturnStops { get; set; }

        /// <summary>
        /// Return barriers.
        /// </summary>
        [QueryParameter(Name = "returnBarriers")]
        public bool ReturnBarriers { get; set; }

        /// <summary>
        /// Out SR.
        /// </summary>
        [QueryParameter(Name = "outSR")]
        public int OutSR { get; set; } // WKID

        /// <summary>
        /// Ignore invalid locations.
        /// </summary>
        [QueryParameter(Name = "ignoreInvalidLocations")]
        public bool IgnoreInvalidLocations { get; set; }

        /// <summary>
        /// Output lines.
        /// </summary>
        [QueryParameter(Name = "outputLines")]
        public string OutputLines { get; set; }

        /// <summary>
        /// Find best sequence.
        /// </summary>
        [QueryParameter(Name = "findBestSequence")]
        public bool FindBestSequence { get; set; }

        /// <summary>
        /// Preserve first stop.
        /// </summary>
        [QueryParameter(Name = "preserveFirstStop")]
        public bool PreserveFirstStop { get; set; }

        /// <summary>
        /// Preserve last stop.
        /// </summary>
        [QueryParameter(Name = "preserveLastStop")]
        public bool PreserveLastStop { get; set; }

        /// <summary>
        /// Use Time Windows.
        /// </summary>
        [QueryParameter(Name = "useTimeWindows")]
        public bool UseTimeWindows { get; set; }

        /// <summary>
        /// Start time.
        /// </summary>
        [QueryParameter(Name = "startTime")]
        public string StartTime { get; set; }

        /// <summary>
        /// Accumulate attribute names.
        /// </summary>
        [QueryParameter(Name = "accumulateAttributeNames")]
        public string AccumulateAttributeNames { get; set; }

        /// <summary>
        /// Impedance attribute name.
        /// </summary>
        [QueryParameter(Name = "impedanceAttributeName")]
        public string ImpedanceAttributeName { get; set; }

        /// <summary>
        /// Restriction attribute names.
        /// </summary>
        [QueryParameter(Name = "restrictionAttributeNames")]
        public string RestrictionAttributeNames { get; set; }

        /// <summary>
        /// Attribute parameters.
        /// </summary>
        [QueryParameter(Name = "attributeParameterValues")]
        public string AttributeParameters { get; set; }

        /// <summary>
        /// Restrict UTurns.
        /// </summary>
        [QueryParameter(Name = "restrictUTurns")]
        public string RestrictUTurns { get; set; }

        /// <summary>
        /// Use hierarchy.
        /// </summary>
        [QueryParameter(Name = "useHierarchy")]
        public bool? UseHierarchy { get; set; }

        /// <summary>
        /// Directions language.
        /// </summary>
        [QueryParameter(Name = "directionsLanguage")]
        public string DirectionsLanguage { get; set; }

        /// <summary>
        /// Output geometry precision.
        /// </summary>
        [QueryParameter(Name = "outputGeometryPrecision")]
        public double? OutputGeometryPrecision { get; set; }

        /// <summary>
        /// Output geometry precision units.
        /// </summary>
        [QueryParameter(Name = "outputGeometryPrecisionUnits")]
        public int? OutputGeometryPrecisionUnits { get; set; }

        /// <summary>
        /// Directions length units.
        /// </summary>
        [QueryParameter(Name = "directionsLengthUnits")]
        public string DirectionsLengthUnits { get; set; }

        /// <summary>
        /// Directions time attribute name.
        /// </summary>
        [QueryParameter(Name = "directionsTimeAttributeName")]
        public string DirectionsTimeAttributeName { get; set; }

        /// <summary>
        /// Output format.
        /// </summary>
        [QueryParameter(Name = "f")]
        public string OutputFormat { get; set; }
    }
}