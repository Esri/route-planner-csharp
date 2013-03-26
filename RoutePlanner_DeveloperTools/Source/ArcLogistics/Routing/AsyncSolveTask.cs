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
