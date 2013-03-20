using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Tracking;

namespace ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers
{
    /// <summary>
    /// Provides base facilities for routes management tasks.
    /// </summary>
    internal abstract class RoutesManagementTaskBase :
        StateTrackingServiceBase,
        IStateTrackingService
    {
        #region constructos
        /// <summary>
        /// Initializes a new instance of the RoutesManagementTaskBase class.
        /// </summary>
        /// <param name="workflowManagementStateTracker">Reference to the
        /// workflow management connection state tracking service.</param>
        /// <param name="solverStateTracker">Reference to the VRP solver state
        /// tracking service.</param>
        /// <param name="optimizeAndEditPage">Reference to the "Optimize And Edit"
        /// page object.</param>
        public RoutesManagementTaskBase(
            IStateTrackingService workflowManagementStateTracker,
            IStateTrackingService solverStateTracker,
            IOptimizeAndEditPage optimizeAndEditPage)
        {
            Debug.Assert(workflowManagementStateTracker != null);
            Debug.Assert(solverStateTracker != null);
            Debug.Assert(optimizeAndEditPage != null);

            _workflowManagementStateTracker = workflowManagementStateTracker;
            _workflowManagementStateTracker.StateChanged +=
                _WorkflowManagementStateTrackerStateChanged;
            _workflowManagementIsEnabled = _workflowManagementStateTracker.IsEnabled;

            _solverStateTracker = solverStateTracker;
            _solverStateTracker.StateChanged += _SolverStateTrackerStateChanged;
            _hasRoutingOperationInProgress = !_solverStateTracker.IsEnabled;

            _optimizeAndEditPage = optimizeAndEditPage;
            _optimizeAndEditPage.CurrentScheduleChanged += (_s, _e) =>
                _NotifyCurrentScheduleChanged(_optimizeAndEditPage.CurrentSchedule);
            _NotifyCurrentScheduleChanged(_optimizeAndEditPage.CurrentSchedule);

            this.UpdatedEnabledState();
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Updates value of the "IsEnabled" property analyzing current schedule,
        /// routes and all other necessary parameters.
        /// </summary>
        protected void UpdatedEnabledState()
        {
            this.IsEnabled = this.CheckIsEnabled();
        }

        /// <summary>
        /// Checks if the task should be in enabled state.
        /// </summary>
        /// <returns>True if and only if the task should be in enabled state.</returns>
        protected virtual bool CheckIsEnabled()
        {
            var isEnabled =
                _workflowManagementIsEnabled &&
                !_hasRoutingOperationInProgress &&
                _ScheduleTypeIsCurrent(_schedule) &&
                _HasRoutesWithMobileDevice(_schedule);

            return isEnabled;
        }
        /// <summary>
        /// Called when current schedule is changed.
        /// </summary>
        /// <param name="newSchedule">New schedule object reference.</param>
        protected abstract void OnCurrentScheduleChanged(Schedule newSchedule);
        #endregion

        #region private methods
        /// <summary>
        /// Handles workflow management connection state changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments instance.</param>
        private void _WorkflowManagementStateTrackerStateChanged(
            object sender,
            StateChangedEventArgs e)
        {
            if (_workflowManagementIsEnabled != e.IsEnabled)
            {
                _workflowManagementIsEnabled = e.IsEnabled;
                this.UpdatedEnabledState();
            }
        }

        /// <summary>
        /// Handles VRP solver state changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments instance.</param>
        private void _SolverStateTrackerStateChanged(
            object sender,
            StateChangedEventArgs e)
        {
            var newValue = !e.IsEnabled;
            if (_hasRoutingOperationInProgress != newValue)
            {
                _hasRoutingOperationInProgress = newValue;
                this.UpdatedEnabledState();
            }
        }

        /// <summary>
        /// Handles schedule changes.
        /// </summary>
        /// <param name="newSchedule">New created schedule.</param>
        private void _NotifyCurrentScheduleChanged(Schedule newSchedule)
        {
            if (_schedule != newSchedule)
            {
                if (_schedule != null)
                {
                    _schedule.PropertyChanged -= _SchedulePropertyChanged;
                }

                _schedule = newSchedule;
                _schedule.PropertyChanged += _SchedulePropertyChanged;
                this.OnCurrentScheduleChanged(_schedule);
                this.UpdatedEnabledState();
            }
        }

        /// <summary>
        /// Handles schedule property changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments instance.</param>
        private void _SchedulePropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            this.UpdatedEnabledState();
        }

        /// <summary>
        /// Checks if the type of the specified schedule is "Current".
        /// </summary>
        /// <param name="schedule">The reference to the schedule object to check.</param>
        /// <returns>True if and only if the schedule is not null and it's type
        /// is "Current".</returns>
        private bool _ScheduleTypeIsCurrent(Schedule schedule)
        {
            if (schedule == null)
            {
                return false;
            }

            if (schedule.Type != ScheduleType.Current)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check that schedule has at least 1 route with mobile device, which sync type is WMServer.
        /// </summary>
        /// <param name="schedule">Schedule to check.</param>
        /// <returns>'True' of schedule has such route, 'false' otherwise.</returns>
        private bool _HasRoutesWithMobileDevice(Schedule schedule)
        {
            return schedule.Routes.Any(route =>
            {
                var device = TrackingHelper.GetDeviceByRoute(route);
                return device != null && device.SyncType == SyncType.WMServer;
            });
        }

        #endregion

        #region private fields
        /// <summary>
        /// Tracks state of the workflow management service connection.
        /// </summary>
        private IStateTrackingService _workflowManagementStateTracker;

        /// <summary>
        /// Indicates if tracking service is enabled.
        /// </summary>
        private bool _workflowManagementIsEnabled;

        /// <summary>
        /// Tracks VRP solver switching to disabled state when routing operation
        /// is running.
        /// </summary>
        private IStateTrackingService _solverStateTracker;

        /// <summary>
        /// Indicates if there is a routing operation in progress.
        /// </summary>
        private bool _hasRoutingOperationInProgress;

        /// <summary>
        /// Reference to the optimize and edit page storing current schedule object.
        /// </summary>
        private IOptimizeAndEditPage _optimizeAndEditPage;

        /// <summary>
        /// Current schedule to send routes for.
        /// </summary>
        private Schedule _schedule;
        #endregion
    }
}
