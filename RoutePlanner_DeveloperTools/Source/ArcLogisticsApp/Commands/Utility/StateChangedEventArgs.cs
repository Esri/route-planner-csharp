using System;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Commands.Utility
{
    /// <summary>
    /// Provides data for the <see cref="IStateTrackingService.StateChanged"/> event.
    /// </summary>
    internal sealed class StateChangedEventArgs : EventArgs
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the StateChangedEventArgs class.
        /// </summary>
        /// <param name="isEnabled">The value of the
        /// <see cref="IStateTrackingService.IsEnabled"/> property of the
        /// object which fired an event.</param>
        public StateChangedEventArgs(bool isEnabled)
        {
            this.IsEnabled = isEnabled;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a value indicating whether the tracked component is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get;
            private set;
        }
        #endregion
    }
}
