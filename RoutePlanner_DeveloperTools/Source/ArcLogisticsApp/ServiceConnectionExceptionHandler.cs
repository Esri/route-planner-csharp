using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Threading;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements handler for Services connection exceptions.
    /// </summary>
    internal sealed class ServiceConnectionExceptionHandler : IServiceExceptionHandler
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the UriFormatExceptionHandler class.
        /// </summary>
        public ServiceConnectionExceptionHandler(App application)
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
        /// <param name="serviceName">Name of service which caused exception.</param>
        /// <returns>True if the exception was handled and need not to
        /// be rethrown.</returns>
        public bool HandleException(Exception exceptionToHandle, string serviceName)
        {
            Debug.Assert(exceptionToHandle != null);
            Debug.Assert(serviceName != null);

            if (_IsItUriError(exceptionToHandle))
            {
                // Don't show error message if URL error already occured.
                if (!_isUriExceptionOccured)
                {
                    Logger.Error(exceptionToHandle);

                    string message =
                        (string)App.Current.FindResource(SOME_SERVICE_URI_ERROR_MESSAGE);

                    _application.Messenger.AddError(message);

                    _isUriExceptionOccured = true;
                }

                return true;
            }
            else if (exceptionToHandle is CommunicationException)
            {
                // Handle Communication exception.
                string serviceMsg =
                    CommonHelpers.FormatServiceCommunicationError(serviceName,
                    exceptionToHandle as CommunicationException);

                App.Current.Messenger.AddMessage(MessageType.Error, serviceMsg);

                return true;
            }
            else if (exceptionToHandle is AuthenticationException)
            {
                // Handle Authentication exception.
                var exAuthentication = exceptionToHandle as AuthenticationException;

                string errorMessage =
                    App.Current.GetString(SERVICE_AUTH_ERROR_MESSAGE,
                    serviceName, exAuthentication.ServiceName);

                var link = new Link(App.Current.FindString(LICENSE_PANEL_NAME),
                    Pages.PagePaths.LicensePagePath, LinkType.Page);

                App.Current.Messenger.AddMessage(
                    MessageType.Error, errorMessage, link);

                return true;
            }
            else
            {
                // Do nothing.
            }

            return false;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Method checks if  exception to handle is URI error.
        /// </summary>
        /// <param name="exceptionToHandle">Exception to handle.</param>
        /// <returns>True - if exception to handle is URI error, otherwise - false.</returns>
        private bool _IsItUriError(Exception exceptionToHandle)
        {
            Debug.Assert(exceptionToHandle != null);

            // Create list of known URI exceptions.
            var knownExceptions = new[]
            {
                typeof(ArgumentException),
                typeof(FaultException),
            };

            // Check if exception to handle is one of known URI exceptions.
            return knownExceptions.Any(type => type.IsInstanceOfType(exceptionToHandle));
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Some service URI error message resource name.
        /// </summary>
        private const string SOME_SERVICE_URI_ERROR_MESSAGE = "SomeServiceUriError";

        /// <summary>
        /// Service authentication error message resource name.
        /// </summary>
        private const string SERVICE_AUTH_ERROR_MESSAGE = "ServiceAuthError";

        /// <summary>
        /// Licence panel resource name.
        /// </summary>
        private const string LICENSE_PANEL_NAME = "LicencePanelText";

        #endregion

        #region Private fields

        /// <summary>
        /// The reference to the current application object.
        /// </summary>
        private App _application;

        /// <summary>
        /// Determines if URL exception already occured which
        /// means do not show error message again.
        /// </summary>
        private bool _isUriExceptionOccured = false;

        #endregion
    }
}
