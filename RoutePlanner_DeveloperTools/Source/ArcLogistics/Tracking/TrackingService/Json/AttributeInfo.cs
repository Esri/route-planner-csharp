using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Stores network attribute information for route settings.
    /// </summary>
    [DataContract]
    internal sealed class AttributeInfo
    {
        /// <summary>
        /// Gets or sets a name of the network attribute.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of attribute parameter values.
        /// </summary>
        [DataMember]
        public IEnumerable<ParameterInfo> Parameters
        {
            get;
            set;
        }
    }
}
