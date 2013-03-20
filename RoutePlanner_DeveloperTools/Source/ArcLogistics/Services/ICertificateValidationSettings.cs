namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to remote certificates validation settings.
    /// </summary>
    internal interface ICertificateValidationSettings
    {
        /// <summary>
        /// Disables name mismatching checks for certificates received from the
        /// specified host.
        /// </summary>
        /// <param name="host">The name of the host to disable name mismatching
        /// checks for.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="host"/>
        /// is a null reference.</exception>
        void SkipNameValidation(string host);
    }
}
