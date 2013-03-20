using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Required data for discovery request.
    /// </summary>
    internal class DiscoveryRequestData
    {
        /// <summary>
        /// Map extent.
        /// </summary>
        public GPEnvelope MapExtent
        {
            get;
            set;
        }

        /// <summary>
        /// Point at which region will be detected.
        /// </summary>
        public Point? RegionPoint { get; set; }
    }

    /// <summary>
    /// Discovery requests builder.
    /// </summary>
    internal class DiscoveryRequestBuilder
    {
        #region Public properties

        /// <summary>
        /// Property contains available types for JSON.
        /// </summary>
        public static IEnumerable<Type> JsonTypes
        {
            get { return jsonTypes; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Get point for which region will be detected.
        /// </summary>
        /// <param name="routes">Collection of routes which region must be detected.</param>
        /// <returns>First point of location, or null if there is no locations.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when routes is null.</exception>
        public static Point? GetPointForRegionRequest(ICollection<Route> routes)
        {
            // Collection must be non null.
            if (routes == null)
                throw new ArgumentNullException("routes");

            // Get depot whih point will be used.
            var location = _GetDepot(routes);

            // If there is no such depot - return empty point, otherwise - return depot location.
            if (location != null)
                return _GetDepotPoint(location);

            return null;
        }

        /// <summary>
        /// Method collects information from solve request data for planned date and
        /// builds request object.
        /// </summary>
        /// <param name="data">Required data to get information from.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="data"/> is null reference.</exception>
        /// <returns>Request object with filled information for request.</returns>
        public SubmitDiscoveryRequest BuildRequest(DiscoveryRequestData data)
        {
            // Validate inputs.
            if (data == null)
                throw new ArgumentNullException("data");

            // Build request object.
            SubmitDiscoveryRequest req = new SubmitDiscoveryRequest();

            if (data.RegionPoint != null)
            {
                int wkid = data.MapExtent.SpatialReference.WKID;

                // Get depot point converted with correct spatial reference.
                Point depotPoint = (Point)data.RegionPoint;
                Point wmPoint = WebMercatorUtil.ProjectPointToWebMercator(depotPoint, wkid);
                GPPoint gpPoint = GPObjectHelper.PointToGPPoint(wmPoint);
                gpPoint.SpatialReference = new GPSpatialReference(wkid);

                // Fill required options.
                req.Geometry = gpPoint;
                req.GeometryType = NAGeometryType.esriGeometryPoint;
                req.SpatialReference = wkid;
                req.ResponseFormat = NAOutputFormat.JSON;
                req.Layers = NAIdentifyOperationLayers.AllLayers;
                req.Tolerance = DEFAULT_TOLERANCE;

                req.ImageDisplay = _GetImageDisplayParameter(data.MapExtent);

                req.MapExtent = string.Format(MAP_EXTENT_PARAMETER_FORMAT, data.MapExtent.XMin,
                    data.MapExtent.YMin, data.MapExtent.XMax, data.MapExtent.YMax);

                // Do not return geometry to optimize request.
                req.ReturnGeometry = false;
                req.ReturnZ = false;
                req.ReturnM = false;

                // Fill non-required options.
                req.LayerDefinitions = string.Empty;
                req.Time = string.Empty;
                req.LayerTimeOptions = string.Empty;
                req.MaxAllowableOffset = string.Empty;
                req.DynamicLayers = string.Empty;
            }

            return req;
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Method gets location point of Depot.
        /// </summary>
        /// <param name="loc">Depot to get information from.</param>
        /// <returns>Point location.</returns>
        /// <exception cref="RouteException">If location is not geocoded.</exception>
        private static Point _GetDepotPoint(Location loc)
        {
            Debug.Assert(loc != null);

            IGeocodable gc = loc as IGeocodable;
            Debug.Assert(gc != null);

            if (!gc.IsGeocoded)
            {
                throw new RouteException(String.Format(
                    Properties.Messages.Error_LocationIsUngeocoded, loc.Id));
            }

            Debug.Assert(gc.GeoLocation != null);

            return (Point)gc.GeoLocation;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets one first met depot in the collection of routes.
        /// </summary>
        /// <param name="routes">Collection of routes to get information from.</param>
        /// <returns>Depot or null, if depot is not found.</returns>
        private static Location _GetDepot(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            Location result = null;

            foreach (Route route in routes)
            {
                // Start location.
                if (route.StartLocation != null)
                {
                    result = route.StartLocation;

                    break; // Work done: depot is found.
                }
                // End location.
                else if (route.EndLocation != null)
                {
                    result = route.EndLocation;

                    break; // Work done: depot is found.
                }
            }

            return result;
        }

        /// <summary>
        /// Gets string of image display parameter.
        /// </summary>
        /// <param name="extent">Map extent.</param>
        /// <returns>String of image display parameter.</returns>
        private string _GetImageDisplayParameter(GPEnvelope extent)
        {
            Debug.Assert(extent != null);

            string result = string.Empty;

            try
            {
                // Create evelope of map extent.
                Envelope env = new Envelope(Convert.ToDouble(extent.XMin),
                    Convert.ToDouble(extent.YMax),
                    Convert.ToDouble(extent.XMax),
                    Convert.ToDouble(extent.YMin));

                // Get actual width and heigth.
                int width = Convert.ToInt32(env.Width);
                int height = Convert.ToInt32(env.Height);

                // Format result.
                result = string.Format(IMAGE_DISPLAY_PARAMETER_FORMAT, width, height);
            }
            catch (InvalidCastException ex)
            {
                throw new RouteException(
                    Properties.Messages.Error_GetDiscoveryServiceConfigFailed, ex);
            }

            return result;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Map extent parameter format.
        /// </summary>
        private const string MAP_EXTENT_PARAMETER_FORMAT = "{0},{1},{2},{3}";

        /// <summary>
        /// Image display parameter format.
        /// </summary>
        private const string IMAGE_DISPLAY_PARAMETER_FORMAT = "{0},{1},96";

        /// <summary>
        /// Default distance in screen pixels from the specified geometry
        /// within which the identify should be performed.
        /// </summary>
        private const int DEFAULT_TOLERANCE = 1;

        /// <summary>
        /// Data contract custom types.
        /// </summary>
        private static readonly Type[] jsonTypes = new Type[]
        {
            typeof(DiscoveryServiceResponse),
            typeof(DiscoveryDescription),
            typeof(DiscoveryServiceInfoResponse),
            typeof(GPEnvelope),
            typeof(GPSpatialReference)
        };

        #endregion
    }
}
