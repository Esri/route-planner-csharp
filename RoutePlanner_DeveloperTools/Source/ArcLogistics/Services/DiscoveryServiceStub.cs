using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Discovery service stub to do nothing if services config doesn't contain
    /// discovery service settings.
    /// </summary>
    internal class DiscoveryServiceStub : IDiscoveryService
    {
        #region Public methods

        /// <summary>
        /// Initializes discovery service.
        /// </summary>
        public void Initialize()
        {
            // Do nothing.
        }

        /// <summary>
        /// Validates directory server state.
        /// </summary>
        public void ValidateServerState()
        {
            // Do nothing.
        }

        /// <summary>
        /// Gets full map extent from discovery service.
        /// </summary>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Returns null.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="knownTypes"/> is null reference.</exception>
        public GPEnvelope GetFullMapExtent(IEnumerable<Type> knownTypes)
        {
            // Validate input.
            if (knownTypes == null)
                throw new ArgumentNullException("knownTypes");

			// Return empty extent.
            var result = new GPEnvelope();
            result.SpatialReference = new GPSpatialReference();
            return result;
        }

        /// <summary>
        /// Gets geographic region name.
        /// </summary>
        /// <param name="request">Discovery request.</param>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Returns empty string.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="request"/> or <paramref name="knownTypes"/> is null reference.
        /// </exception>
        public string GetRegionName(SubmitDiscoveryRequest request, IEnumerable<Type> knownTypes)
        {
            // Validate inputs.
            if (request == null)
                throw new ArgumentNullException("request");

            if (knownTypes == null)
                throw new ArgumentNullException("knownTypes");

            return string.Empty;
        }

        #endregion
    }
}
