/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using ESRI.ArcLogistics.LoadBalanceService;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Implements <see cref="IRoutingServiceUrlProvider"/> using load-balancing
    /// web service to obtain routing service urls.
    /// </summary>
    internal class RoutingServiceUrlProvider : IRoutingServiceUrlProvider
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RoutingServiceUrlProvider class.
        /// </summary>
        /// <param name="serviceUrl">A url to the load-balancing web service.</param>
        /// <param name="serverTitle">A title of the server to use load-balancing for.</param>
        /// <param name="certificateValidationSettings">The reference to the certificate
        /// validation settings object.</param>
        public RoutingServiceUrlProvider(
            string serviceUrl,
            string serverTitle,
            ICertificateValidationSettings certificateValidationSettings)
        {
            Debug.Assert(serviceUrl != null, "Expects non-null serviceUrl");
            Debug.Assert(serverTitle != null, "Expects non-null serverTitle");
            Debug.Assert(
                certificateValidationSettings != null,
                "Expects non-null certificateValidationSettings");

            _serviceUrl = serviceUrl;
            _serverTitle = serverTitle;

            var uri = new Uri(serviceUrl);
            _serviceScheme = uri.Scheme;

            _certificateValidationSettings = certificateValidationSettings;

            _serviceWrapper = new RetriableInvocationWrapper(
                MAX_RETRY_COUNT,
                _PrepareForRetry,
                _TranslateExceptions);
        }
        #endregion

        #region IRoutingServiceUrlProvider Members
        /// <summary>
        /// Obtains url to the routing service from the load-balancing web service.
        /// </summary>
        /// <returns>Url to the route service.</returns>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">An error
        /// occured during retrieval of the service url.
        /// </exception>
        public string QueryServiceUrl()
        {
            var result = _GetServiceUrl();
            if (result.Status != StatusCode.Succeed)
            {
                // Service url obtaining failed
                throw _CreateException(result);
            }

            var uriBuilder = new UriBuilder(result.ServerUrl);
            uriBuilder.Scheme = _serviceScheme;
            uriBuilder.Port = -1;

            var url = uriBuilder.Uri;
            _certificateValidationSettings.SkipNameValidation(url.Host);

            return url.ToString();
        }
        #endregion

        #region public static methods
        /// <summary>
        /// Checks if the specified url points to the load balancing service.
        /// </summary>
        /// <param name="serviceUrl">Url to check for service presence.</param>
        /// <returns>true if and only if the <paramref name="serviceUrl"/>
        /// points to the load balancing service.</returns>
        public static bool HasLoadBalanceService(string serviceUrl)
        {
            var hasService = false;

            try
            {
                var metadataUri = new Uri(serviceUrl + "?wsdl");
                var mexClient = new MetadataExchangeClient(
                    metadataUri,
                    MetadataExchangeClientMode.HttpGet);
                var metadataRetrievalWrapper = new RetriableInvocationWrapper(
                    MAX_RETRY_COUNT,
                    _PrepareForRetry);
                var metaDocs = metadataRetrievalWrapper.Invoke(
                    () => mexClient.GetMetadata());
                var importer = new WsdlImporter(metaDocs);
                var contracts = importer.ImportAllContracts();
                foreach (var contract in contracts)
                {
                    if (string.Equals(
                        contract.Name,
                        LOAD_BALANCE_SERVICE_CONTRACT_NAME,
                        StringComparison.InvariantCulture))
                    {
                        hasService = true;
                        break;
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                Logger.Warning(e);
            }
            catch (Exception e)
            {
                if (!ServiceHelper.IsCommunicationError(e))
                {
                    throw;
                }

                Logger.Warning(e);
            }

            return hasService;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Checks if the url retrieval could be retried upon the specified
        /// exception.
        /// </summary>
        /// <param name="exception">The exception thrown upon url retrieval.</param>
        /// <returns>True if and only if retry could be attempted.</returns>
        private static bool _CanRetry(Exception exception)
        {
            while (exception != null)
            {
                var webException = exception as WebException;
                if (webException != null)
                {
                    return WebHelper.IsTransientError(webException);
                }

                var socketException = exception as SocketException;
                if (socketException != null)
                {
                    return _IsTransientError(socketException);
                }

                exception = exception.InnerException;
            }

            return false;
        }

        /// <summary>
        /// Prepares retrying of the url retrieval upon the specified
        /// exception.
        /// </summary>
        /// <param name="exception">The exception thrown upon url retrieval.</param>
        /// <returns>True if and only if retry could be attempted.</returns>
        private static bool _PrepareForRetry(Exception exception)
        {
            if (!_CanRetry(exception))
            {
                return false;
            }

            Thread.Sleep(RETRY_WAIT_TIME);

            return true;
        }

        /// <summary>
        /// Checks if the specified socket exception denotes transient error.
        /// </summary>
        /// <param name="exception">The exception object to be checked.</param>
        /// <returns>True if and only if the exception denotes transient error.</returns>
        private static bool _IsTransientError(SocketException exception)
        {
            switch (exception.SocketErrorCode)
            {
                case SocketError.ConnectionAborted:
                    return true;

                case SocketError.ConnectionReset:
                    return true;

                default:
                    return false;
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Queries load-balancing service for routing service url.
        /// </summary>
        /// <returns>Result of the url request.</returns>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">An error
        /// occured during retrieval of the service url.
        /// </exception>
        private LoadBalanceResult _GetServiceUrl()
        {
            var result = default(LoadBalanceResult);
            _serviceWrapper.Invoke(() =>
            {
                var client = ServiceHelper.CreateServiceClient<LoadBalanceServiceSoapClient>(
                    "LoadBalanceServiceBinding",
                    _serviceUrl);

                try
                {
                    result = client.GetServiceUrl();
                }
                finally
                {
                    ServiceHelper.CloseCommObject(client);
                }
            });

            return result;
        }

        /// <summary>
        /// Translates the specified exceptions into ones specific for
        /// <see cref="QueryServiceUrl"/> method.
        /// </summary>
        /// <param name="exception">The exception to be translated.</param>
        /// <returns>Translated exception or null if the exception cannot be
        /// translated and the original one should be thrown.</returns>
        private Exception _TranslateExceptions(Exception exception)
        {
            var isCommunicationError =
                exception is FaultException ||
                ServiceHelper.IsCommunicationError(exception);

            if (isCommunicationError)
            {
                return ServiceHelper.CreateCommException(_serverTitle, exception);
            }

            return null;
        }

        /// <summary>
        /// Creates exception to be thrown upon load balancing service failure.
        /// </summary>
        /// <param name="result">Instance of the service url retrieval result
        /// to create exception for.</param>
        /// <returns>A new instance of the class derieved from <see cref="System.Exception"/>
        /// containing error details.</returns>
        private Exception _CreateException(LoadBalanceResult result)
        {
            Debug.Assert(result != null, "Result should not be null");
            Debug.Assert(
                result.Status != StatusCode.Succeed,
                "Cannot create exceptions upon success");

            switch (result.Status)
            {
                case StatusCode.NoServerAvailable:
                    {
                        var message = string.Format(
                            Properties.Messages.Error_ServerTemporaryUnavailable,
                            _serverTitle);

                        return new CommunicationException(
                            message,
                            _serverTitle,
                            CommunicationError.ServiceTemporaryUnavailable);
                    }

                default:
                    {
                        var message = string.Format(
                            Properties.Messages.Error_CannotRetrieveServiceUrl,
                            _serverTitle,
                            result.Status);

                        return new CommunicationException(
                            message,
                            _serverTitle,
                            CommunicationError.Unknown);
                    }
            }
        }
        #endregion

        #region private constants
        /// <summary>
        /// Name of the load balance service contract to be used for checking
        /// if there is load balance service at the specified url.
        /// </summary>
        private const string LOAD_BALANCE_SERVICE_CONTRACT_NAME = "LoadBalanceServiceSoap";

        /// <summary>
        /// Maximum number of service url obtaining retries.
        /// </summary>
        private const int MAX_RETRY_COUNT = 4;

        /// <summary>
        /// Time (in milliseconds) to wait before retrying url retrieval.
        /// </summary>
        private const int RETRY_WAIT_TIME = 500;
        #endregion

        #region private fields
        /// <summary>
        /// Stores url to the load-balancing web service.
        /// </summary>
        private string _serviceUrl;

        /// <summary>
        /// Stores url scheme to be used for urls returned by the load balancer.
        /// </summary>
        private string _serviceScheme;

        /// <summary>
        /// Title of the ArcGIS server instance.
        /// </summary>
        private string _serverTitle;

        /// <summary>
        /// Wraps calls to the load-balancing service.
        /// </summary>
        private IInvocationWrapper _serviceWrapper;

        /// <summary>
        /// The reference to the remote certificate validation settings object.
        /// </summary>
        private ICertificateValidationSettings _certificateValidationSettings;
        #endregion
    }
}
