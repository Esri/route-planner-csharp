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
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides common interface for VRP and Routing solve operations.
    /// </summary>
    /// <typeparam name="TSolveRequest">The type of the solve request for the operation.</typeparam>
    internal interface ISolveOperation<TSolveRequest>
    {
        Schedule Schedule
        {
            get;
        }

        SolveOperationType OperationType
        {
            get;
        }

        Object InputParams
        {
            get;
        }

        bool CanGetResultWithoutSolve
        {
            get;
        }

        SolveResult CreateResultWithoutSolve();

        TSolveRequest CreateRequest();

        Func<SolveOperationResult<TSolveRequest>> Solve(
            TSolveRequest request,
            ICancelTracker cancellationTracker);
    }
}
