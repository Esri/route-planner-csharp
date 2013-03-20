using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Represents errors occurred during communication with the tracking service.
    /// </summary>
    internal class TrackingServiceException : Exception
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the TrackingServiceException class.
        /// </summary>
        public TrackingServiceException()
        {
            this.Errors = Enumerable.Empty<TrackingServiceError>();
        }

        /// <summary>
        /// Initializes a new instance of the TrackingServiceException class.
        /// </summary>
        /// <param name="message">The message describing exception to be initialized.</param>
        /// <param name="errors">The reference to the collection of tracking errors which
        /// caused this exception to be thrown.</param>
        public TrackingServiceException(string message, IEnumerable<TrackingServiceError> errors)
            : base(message)
        {
            this.Errors = (errors ?? Enumerable.Empty<TrackingServiceError>())
                .Distinct()
                .Select(error => error ?? new TrackingServiceError(
                    -1,
                    Properties.Messages.Error_UnspecifiedTrackingServiceError))
                .ToList();
        }
        #endregion constructors

        #region public properties
        /// <summary>
        /// Gets reference to the collection of errors associated with the exception.
        /// </summary>
        public IEnumerable<TrackingServiceError> Errors
        {
            get;
            private set;
        }
        #endregion public properties

        #region Object Members
        /// <summary>
        /// Creates a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine(base.ToString());

            foreach (var error in this.Errors.ToIndexed())
            {
                var header = string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.TrackingServiceException_Error_Format,
                    error.Index,
                    error.Value.Code,
                    error.Value.Description);
                result.AppendLine(header);
                foreach (var detail in error.Value.Details)
                {
                    result.AppendLine(detail);
                }
            }

            return result.ToString();
        }
        #endregion
    }

}