using System;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Commands.Utility;

namespace ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers
{
    /// <summary>
    /// Simplifies IsEnabled property managing for tracking service related commands.
    /// </summary>
    internal sealed class TrackingCommandStateService :
        StateTrackingServiceBase,
        IStateTrackingService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the TrackingCommandStateService class
        /// with the reference to current application object.
        /// </summary>
        /// <param name="app">The application object to check state changes for.</param>
        public TrackingCommandStateService(App app)
        {
            Debug.Assert(app != null);

            _hasProject = app.Project != null;
            _hasTracker = app.Tracker != null && app.Tracker.InitError == null;

            _UpdateEnabledState();

            app.ProjectLoaded += _AppProjectLoaded;
            app.ProjectClosed += _AppProjectClosed;
            app.TrackerInitialized += _AppTrackerInitialized;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Handles project loading event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Project loading event arguments.</param>
        private void _AppProjectLoaded(object sender, EventArgs e)
        {
            _hasProject = true;
            _UpdateEnabledState();
        }

        /// <summary>
        /// Handles project closing event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Project closing event arguments.</param>
        private void _AppProjectClosed(object sender, EventArgs e)
        {
            _hasProject = false;
            _UpdateEnabledState();
        }

        /// <summary>
        /// Handles tracker initialization event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Tracker initialization event arguments.</param>
        private void _AppTrackerInitialized(object sender, EventArgs e)
        {
            _hasTracker = true;
            _UpdateEnabledState();
        }

        /// <summary>
        /// Updates current value of <see cref="IStateTrackingService.IsEnabled"/> property.
        /// </summary>
        private void _UpdateEnabledState()
        {
            this.IsEnabled = _hasProject && _hasTracker;
        }
        #endregion

        #region private fields
        /// <summary>
        /// Indicates if there is current project in application.
        /// </summary>
        private bool _hasProject;

        /// <summary>
        /// Indicates if there is a tracking in application.
        /// </summary>
        private bool _hasTracker;
        #endregion
    }
}
