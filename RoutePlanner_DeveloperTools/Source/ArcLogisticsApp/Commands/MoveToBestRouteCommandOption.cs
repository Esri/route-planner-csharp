using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class implements logic for "MoveToBestRoute" option.
    /// </summary>
    internal class MoveToBestRouteCommandOption : MoveToCommandOptionBase
    {
        #region Constructors

        /// <summary>
        /// Crates new MoveToBestRouteCommandOption. Initializes all main class fields.
        /// </summary>
        /// <param name="groupId">Option group ID (to set in separate group in UI).</param>
        /// <param name="currentSchedule">Processed schedule.</param>
        public MoveToBestRouteCommandOption(int groupId)
        {
            GroupID = groupId;
            EnabledTooltip = (string)App.Current.FindResource(ENABLED_TOOLTIP_RESOURCE);
            DisabledTooltip = (string)App.Current.FindResource(DISABLED_TOOLTIP_RESOURCE);
        }

        #endregion

        #region CommandBase Members

        /// <summary>
        /// Command option title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource(BEST_ROUTE_OPTION_RESOURCE); }
        }

        /// <summary>
        /// Gets/sets tooltip string.
        /// </summary>
        public override string TooltipText
        {
            get;
            protected set;
        }

        #endregion

        #region RoutingCommandOptionBase Members

        /// <summary>
        /// Define whether option is enabled.
        /// </summary>
        protected override void _CheckEnabled(OptimizeAndEditPage schedulePage)
        {
            Debug.Assert(schedulePage != null);

            if (schedulePage.CurrentSchedule == null)
            {
                this.IsEnabled = false;

                return;
            }

            bool result = false;

            // If all routes locked - option should be disabled.
            foreach (Route route in schedulePage.CurrentSchedule.Routes)
            {
                if (!route.IsLocked)
                {
                    result = true;
                    break;
                }
            }

            IsEnabled = result;
        }

        /// <summary>
        /// Starts operation process.
        /// </summary>
        /// <param name="args">Command args. Empty there.</param>
        protected override void _Execute(params object[] args)
        {
            // Create command parameters.
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            Schedule schedule = schedulePage.CurrentSchedule;
            ICollection<Route> targetRoutes = ViolationsHelper.GetBuildRoutes(schedule);

            // And pass they to call _Execute from base class.
            base._Execute(targetRoutes, (string)App.Current.FindResource(BEST_ROUTE_OPTION_RESOURCE));
        }

        #endregion

        #region Private Constants 

        /// <summary>
        /// BestRoute string resource.
        /// </summary>
        private const string BEST_ROUTE_OPTION_RESOURCE = "BestRouteOption";

        /// <summary>
        /// Enabled tooltip resource.
        /// </summary>
        private const string ENABLED_TOOLTIP_RESOURCE = "AssignToBestRouteCommandEnabledTooltip";

        /// <summary>
        /// Disabled tooltip resource.
        /// </summary>
        private const string DISABLED_TOOLTIP_RESOURCE = "AssignToBestRouteCommandDisabledTooltip";

        #endregion
    }
}
