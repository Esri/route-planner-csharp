using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Services
{
    /// <summary>
    /// Exception handler for communication exceptions occurred during interactions with the
    /// tracking service.
    /// </summary>
    internal sealed class TrackingServiceExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Handles communication related exception assuming it was generated during interaction
        /// with tracking service.
        /// </summary>
        /// <param name="exceptionToHandle">The exception to be handled.</param>
        /// <returns>True if and only if the exception was handled and need not
        /// be rethrown.</returns>
        public bool HandleException(Exception exceptionToHandle)
        {
            Debug.Assert(exceptionToHandle != null);

            Logger.Error(exceptionToHandle);

            var isTrackingError =
                exceptionToHandle is AuthenticationException ||
                exceptionToHandle is CommunicationException;

            if (!isTrackingError)
            {
                return false;
            }

            CommonHelpers.AddTrackingErrorMessage(exceptionToHandle);

            return true;
        }
    }
}
