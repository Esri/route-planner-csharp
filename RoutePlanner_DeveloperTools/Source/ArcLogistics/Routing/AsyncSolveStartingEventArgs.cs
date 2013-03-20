using System;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides data for asynchronous solve starting event.
    /// </summary>
    public class AsyncSolveStartingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the AsyncSolveStartingEventArgs class.
        /// </summary>
        /// <param name="schedule">The reference to the schedule object to
        /// solve routes for.</param>
        public AsyncSolveStartingEventArgs(Schedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException("schedule");
            }

            this.Schedule = schedule;
        }

        /// <summary>
        /// Gets reference to the schedule object the solve will be started for.
        /// </summary>
        public Schedule Schedule
        {
            get;
            private set;
        }
    }
}
