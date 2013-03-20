using System;

namespace ESRI.ArcLogistics.App.Commands.Utility
{
    /// <summary>
    /// Provides common facilities for <see cref="IStateTrackingService"/> implementors.
    /// </summary>
    internal abstract class StateTrackingServiceBase : IStateTrackingService
    {
        #region IStateTrackingService Members
        /// <summary>
        /// Fired when state was changed.
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets a value indicating whether the tracked component is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            protected set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    _NotifyStateChanged(_isEnabled);
                }
            }
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Called when state is changed.
        /// </summary>
        /// <param name="e">Event arguments instance.</param>
        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            var temp = StateChanged;
            if (temp != null)
            {
                temp(this, e);
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Notifies about state change.
        /// </summary>
        /// <param name="isEnabled">The new value indicating whether the
        /// tracking service is enabled.</param>
        private void _NotifyStateChanged(bool isEnabled)
        {
            var args = new StateChangedEventArgs(isEnabled);
            this.OnStateChanged(args);
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores a value indicating whether the tracked component is enabled.
        /// </summary>
        private bool _isEnabled;
        #endregion
    }
}
