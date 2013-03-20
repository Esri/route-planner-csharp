using System.Diagnostics;
using System.ServiceModel;
using ESRI.ArcLogistics.MapService;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// MapServiceClient class.
    /// </summary>
    internal class MapServiceClient
        : ServiceClientWrap<MapServerPortClient, MapServerPort>
    {
        public MapServiceClient(string url, AgsServerConnection connection)
            : base(url, connection)
        {
        }

        public MapServerInfo GetServerInfo(string MapName)
        {
            ServiceMethod<MapServerInfo> method = (client) =>
            {
                return client.GetServerInfo(MapName);
            };

            return Invoke<MapServerInfo>(method);
        }

        public string GetDefaultMapName()
        {
            ServiceMethod<string> method = (client) =>
            {
                return client.GetDefaultMapName();
            };

            return Invoke<string>(method);
        }

        public MapImage ExportMapImage(MapDescription MapDescription, ImageDescription ImageDescription)
        {
            ServiceMethod<MapImage> method = (client) =>
            {
                return client.ExportMapImage(MapDescription, ImageDescription);
            };

            return Invoke<MapImage>(method);
        }

        protected override MapServerPortClient CreateInnerClient(
            string url)
        {
            Debug.Assert(url != null);

            return ServiceHelper.CreateServiceClient<MapServerPortClient>(
                "MapServiceBinding",
                url);
        }
    }

}
