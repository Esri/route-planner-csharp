using System;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Commands.Utility
{
    /// <summary>
    /// Tracks changes of the component enablement state.
    /// </summary>
    internal interface IStateTrackingService
    {
        /// <summary>
        /// Fired when state was changed.
        /// </summary>
        event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets a value indicating whether the tracked component is enabled.
        /// </summary>
        bool IsEnabled
        {
            get;
        }
    }
}
