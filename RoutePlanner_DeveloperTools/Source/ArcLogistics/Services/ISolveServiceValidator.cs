using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Validates settings for solve services.
    /// </summary>
    internal interface ISolveServiceValidator
    {
        /// <summary>
        /// Checks if the specified service info is valid.
        /// </summary>
        /// <param name="serviceInfo">The reference to the VRP service info to
        /// be validated.</param>
        /// <exception cref="T:System.ApplicationException">
        /// <paramref name="serviceInfo"/> argument is null or does not contain
        /// valid service info.</exception>
        void Validate(VrpServiceInfo serviceInfo);

        /// <summary>
        /// Checks if the specified service info is valid.
        /// </summary>
        /// <param name="serviceInfo">The reference to the Route service info to
        /// be validated.</param>
        /// <exception cref="T:System.ApplicationException">
        /// <paramref name="serviceInfo"/> argument is null or does not contain
        /// valid service info.</exception>
        void Validate(RouteServiceInfo serviceInfo);

        /// <summary>
        /// Checks if the specified service info is valid.
        /// </summary>
        /// <param name="serviceInfo">The reference to the Discovery service info to
        /// be validated.</param>
        /// <exception cref="T:System.ApplicationException">
        /// <paramref name="serviceInfo"/> argument is null or does not contain
        /// valid service info.</exception>
        void Validate(DiscoveryServiceInfo serviceInfo);
    }
}
