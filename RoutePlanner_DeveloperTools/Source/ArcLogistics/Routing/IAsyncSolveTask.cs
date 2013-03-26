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
