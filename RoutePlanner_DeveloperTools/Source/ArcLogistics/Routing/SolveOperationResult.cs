namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Stores result of the solve operation.
    /// </summary>
    /// <typeparam name="TSolveRequest">The type of the solve request for the operation.</typeparam>
    internal class SolveOperationResult<TSolveRequest>
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
        /// Gets or sets a reference to the next operation or null if there is no one.
        /// </summary>
        public ISolveOperation<TSolveRequest> NextStepOperation
        {
            get;
            set;
        }
    }
}
