namespace ESRI.ArcLogistics
{
    /// <summary>
    /// ICancelTracker interface.
    /// </summary>
    internal interface ICancelTracker
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a boolean value indicating whether an operation was cancelled.
        /// </summary>
        bool IsCancelled
        {
            get;
        }

        #endregion properties
    }
}
