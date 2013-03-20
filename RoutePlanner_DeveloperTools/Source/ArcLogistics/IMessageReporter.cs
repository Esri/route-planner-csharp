namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Provides facilities for reporting various messages to the application.
    /// </summary>
    internal interface IMessageReporter
    {
        /// <summary>
        /// Reports a warning.
        /// </summary>
        /// <param name="message">The message describing cause of the warning.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is a null reference.
        /// </exception>
        void ReportWarning(string message);

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="message">The message describing cause of the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is a null reference.
        /// </exception>
        void ReportError(string message);
    }
}
