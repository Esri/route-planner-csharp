using System;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// IFaultInfo interface.
    /// </summary>
    internal interface IFaultInfo
    {
        /// <summary>
        /// Gets a boolean value indicating whether corresponding
        /// request failed.
        /// </summary>
        bool IsFault { get; }

        /// <summary>
        /// Gets GPError object that contains error information.
        /// </summary>
        GPError FaultInfo { get; set; }
    }
}
