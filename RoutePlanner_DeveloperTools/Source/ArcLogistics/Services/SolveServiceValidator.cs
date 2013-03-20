using System;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides default implementation of the
    /// <see cref="T:ESRI.ArcLogistics.Services.ISolveServiceValidator"/> interface.
    /// </summary>
    internal sealed class SolveServiceValidator : ISolveServiceValidator
    {
        /// <summary>
        /// Checks if the specified service info is valid.
        /// </summary>
        /// <param name="serviceInfo">The reference to the VRP service info to
        /// be validated.</param>
        /// <exception cref="T:System.ApplicationException">
        /// <paramref name="serviceInfo"/> argument is null or does not contain
        /// valid service info.</exception>
        public void Validate(VrpServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);
            }

            if (string.IsNullOrEmpty(serviceInfo.RestUrl) ||
                string.IsNullOrEmpty(serviceInfo.SoapUrl) ||
                string.IsNullOrEmpty(serviceInfo.ToolName))
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);
            }
        }

        /// <summary>
        /// Checks if the specified service info is valid.
        /// </summary>
        /// <param name="serviceInfo">The reference to the Route service info to
        /// be validated.</param>
        /// <exception cref="T:System.ApplicationException">
        /// <paramref name="serviceInfo"/> argument is null or does not contain
        /// valid service info.</exception>
        public void Validate(RouteServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);
            }

            if (string.IsNullOrEmpty(serviceInfo.RestUrl) ||
                string.IsNullOrEmpty(serviceInfo.SoapUrl) ||
                string.IsNullOrEmpty(serviceInfo.LayerName))
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);
            }
        }

        /// <summary>
        /// Checks if the specified service info is valid.
        /// </summary>
        /// <param name="serviceInfo">The reference to the Discovery service info to
        /// be validated.</param>
        /// <exception cref="T:System.ApplicationException">
        /// <paramref name="serviceInfo"/> argument is null or does not contain
        /// valid service info.</exception>
        public void Validate(DiscoveryServiceInfo serviceInfo)
        {
            if (serviceInfo != null &&
                (string.IsNullOrEmpty(serviceInfo.RestUrl)))
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);
            }
        }
    }
}
