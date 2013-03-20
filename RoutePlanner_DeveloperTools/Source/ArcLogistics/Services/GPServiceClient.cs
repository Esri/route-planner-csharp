using System.Diagnostics;
using System.ServiceModel;
using ESRI.ArcLogistics.GPService;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// GPServiceClient class.
    /// </summary>
    internal class GPServiceClient
        : RoutingServiceClientBase<GPServerPortClient, GPServerPort>
    {
        public GPServiceClient(
            string url,
            AgsServerConnection connection)
            : base(url, connection)
        {
        }

        public void CancelJob(string JobID)
        {
            ServiceMethod method = (client) =>
            {
                client.CancelJob(JobID);
            };

            Invoke(method);
        }

        protected override GPServerPortClient CreateInnerClient(
            string url)
        {
            Debug.Assert(url != null);

            var serviceUrl = this.QueryServiceUrl(url);
            return ServiceHelper.CreateServiceClient<GPServerPortClient>(
                "GPServiceBinding",
                serviceUrl);
        }
    }

}
