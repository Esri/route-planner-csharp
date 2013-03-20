using System.Collections.Generic;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;
using ESRI.ArcLogistics.Tracking.TrackingService.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Provides access to feature layers and tables on the feature service.
    /// </summary>
    internal interface IFeatureService
    {
        /// <summary>
        /// Gets reference to the collection of feature service layers.
        /// </summary>
        IEnumerable<LayerReference> Layers
        {
            get;
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
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the feature service.</exception>
        IFeatureLayer<T> OpenLayer<T>(string layerName)
            where T : DataRecordBase, new();
    }
}
