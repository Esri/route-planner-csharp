using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.App.Services
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.Services.ICertificateValidationSettings"/>
    /// and provides certificate validation callback.
    /// </summary>
    internal class ConfigurableCertificateValidator : ICertificateValidationSettings
    {
        #region ICertificateValidationSettings Members
        /// <summary>
        /// Disables name mismatching checks for certificates received from the
        /// specified host.
        /// </summary>
        /// <param name="host">The name of the host to disable name mismatching
        /// checks for.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="host"/>
        /// is a null reference.</exception>
        public void SkipNameValidation(string host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }

            _hosts.Add(host);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Performs certificate validation allowing name mismatches for a set
        /// of configured hosts.
        /// </summary>
        /// <param name="sender">The reference to an object providing context
        /// information for certificate validation.</param>
        /// <param name="certificate">The reference to the remote certificate being
        /// validated.</param>
        /// <param name="chain">The reference to the chain of certificate
        /// authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">Errors for the certificate
        /// being validated.</param>
        /// <returns>True if and only if the certificate can be accepted.</returns>
        public bool ValidateRemoteCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (sslPolicyErrors != SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                return false;
            }

            var request = sender as HttpWebRequest;
            if (request == null)
            {
                return false;
            }

            if (_hosts.Contains(request.Address.Host))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores set of host names to disable name mismatching checks for.
        /// </summary>
        private HashSet<string> _hosts = new HashSet<string>();
        #endregion
    }
}
