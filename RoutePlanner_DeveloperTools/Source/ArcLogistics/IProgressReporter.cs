namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Reports changes in the current operation progress status.
    /// </summary>
    internal interface IProgressReporter
    {
        /// <summary>
        /// Reports completion of the current step and beginning of the next one
        /// if any.
        /// </summary>
        void Step();
    }
}
