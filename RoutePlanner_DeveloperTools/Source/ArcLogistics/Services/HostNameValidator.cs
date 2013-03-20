using System;
using System.Net;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// The default implementation of the <see cref="IHostNameValidator"/> interface.
    /// </summary>
    internal sealed class HostNameValidator : IHostNameValidator
    {
        /// <summary>
        /// Validates the specified host name and returns appropriate validation result object.
        /// </summary>
        /// <param name="hostname">The host name to be validated.</param>
        /// <returns>A <see cref="ValidationResult"/> object describing the outcome of
        /// validation.</returns>
        public ValidationResult Validate(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                return new ValidationResult(false, Properties.Messages.Error_HostNameIsEmpty);
            }

            try
            {
                new WebProxy(hostname);
            }
            catch (UriFormatException e)
            {
                return new ValidationResult(false, e.Message);
            }

            return ValidationResult.ValidResult;
        }
    }
}
