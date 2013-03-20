using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class helpers container for optimize and edit page.
    /// </summary>
    internal static class OptimizeAndEditHelpers
    {
        /// <summary>
        /// Method finds appropriate schedule to select when some date is opening.
        /// </summary>
        /// <param name="schedules">Schedules collection.</param>
        /// <returns>Schedule, which needs to be opened.</returns>
        public static Schedule FindScheduleToSelect(IEnumerable<Schedule> schedules)
        {
            Debug.Assert(schedules != null);

            // Find last edited version or current schedule.
            Schedule candidateToBeCurrent = null;

            foreach (Schedule schedule in schedules)
            {
                if (candidateToBeCurrent == null)
                    candidateToBeCurrent = schedule;
                else if (schedule.Type == ScheduleType.Edited)
                {
                    // If candidate is Edited schedule or its creation time is bigger.
                    if (candidateToBeCurrent.Type == ScheduleType.Edited ||
                        schedule.CreationTime > candidateToBeCurrent.CreationTime)
                        candidateToBeCurrent = schedule;
                }
                else if (schedule.Type == ScheduleType.Current)
                {
                    Debug.Assert(candidateToBeCurrent.Type != ScheduleType.Current); // Only one schedule can be current.

                    // Override candidate if it is Edited Schedule.
                    if (candidateToBeCurrent.Type != ScheduleType.Edited)
                        candidateToBeCurrent = schedule;
                }
            }

            return candidateToBeCurrent;
        }

        /// <summary>
        /// Loads schedule of type <see cref="ScheduleType.Current"/> with the specified planned
        /// date from the specified project.
        /// </summary>
        /// <param name="project">The project to load schedule from</param>
        /// <param name="plannedDate">The date/time to load schedule for.</param>
        /// <returns>A "Current" schedule for the specified planned date.</returns>
        public static Schedule LoadSchedule(
            Project project,
            DateTime plannedDate)
        {
            return LoadSchedule(
                project,
                plannedDate,
                schedules => schedules.FirstOrDefault(
                    schedule => schedule.Type == ScheduleType.Current));
        }

        /// <summary>
        /// Loads schedule with the specified planned date from the specified project.
        /// </summary>
        /// <param name="project">The project to load schedule from</param>
        /// <param name="plannedDate">The date/time to load schedule for.</param>
        /// <param name="scheduleSelector">The function for selecting particular schedule object
        /// from all schedules with the same planned date.</param>
        /// <returns>A "Current" schedule for the specified planned date.</returns>
        public static Schedule LoadSchedule(
            Project project,
            DateTime plannedDate,
            Func<IEnumerable<Schedule>, Schedule> scheduleSelector)
        {
            Debug.Assert(project != null);
            Debug.Assert(scheduleSelector != null);

            // Load or create schedule for plannedDate.
            var currentSchedule = project.Schedules.Search(plannedDate, false)
                .FirstOrDefault(schedule => schedule.Type == ScheduleType.Current);
            if (currentSchedule == null)
            {
                currentSchedule = project.AddNewSchedule(
                    plannedDate,
                    App.Current.FindString("CurrentScheduleName"));
            }

            OptimizeAndEditHelpers.FixSchedule(project, currentSchedule);

            return currentSchedule;
        }

        /// <summary>
        /// Fills necessary schedule properties which could be missing after schedule loading.
        /// </summary>
        /// <param name="project">The reference to the project to load data from.</param>
        /// <param name="schedule">The schedule to be fixed.</param>
        public static void FixSchedule(Project project, Schedule schedule)
        {
            Debug.Assert(project != null);
            Debug.Assert(schedule != null);

            // Fill missing schedule properties when necessary.
            if (schedule.Routes.Count == 0)
            {
                project.LoadDefaultRoutesForSchedule(schedule.Id, schedule.PlannedDate.Value);
            }

            if (schedule.UnassignedOrders == null)
            {
                schedule.UnassignedOrders = project.Orders.SearchUnassignedOrders(
                    schedule,
                    true);
            }
        }

        #region private constants
        /// <summary>
        /// Resource key for the format string for empty selection status.
        /// </summary>
        private const string NO_SELECTION_GRID_STATUS_FORMAT_KEY = "NoSelectedGridStatusFormat";
        #endregion
    }
}