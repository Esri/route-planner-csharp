using System.Collections.Generic;
using System.Linq;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Stores information about single tracking service error.
    /// </summary>
    internal sealed class TrackingServiceError
    {
        /// <summary>
        /// Initializes a new instance of the TrackingServiceError class.
        /// </summary>
        /// <param name="code">The error code number.</param>
        /// <param name="description">The error description.</param>
        /// <param name="details">The error details.</param>
        public TrackingServiceError(
            int code,
            string description = null,
            IEnumerable<string> details = null)
        {
            this.Code = code;
            this.Description = description ?? string.Empty;
            this.Details = details ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets error code value for the tracking error.
        /// </summary>
        public int Code
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets description for the tracking error.
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets error details for the tracking error.
        /// </summary>
        public IEnumerable<string> Details
        {
            get;
            private set;
        }
    }
}
