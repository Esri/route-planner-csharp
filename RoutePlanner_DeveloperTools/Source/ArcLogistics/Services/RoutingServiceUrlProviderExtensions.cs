using System;
using System.Diagnostics;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Helper extensions for <see cref="IRoutingServiceUrlProvider"/> interface.
    /// </summary>
    internal static class RoutingServiceUrlProviderExtensions
    {
        /// <summary>
        /// Makes service url using <see cref="IRoutingServiceUrlProvider"/>
        /// instance to obtain a server url and path and query components from
        /// <paramref name="baseUrl"/>.
        /// </summary>
        /// <param name="self">An <see cref="IRoutingServiceUrlProvider"/> to
        /// obtain server url from.</param>
        /// <param name="baseUrl">A url whose path and query components
        /// will be used for making service url.</param>
        /// <returns>A new routing service url.</returns>
        public static string MakeServiceUrl(
            this IRoutingServiceUrlProvider self,
            string baseUrl)
        {
            Debug.Assert(self != null, "Expects a non-null service url provider.");

            return UriHelper.Concat(self.QueryServiceUrl(), baseUrl);
        }
    }
}
