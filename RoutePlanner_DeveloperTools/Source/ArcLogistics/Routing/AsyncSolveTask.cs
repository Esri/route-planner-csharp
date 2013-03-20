using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Simplifies creation of the <see cref="T:ESRI.ArcLogistics.Routing.IAsyncSolveTask"/>
    /// instances.
    /// </summary>
    internal static class AsyncSolveTask
    {
        /// <summary>
        /// Creates a new task for executing the specified function.
        /// </summary>
        /// <param name="taskFunction">The function to be executed by the task.</param>
        /// <returns>A new instance of the <see cref="T:ESRI.ArcLogistics.Routing.IAsyncSolveTask"/>
        /// for executing the specified function.</returns>
        public static IAsyncSolveTask FromDelegate(
            Func<ICancelTracker, Func<SolveTaskResult>> taskFunction)
        {
            Debug.Assert(taskFunction != null);

            return new DelegateAsyncSolveTask(taskFunction);
        }

        /// <summary>
        /// Creates a new task for executing the specified solve operation.
        /// </summary>
        /// <typeparam name="TSolveRequest">The type of the solve request for the operation.
        /// </typeparam>
        /// <param name="operation">The solve operation to be executed by the task.</param>
        /// <returns>A new instance of the <see cref="T:ESRI.ArcLogistics.Routing.IAsyncSolveTask"/>
        /// for executing the specified solve operation.</returns>
        public static IAsyncSolveTask FromSolveOperation<TSolveRequest>(
            ISolveOperation<TSolveRequest> operation)
        {
            Debug.Assert(operation != null);

            return new SolveOperationAsyncSolveTask<TSolveRequest>(operation);
        }
    }
}
