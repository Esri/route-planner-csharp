using System;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Provides access to "Optimize And Edit" page facilities.
    /// </summary>
    internal interface IOptimizeAndEditPage
    {
        #region events
        /// <summary>
        /// Fired when current schedule was changed.
        /// </summary>
        event EventHandler CurrentScheduleChanged;
        #endregion

        #region properties
        /// <summary>
        /// Gets or sets current schedule.
        /// </summary>
        Schedule CurrentSchedule
        {
            get;
            set;
        }
        #endregion
    }
}
