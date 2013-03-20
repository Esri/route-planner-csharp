using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Implements the <see cref="T:ESRI.ArcLogistics.Routing.IAsyncSolveTask"/> by running the
    /// provided delegate.
    /// </summary>
    internal sealed class DelegateAsyncSolveTask : IAsyncSolveTask
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DelegateAsyncSolveTask class.
        /// </summary>
        /// <param name="taskFunction">The function to be executed by the task.</param>
        public DelegateAsyncSolveTask(Func<ICancelTracker, Func<SolveTaskResult>> taskFunction)
        {
            Debug.Assert(taskFunction != null);

            _taskFunction = taskFunction;
        }
        #endregion

        #region IAsyncSolveTask Members
        /// <summary>
        /// Runs function passed to the class constructor.
        /// </summary>
        /// <param name="cancellationTracker">Cancellation tracker to be used for cancelling running
        /// solve operation.</param>
        /// <returns>A function returning asynchronous solve task result.</returns>
        public Func<SolveTaskResult> Run(ICancelTracker cancellationTracker)
        {
            return _taskFunction(cancellationTracker);
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores function to be executed by the task.
        /// </summary>
        private Func<ICancelTracker, Func<SolveTaskResult>> _taskFunction;
        #endregion
    }
}
