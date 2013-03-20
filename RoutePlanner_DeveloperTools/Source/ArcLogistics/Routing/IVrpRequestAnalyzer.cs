using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides VRP requests analyzing facilities.
    /// </summary>
    internal interface IVrpRequestAnalyzer
    {
        /// <summary>
        /// Checks if the VRP request is short enough to be serviced with the
        /// synchronous VRP service.
        /// </summary>
        /// <param name="request">The reference to the request object to be
        /// analyzed.</param>
        /// <returns>True if the request could be executed with a synchronous VRP
        /// service.</returns>
        bool CanExecuteSyncronously(SubmitVrpJobRequest request);
    }
}
