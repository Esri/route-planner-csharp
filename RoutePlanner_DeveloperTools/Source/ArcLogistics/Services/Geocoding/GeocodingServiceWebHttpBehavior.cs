using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ESRI.ArcLogistics.Services.Geocoding
{
    /// <summary>
    /// Enables interaction with geocoding REST service.
    /// </summary>
    internal sealed class GeocodingServiceWebHttpBehavior : WebHttpBehavior
    {
        /// <summary>
        /// Gets query string converter instance.
        /// </summary>
        /// <param name="operationDescription">The description of the operation to get
        /// converter instance for.</param>
        /// <returns>A query string converter instance suitable for the specified
        /// operation.</returns>
        protected override QueryStringConverter GetQueryStringConverter(
            OperationDescription operationDescription)
        {
            return new GeocodingQueryStringConverter();
        }
    }
}
