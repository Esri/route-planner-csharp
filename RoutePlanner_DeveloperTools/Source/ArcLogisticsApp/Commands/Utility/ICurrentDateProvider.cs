using System;

namespace ESRI.ArcLogistics.App.Commands.Utility
{
    /// <summary>
    /// Provides access to currently used date/time value.
    /// </summary>
    interface ICurrentDateProvider
    {
        /// <summary>
        /// Fired when current date is changed.
        /// </summary>
        event EventHandler CurrentDateChanged;

        /// <summary>
        /// Gets current date/time value.
        /// </summary>
        DateTime CurrentDate
        {
            get;
        }
    }
}
