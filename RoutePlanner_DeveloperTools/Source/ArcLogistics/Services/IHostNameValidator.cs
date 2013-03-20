using System.Windows.Controls;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Validates host name address strings.
    /// </summary>
    internal interface IHostNameValidator
    {
        /// <summary>
        /// Validates the specified host name and returns appropriate validation result object.
        /// </summary>
        /// <param name="hostname">The host name to be validated.</param>
        /// <returns>A <see cref="ValidationResult"/> object describing the outcome of
        /// validation.</returns>
        ValidationResult Validate(string hostname);
    }
}
