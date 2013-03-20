using System;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// BatchRouteSolveResponse class.
    /// </summary>
    internal class BatchRouteSolveResponse
    {
        public BatchRouteSolveResponse(RouteSolveResponse[] responses)
        {
            this.Responses = responses;
        }

        public RouteSolveResponse[] Responses { get; set; }
    }
}