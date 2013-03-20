namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Stores result of the solve task execution.
    /// </summary>
    internal sealed class SolveTaskResult
    {
        /// <summary>
        /// Gets or sets a reference to the solve result.
        /// </summary>
        public SolveResult SolveResult
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to the next solve task.
        /// </summary>
        public IAsyncSolveTask NextTask
        {
            get;
            set;
        }
    }
}
