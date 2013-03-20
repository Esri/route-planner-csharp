using System;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Tracking.TrackingService;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements handler for workflow management exceptions.
    /// </summary>
    internal sealed class WorkflowManagementExceptionHandler : IExceptionHandler
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LicenserExceptionHandler class.
        /// </summary>
        /// <param name="application">The reference to the current application
        /// object.</param>
        public WorkflowManagementExceptionHandler(App application)
        {
            Debug.Assert(application != null);

            _application = application;
        }
        #endregion

        #region IExceptionHandler Members
        /// <summary>
        /// Handles the specified exception by adding corresponding message to
        /// the application messenger object.
        /// </summary>
        /// <param name="exceptionToHandle">The reference to the exception
        /// object to be handled.</param>
        /// <returns>True if and only if the exception was handled and need not
        /// be rethrown.</returns>
        public bool HandleException(Exception exceptionToHandle)
        {
            Debug.Assert(exceptionToHandle != null);

            var knownExceptions = new[]
            {
                typeof(AuthenticationException),
                typeof(CommunicationException),
                typeof(TrackingServiceException),
            };

            var isTrackingError = knownExceptions
                .Any(type => type.IsInstanceOfType(exceptionToHandle));
            if (isTrackingError)
            {
                Logger.Error(exceptionToHandle);
                CommonHelpers.AddTrackingErrorMessage(exceptionToHandle);

                return true;
            }

            return false;
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the current application object.
        /// </summary>
        private App _application;
        #endregion
    }
}
