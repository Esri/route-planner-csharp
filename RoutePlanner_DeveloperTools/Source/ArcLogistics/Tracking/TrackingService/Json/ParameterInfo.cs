using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Stores information about attribute parameter.
    /// </summary>
    [DataContract]
    internal sealed class ParameterInfo
    {
        #region constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        /// <param name="isRestrictionUsage">'True' if it is restriction usage parameter,
        /// 'false' otherwise.</param>
        public ParameterInfo(string name, string value, bool isRestrictionUsage)
        {
            Name = name;
            Value = value;
            UsageType = isRestrictionUsage ? RESTICTION : GENERAL;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Parameter name.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Parameter value.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Parameter usage type.
        /// </summary>
        [DataMember(Name = "usageType")]
        public string UsageType { get; set; }

        #endregion

        #region private members

        /// <summary>
        /// UsageType property possible values.
        /// </summary>
        private const string RESTICTION = @"Restriction";
        private const string GENERAL = @"General";

        #endregion
    }
}
