/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Implements the <see cref="T:ESRI.ArcLogistics.Routing.IAsyncSolveTask"/> by executing the
    /// provided solve operation.
    /// </summary>
    /// <typeparam name="TSolveRequest">The type of the solve request for the operation.
    /// </typeparam>
    internal sealed class SolveOperationAsyncSolveTask<TSolveRequest> :
        IAsyncSolveTask
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the SolveOperationAsyncSolveTask class.
        /// </summary>
        /// <param name="operation">The reference to the solve operation to be executed by
        /// the task.</param>
        public SolveOperationAsyncSolveTask(
            ISolveOperation<TSolveRequest> operation)
        {
            Debug.Assert(operation != null);

            _operation = operation;
        }
        #endregion

        #region IAsyncSolveTask Members
        /// <summary>
        /// Runs solve operation passed to the class constructor.
        /// </summary>
        /// <param name="cancellationTracker">Cancellation tracker to be used for cancelling running
        /// solve operation.</param>
        /// <returns>A function returning asynchronous solve task result.</returns>
        public Func<SolveTaskResult> Run(ICancelTracker cancellationTracker)
        {
            if (_operation.CanGetResultWithoutSolve)
            {
                return () =>
                {
                    var result = _operation.CreateResultWithoutSolve();
                    // we must have a result if CanGetResultWithoutSolve is true
                    Debug.Assert(result != null);

                    return new SolveTaskResult
                    {
                        SolveResult = result,
                        NextTask = null,
                    };
                };
            }
            else
            {
                var request = _operation.CreateRequest();
                var resultProvider = _operation.Solve(request, cancellationTracker);

                return () => this._ProcessSolveResult(resultProvider);
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Performs solve result processing for the specified solve operation response.
        /// </summary>
        /// <param name="resultProvider">Function returning result of the solve operation.</param>
        /// <returns>A new <see cref="T:ESRI.ArcLogistics.Routing.SolveTaskResult"/> object storing
        /// solve result and the next task to run if any.</returns>
        private SolveTaskResult _ProcessSolveResult(
            Func<SolveOperationResult<TSolveRequest>> resultProvider)
        {
            var result = resultProvider();
            var nextStepOperation = result.NextStepOperation;

            IAsyncSolveTask nextTask = null;
            if (nextStepOperation != null)
            {
                nextTask = AsyncSolveTask.FromSolveOperation(nextStepOperation);
            }

            return new SolveTaskResult
            {
                SolveResult = result.SolveResult,
                NextTask = nextTask,
            };
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores reference to the solve operation to be executed by the task.
        /// </summary>
        private ISolveOperation<TSolveRequest> _operation;
        #endregion
    }
}
