using System;
using System.Collections.Generic;
using System.Net;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Combines multiple <see cref="IWebProxy"/> objects into a single one dispatching proxy
    /// requests using <see cref="Uri.Scheme"/> property.
    /// </summary>
    internal sealed class AggregateWebProxy : IWebProxy
    {
        /// <summary>
        /// Gets or sets a reference to the credentials to be used for proxy authentication.
        /// </summary>
        public ICredentials Credentials
        {
            get
            {
                return _credentials;
            }

            set
            {
                if (_credentials != value)
                {
                    _credentials = value;
                    foreach (var proxy in _proxies.Values)
                    {
                        proxy.Credentials = _credentials;
                    }
                }
            }
        }

        /// <summary>
        /// Gets proxy URI for the specified resource.
        /// </summary>
        /// <param name="destination">The URI of the resource to get proxy for.</param>
        /// <returns>A URI of the proxy to be used for accessing
        /// <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is a null
        /// reference.</exception>
        public Uri GetProxy(Uri destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            var proxy = default(IWebProxy);
            if (!_proxies.TryGetValue(destination.Scheme, out proxy))
            {
                return destination;
            }

            return proxy.GetProxy(destination);
        }

        /// <summary>
        /// Gets a value indicating if the proxy should be used for the specified resource.
        /// </summary>
        /// <param name="host">The URI of the resource to check proxy usage for.</param>
        /// <returns>True if and only if proxy server should be used for accessing
        /// the resource.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is a null
        /// reference.</exception>
        public bool IsBypassed(Uri host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }

            var proxy = default(IWebProxy);
            if (!_proxies.TryGetValue(host.Scheme, out proxy))
            {
                return false;
            }

            return proxy.IsBypassed(host);
        }

        /// <summary>
        /// Adds sub-proxy for the specified scheme.
        /// </summary>
        /// <param name="scheme">A URI scheme to use the specified proxy for.</param>
        /// <param name="proxy">A proxy to be used for the specified scheme.</param>
        /// <exception cref="ArgumentNullException"><paramref name="scheme"/> or
        /// <paramref name="proxy"/> is a null reference.</exception>
        /// <remarks>Sets <see cref="IWebProxy.Credentials"/> property for
        /// the <paramref name="proxy"/> object to current proxy credentials.</remarks>
        public void AddProxy(string scheme, IWebProxy proxy)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException("scheme");
            }

            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }

            _proxies.Add(scheme, proxy);
            proxy.Credentials = this.Credentials;
        }

        /// <summary>
        /// Maps URI schemes to appropriate proxies.
        /// </summary>
        private IDictionary<string, IWebProxy> _proxies = new Dictionary<string, IWebProxy>();

        /// <summary>
        /// Credentials to be used by the current proxy.
        /// </summary>
        private ICredentials _credentials;
    }
}
