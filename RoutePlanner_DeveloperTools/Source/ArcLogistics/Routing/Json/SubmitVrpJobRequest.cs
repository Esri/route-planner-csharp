using System;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// SubmitVrpJobRequest class.
    /// </summary>
    internal class SubmitVrpJobRequest
    {
        [QueryParameter(Name = "routes")]
        public GPRecordSet Routes { get; set; }

        [QueryParameter(Name = "depots")]
        public GPFeatureRecordSetLayer Depots { get; set; }

        [QueryParameter(Name = "route_renewals")]
        public GPRecordSet Renewals { get; set; }

        [QueryParameter(Name = "breaks")]
        public GPRecordSet Breaks { get; set; }

        /// <summary>
        /// Current analysis region.
        /// </summary>
        [QueryParameter(Name = "analysis_region")]
        public string AnalysisRegion
        {
            get;
            set;
        }

        [QueryParameter(Name = "orders")]
        public GPFeatureRecordSetLayer Orders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if generated routes should clustered around certain
        /// point.
        /// </summary>
        /// <remarks>This option could not be set if route zones are used.</remarks>
        [QueryParameter(Name = "spatially_cluster_routes")]
        public bool SpatiallyClusterRoutes { get; set; }

        [QueryParameter(Name = "route_zones")]
        public GPFeatureRecordSetLayer RouteZones { get; set; }

        [QueryParameter(Name = "order_pairs")]
        public GPRecordSet OrderPairs { get; set; }

        [QueryParameter(Name = "point_barriers")]
        public GPFeatureRecordSetLayer PointBarriers { get; set; }

        [QueryParameter(Name = "line_barriers")]
        public GPFeatureRecordSetLayer LineBarriers { get; set; }

        [QueryParameter(Name = "polygon_barriers")]
        public GPFeatureRecordSetLayer PolygonBarriers { get; set; }

        [QueryParameter(Name = "attribute_parameter_values")]
        public GPRecordSet NetworkParams { get; set; }

        [QueryParameter(Name = "restrictions")]
        public string Restrictions { get; set; }

        [QueryParameter(Name = "default_date")]
        public long Date { get; set; }

        /// <summary>
        /// Gets or sets a value specifying u-turn policy.
        /// </summary>
        [QueryParameter(Name = "uturn_policy")]
        public string UTurnPolicy { get; set; }

        [QueryParameter(Name = "use_hierarchy_in_analysis")]
        public bool UseHierarchyInAnalysis { get; set; }

        /// <summary>
        /// Gets or sets a value specifying time window factor.
        /// </summary>
        [QueryParameter(Name = "time_window_factor")]
        public string TWPreference { get; set; }

        [QueryParameter(Name = "exclude_restricted_portions_of_the_network")]
        public bool ExcludeRestrictedStreets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if driving directions should be generated and returned
        /// within VRP Solve response.
        /// </summary>
        [QueryParameter(Name = "populate_directions")]
        public bool PopulateDirections { get; set; }

        /// <summary>
        /// Gets or sets a value specifying generated directions language.
        /// </summary>
        [QueryParameter(Name = "directions_language")]
        public string DirectionsLanguage { get; set; }

        /// <summary>
        /// Gets or sets a value specifying generated directions style.
        /// </summary>
        [QueryParameter(Name = "directions_style_name")]
        public string DirectionsStyleName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if route shapes should be generated and returned
        /// within VRP Solve response.
        /// </summary>
        [QueryParameter(Name = "populate_route_lines")]
        public bool PopulateRouteLines { get; set; }

        /// <summary>
        /// Gets or sets a value specifying simplification tolerance for route shapes.
        /// </summary>
        [QueryParameter(Name = "route_line_simplification_tolerance")]
        public GPLinearUnit RouteLineSimplificationTolerance { get; set; }

        [QueryParameter(Name = "env:outSR")]
        public int EnvOutSR { get; set; } // WKID

        [QueryParameter(Name = "env:processSR")]
        public int EnvProcessSR { get; set; } // WKID

        [QueryParameter(Name = "save_output_layer")]
        public bool SaveOutputLayer { get; set; }

        [QueryParameter(Name = "f")]
        public string OutputFormat { get; set; }

        /// <summary>
        /// Flag to include M values to results.
        /// </summary>
        [QueryParameter(Name = "returnM")]
        public bool ReturnM { get; set; }

        // operation info (for logging only)
        public SolveOperationType OperationType { get; set; }
        public DateTime OperationDate { get; set; }
    }
}