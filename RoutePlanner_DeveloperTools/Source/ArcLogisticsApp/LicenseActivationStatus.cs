namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Defines license activation statuses.
    /// </summary>
    internal enum LicenseActivationStatus
    {
        /// <summary>
        /// The license was not yet activated.
        /// </summary>
        None,

        /// <summary>
        /// The license was successfully activated.
        /// </summary>
        Activated,

        /// <summary>
        /// The license activation failed due to license expiration.
        /// </summary>
        Expired,

        /// <summary>
        /// The license activation failed due to incorrect credentials.
        /// </summary>
        WrongCredentials,

        /// <summary>
        /// The license activation failed because there is no license available
        /// for credentials used for activation.
        /// </summary>
        NoSubscription,

        /// <summary>
        /// License activation failed due to licenser specific issues like
        /// network communication errors.
        /// </summary>
        Failed,
    }
}
