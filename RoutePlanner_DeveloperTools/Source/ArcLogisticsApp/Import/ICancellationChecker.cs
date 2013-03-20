namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Interface class for operation canceler.
    /// </summary>
    internal interface ICancellationChecker
    {
        /// <summary>
        /// This property indicates whether cancellation has been requested for this token source.
        /// </summary>
        bool IsCancellationRequested { get; }

        /// <summary>
        /// Checks cancel state if operation was canceled throw excemtion.
        /// </summary>
        /// <exception cref="ESRI.ArcLogistics.UserBreakException">
        /// Operation was cancelled.
        /// </exception>
        void ThrowIfCancellationRequested();
    }
}
