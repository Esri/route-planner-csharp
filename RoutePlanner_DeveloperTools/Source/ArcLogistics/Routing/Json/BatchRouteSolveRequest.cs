using System;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// BatchRouteSolveRequest class.
    /// </summary>
    internal class BatchRouteSolveRequest
    {
        public BatchRouteSolveRequest(RouteSolveRequest[] requests)
        {
            this.Requests = requests;
        }

        public RouteSolveRequest[] Requests { get; set; }
    }
}