using System;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// VrpResult class.
    /// </summary>
    internal class VrpResultsResponse
    {
        /// <summary>
        /// Gets or sets reference to the route result object.
        /// </summary>
        public GPRouteResult RouteResult
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets value of the HRESULT code returned by the solve.
        /// </summary>
        public int SolveHR
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ID of the VRP service job which produced this result.
        /// </summary>
        public string JobID
        {
            get;
            set;
        }

        public bool SolveSucceeded { get; set; }

        public JobMessage[] Messages { get; set; }
    }
}
