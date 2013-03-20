using System;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Tracks VRP solver switching to disabled state when asynchronous solve is
    /// running.
    /// </summary>
    internal sealed class SolveStateTrackingService :
        StateTrackingServiceBase,
        IStateTrackingService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the SolveStateTrackingService.
        /// </summary>
        /// <param name="solver">VRP solver to track solves for.</param>
        /// <param name="currentDateProvider">Date/time provider object.</param>
        public SolveStateTrackingService(
            IVrpSolver solver,
            ICurrentDateProvider currentDateProvider)
        {
            Debug.Assert(solver != null);

            _solver = solver;
            _solver.AsyncSolveStarted += (s, e) => _UpdateState();
            _solver.AsyncSolveCompleted += (s, e) => _UpdateState(e.OperationId);

            _currentDateProvider = currentDateProvider;
            _currentDateProvider.CurrentDateChanged += (s, e) => _UpdateState();

            _UpdateState();
        }
        #endregion

        #region private methods
        /// <summary>
        /// Updates current enablement state.
        /// </summary>
        private void _UpdateState()
        {
            _UpdateState(null);
        }

        /// <summary>
        /// Updates current enablement state excluding operation with the specified id.
        /// </summary>
        /// <param name="id">Identifier of the operation to be excluded when
        /// updating state.</param>
        private void _UpdateState(Guid? id)
        {
            var currentDate = _currentDateProvider.CurrentDate;
            var operations = _solver.GetAsyncOperations(currentDate)
                .Where(operation => operation.Id != id)
                .Take(1);

            this.IsEnabled = operations.Count() == 0;
        }
        #endregion

        #region private fields
        /// <summary>
        /// VRP solver object to be used for tracking solves.
        /// </summary>
        private IVrpSolver _solver;

        /// <summary>
        /// Date/time provider object.
        /// </summary>
        private ICurrentDateProvider _currentDateProvider;
        #endregion
    }
}
