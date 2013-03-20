using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Provides services for synchronizing project state with tracking service data
    /// and vice versa.
    /// </summary>
    internal interface ISynchronizationService
    {
        /// <summary>
        /// Updates the specified project information from the tracking service.
        /// </summary>
        /// <param name="project">Project to be updated.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="project"/> is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        void UpdateFromServer(IProject project);

        /// <summary>
        /// Adds specified devices to the tracking service.
        /// </summary>
        /// <param name="trackingIds">Devices to be added.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        void AddDevices(IEnumerable<string> trackingIds);

        /// <summary>
        /// Updates specified devices at the tracking service.
        /// </summary>
        /// <param name="devices">Mapping from old tracking ids to new ones.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        void UpdateTrackingIds(IDictionary<string, string> devices);

        /// <summary>
        /// Deletes specified devices from the tracking service.
        /// </summary>
        /// <param name="project">The project containing devices to be deleted from the server.
        /// </param>
        /// <param name="trackingIds">Devices to be deleted.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> or
        /// <paramref name="project"/> is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        void DeleteDevices(IProject project, IEnumerable<string> trackingIds);
    }
}
