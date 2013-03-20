using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Stores settings affecting routing operations.
    /// </summary>
    [DataContract]
    internal sealed class RouteSettings
    {
        /// <summary>
        /// Gets or sets a value of the U-Turn policy to be used for routing.
        /// </summary>
        [DataMember]
        public UTurnPolicy UTurnPolicy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of the impedance attribute.
        /// </summary>
        [DataMember]
        public string ImpedanceAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets units of measure to be used for length in driving directions.
        /// </summary>
        [DataMember]
        public NANetworkAttributeUnits DirectionsLengthUnits
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of restriction attributes to be used for routing.
        /// </summary>
        [DataMember]
        public IEnumerable<RestrictionAttributeInfo> Restrictions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a collection of attribute settings.
        /// </summary>
        [DataMember]
        public IEnumerable<AttributeInfo> Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a break tolerance.
        /// </summary>
        [DataMember]
        public int BreakTolerance
        {
            get;
            set;
        }
    }
}
