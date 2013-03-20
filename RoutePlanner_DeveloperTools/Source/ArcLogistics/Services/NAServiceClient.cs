using System.ServiceModel;
using ESRI.ArcLogistics.NAService;
using System.ServiceModel.Channels;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// NAServiceClient class.
    /// </summary>
    internal class NAServiceClient
        : RoutingServiceClientBase<NAServerPortClient, NAServerPort>
    {
        public NAServiceClient(
            string url,
            AgsServerConnection connection)
            : base(url, connection)
        {
        }

        public NAServerNetworkDescription GetNetworkDescription(string NALayerName)
        {
            ServiceMethod<NAServerNetworkDescription> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GetNetworkDescription(NALayerName);
                }

            };

            return Invoke<NAServerNetworkDescription>(method);
        }

        public NAServerSolverParams GetSolverParameters(string NALayerName)
        {
            ServiceMethod<NAServerSolverParams> method = (client) =>
            {
                using (var contextScope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                        _CreateRequestMessageProperty();

                    return client.GetSolverParameters(NALayerName);
                }
            };

            return Invoke<NAServerSolverParams>(method);
        }

        protected override NAServerPortClient CreateInnerClient(
            string url)
        {
            var serviceUrl = this.QueryServiceUrl(url);
            return ServiceHelper.CreateServiceClient<NAServerPortClient>(
                "NAServiceBinding",
                serviceUrl);
        }

        /// <summary>
        /// Create message property which add "referer" header to HTTP request.
        /// </summary>
        /// <returns>HttpRequestMessageProperty.</returns>
        private HttpRequestMessageProperty _CreateRequestMessageProperty()
        {
            var httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Headers.Add(AgsHelper.RefererParameterName, AgsHelper.RefererValue);

            return httpRequestProperty;
        }
    }

}
