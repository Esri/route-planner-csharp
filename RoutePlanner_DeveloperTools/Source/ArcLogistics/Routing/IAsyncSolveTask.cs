using System;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides a way to run solve operations asynchronously.
    /// </summary>
    internal interface IAsyncSolveTask
    {
        /// <summary>
        /// Runs solve operation asynchronously.
        /// </summary>
        /// <param name="cancellationTracker">Cancellation tracker to be used for cancelling running
        /// solve operation.</param>
        /// <returns>A function returning asynchronous solve task result.</returns>
        Func<SolveTaskResult> Run(ICancelTracker cancellationTracker);
    }
}
