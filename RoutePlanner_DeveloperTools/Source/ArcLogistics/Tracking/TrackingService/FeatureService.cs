using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;
using ESRI.ArcLogistics.Tracking.TrackingService.Json;
using ESRI.ArcLogistics.Tracking.TrackingService.Requests;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Provides access to feature layers located at the feature service.
    /// </summary>
    internal sealed class FeatureService : IFeatureService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the FeatureService class.
        /// </summary>
        /// <param name="serviceUrl">The URL of the feature service.</param>
        /// <param name="serviceInfo">The reference to the service info object.</param>
        /// <param name="requestSender">The reference to the request sender object.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="serviceUrl"/>,
        /// <paramref name="serviceInfo"/> or <paramref name="requestSender"/> argument is
        /// a null reference.</exception>
        public FeatureService(
            Uri serviceUrl,
            ServiceInfo serviceInfo,
            IFeatureServiceRequestSender requestSender)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }

            if (serviceInfo == null)
            {
                throw new ArgumentNullException("serviceInfo");
            }

            if (requestSender == null)
            {
                throw new ArgumentNullException("requestSender");
            }

            _requestSender = requestSender;
            _serviceInfo = serviceInfo;
            _serviceUrl = serviceUrl;
            _layerDescriptions = serviceInfo.AllLayers
                .ToLookup(
                    info => info.Name,
                    info => new LayerInfo
                    {
                        ID = info.ID,
                        Url = new Uri(string.Format(
                            TABLE_QUERY_FORMAT,
                            _serviceUrl.AbsoluteUri,
                            info.ID)),
                    })
                .ToDictionary(item => item.Key, item => item.First());

            this.Layers = serviceInfo.AllLayers.ToList();
        }
        #endregion

        #region public static methods
        /// <summary>
        /// Creates a new instance of the <see cref="FeatureService"/> class with the specified
        /// url.
        /// </summary>
        /// <param name="serviceUrl">The URL of the feature service to be created.</param>
        /// <param name="server">The reference to the feature services server object.</param>
        /// <returns>A reference to the feature service object for the specified URL.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="serviceUrl"/>
        /// argument is a null reference.</exception>
        public static IFeatureService Create(Uri serviceUrl, AgsServer server)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }

            var requestSender = new FeatureServiceRequestSender(server);

            var serviceQueryUri = string.Format(SERVICE_QUERY_FORMAT, serviceUrl.AbsoluteUri);
            var options = new HttpRequestOptions
            {
                Method = HttpMethod.Get,
                UseGZipEncoding = true,
            };

            var request = new ESRI.ArcLogistics.Tracking.TrackingService.Requests.RequestBase();
            var query = RestHelper.BuildQueryString(request, Enumerable.Empty<Type>(), true);

            var info = requestSender.SendRequest<ServiceInfo>(serviceQueryUri, query, options);

            return new FeatureService(new Uri(serviceQueryUri), info, requestSender);
        }
        #endregion

        #region IFeatureService Members
        /// <summary>
        /// Gets reference to the collection of feature service layers.
        /// </summary>
        public IEnumerable<LayerReference> Layers
        {
            get;
            private set;
        }

        /// <summary>
        /// Opens feature layer or table with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of objects stored in the feature layer.</typeparam>
        /// <param name="layerName">The name of the feature layer to be opened.</param>
        /// <returns>A reference to the <see cref="IFeatureLayer&lt;T&gt;"/> object representing
        /// feature layer with the specified name.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="layerName"/> is a null
        /// reference.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="layerName"/> is not
        /// a valid feature layer/table name.</exception>
        public IFeatureLayer<T> OpenLayer<T>(string layerName)
            where T : DataRecordBase, new()
        {
            if (layerName == null)
            {
                throw new ArgumentNullException("layerName");
            }

            LayerInfo layerInfo = null;
            if (!_layerDescriptions.TryGetValue(layerName, out layerInfo))
            {
                throw new ArgumentException(
                    Properties.Messages.Error_FeatureServiceUnknownLayerName,
                    "layerName");
            }

            lock (layerInfo)
            {
                if (layerInfo.Description == null)
                {
                    layerInfo.Description = _LoadLayerDescription(layerInfo.Url);
                }
            }

            return new FeatureLayer<T>(layerInfo.Url, layerInfo.Description, _requestSender);
        }
        #endregion

        #region private classes
        /// <summary>
        /// Stores information about single feature layer.
        /// </summary>
        private sealed class LayerInfo
        {
            /// <summary>
            /// Gets or sets identifier of the feature layer.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Gets or sets URI of the feature layer.
            /// </summary>
            public Uri Url { get; set; }

            /// <summary>
            /// Gets or sets description of the feature layer.
            /// </summary>
            public LayerDescription Description { get; set; }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Loads feature layer description from the specified URL.
        /// </summary>
        /// <param name="featureLayerUrl">The URL of the feature layer to load
        /// description for.</param>
        /// <returns>A reference to the feature layer description obtained at the
        /// specified URL.</returns>
        private LayerDescription _LoadLayerDescription(Uri featureLayerUrl)
        {
            Debug.Assert(featureLayerUrl != null);

            var request = new ESRI.ArcLogistics.Tracking.TrackingService.Requests.RequestBase();
            var query = RestHelper.BuildQueryString(request, Enumerable.Empty<Type>(), true);
            var options = new HttpRequestOptions
            {
                Method = HttpMethod.Get,
                UseGZipEncoding = true,
            };

            var layerDescription = _requestSender.SendRequest<LayerDescription>(
                featureLayerUrl.AbsoluteUri,
                query,
                options);

            return layerDescription;
        }
    	#endregion

        #region private constants
        /// <summary>
        /// Format of the service query URL.
        /// </summary>
        private const string SERVICE_QUERY_FORMAT = "{0}/FeatureServer";

        /// <summary>
        /// Format of the table query URL.
        /// </summary>
        private const string TABLE_QUERY_FORMAT = "{0}/{1}";
        #endregion

        #region private fields
        /// <summary>
        /// The URL of the feature service.
        /// </summary>
        private Uri _serviceUrl;

        /// <summary>
        /// The reference to the feature service information object.
        /// </summary>
        private ServiceInfo _serviceInfo;

        /// <summary>
        /// The reference to request sender object.
        /// </summary>
        private IFeatureServiceRequestSender _requestSender;

        /// <summary>
        /// The dictionary for mapping feature layer names into corresponding information objects.
        /// </summary>
        private IDictionary<string, LayerInfo> _layerDescriptions;
        #endregion
    }
}
