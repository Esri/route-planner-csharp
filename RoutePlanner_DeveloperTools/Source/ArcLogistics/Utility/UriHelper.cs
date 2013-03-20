using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides helper methods for URI's manipulation.
    /// </summary>
    internal static class UriHelper
    {
        #region public static methods
        /// <summary>
        /// Makes service url from the base and relative urls.
        /// </summary>
        /// <param name="serviceUrl">A base url to the routing REST service.</param>
        /// <param name="relativeUrl">A relative url to add to the <paramref name="serviceUrl"/>.</param>
        /// <returns>A complete service url.</returns>
        public static string Concat(string serviceUrl, string relativeUrl)
        {
            Debug.Assert(serviceUrl != null);
            Debug.Assert(relativeUrl != null);

            serviceUrl = serviceUrl.TrimEnd('/') + "/";
            var baseUri = new Uri(serviceUrl.Trim());
            var relativeUri = new Uri(relativeUrl.Trim(), UriKind.RelativeOrAbsolute);
            var path = relativeUri.IsAbsoluteUri ?
                relativeUri.PathAndQuery : relativeUrl;
            var serviceUri = new Uri(baseUri, path);

            return serviceUri.ToString();
        }
        #endregion private methods
    }
}
