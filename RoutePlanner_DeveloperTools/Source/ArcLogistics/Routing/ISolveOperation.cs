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
