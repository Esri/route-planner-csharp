using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Discovery service interface.
    /// </summary>
    internal interface IDiscoveryService
    {
        /// <summary>
        /// Initializes discovery service.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Validates directory server state.
        /// </summary>
        void ValidateServerState();

        /// <summary>
        /// Gets full map extent from discovery service.
        /// </summary>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Full map extent.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="knownTypes"/> is null reference.</exception>
        GPEnvelope GetFullMapExtent(IEnumerable<Type> knownTypes);

        /// <summary>
        /// Gets geographic region name.
        /// </summary>
        /// <param name="request">Discovery request.</param>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Region name if successfully found, otherwise - empty string.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="request"/> or <paramref name="knownTypes"/> is null reference.
        /// </exception>
        string GetRegionName(SubmitDiscoveryRequest request, IEnumerable<Type> knownTypes);
    }
}
