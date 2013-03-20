using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.DomainObjects;
using System.Diagnostics;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command for move orders from one date to other - inherits from MultiUnassignCmdBase where included common logic to unassign orders
    /// </summary>
    internal class MoveOrdersToOtherDateCmd : MultiUnassignCmdBase
    {
        #region Overrided Members

        protected override string OperationSuccessStartedMessage
        {
            get { return (string)App.Current.FindResource("MovingOrdersStartSuccessfulMessage"); }
        }

        protected override string OperationIsFailedMessage
        {
            get { return (string)App.Current.FindResource("MovingFailedCommonMessage"); }
        }

        protected override string OrdersAreLockedMessage
        {
            get { return (string)App.Current.FindResource("LockedOrdersDuringMoveMessage"); }
        }

        protected override void _Execute(params object[] args)
        {
            Debug.Assert(args.Count() == 2);

            // If user try to drop orders to current date from current date - do nothing
            if ((DateTime)args[1] == App.Current.CurrentDate)
                return;

            base._Execute(args);
        }

        /// <summary>
        /// Overrided method for move orders. It will be called in parent class
        /// </summary>
        /// <param name="args"></param>
        protected override void _ProcessOrders(params object[] args)
        {
            Collection<Order> ordersToAssign = args[0] as Collection<Order>;
            DateTime targetDate = (DateTime)args[1];

            Debug.Assert(targetDate != null);
            Debug.Assert(ordersToAssign != null);

            // Include any paired orders
            ICollection<Schedule> currentSchedules = App.Current.Project.Schedules.Search(App.Current.CurrentDate);
            if (currentSchedules != null && currentSchedules.Count > 0)
            {
                Schedule schedule = currentSchedules.ElementAt(0);
                // Ensure unassigned orders is up to date
                schedule.UnassignedOrders = null;
                OptimizeAndEditHelpers.FixSchedule(App.Current.Project, schedule);
                ordersToAssign = RoutingCmdHelpers.GetOrdersIncludingPairs(schedule, ordersToAssign) as Collection<Order>;
            }

            // Change orders planned date
            foreach (Order order in ordersToAssign)
                order.PlannedDate = targetDate;

            App.Current.Project.Save();

            // add info message to MessageWindow
            string infoMessage = string.Format((string)App.Current.FindResource("DropOnDateCmdSuccessText"), ordersToAssign.Count, App.Current.CurrentDate.ToShortDateString(), targetDate.ToShortDateString());
            App.Current.Messenger.AddInfo(infoMessage);

            // clear unassigned orders for all schedule in current date 
            foreach (Schedule schedule in currentSchedules)
            {
                if (schedule.UnassignedOrders != null)
                {
                    schedule.UnassignedOrders.Dispose();
                    schedule.UnassignedOrders = null;
                }
            }

            // clear unassigned orders for all schedule in target date 
            ICollection<Schedule> targetSchedules = App.Current.Project.Schedules.Search(targetDate);
            foreach (Schedule schedule in targetSchedules)
            {
                if (schedule.UnassignedOrders != null)
                {
                    schedule.UnassignedOrders.Dispose();
                    schedule.UnassignedOrders = null;
                }
            }

            OptimizeAndEditPage optimizeAndEditPage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);

            if (ordersToAssign[0].PlannedDate != optimizeAndEditPage.CurrentSchedule.PlannedDate)
            {
                optimizeAndEditPage.DeleteStoredSelection(optimizeAndEditPage.CurrentSchedule.PlannedDate.Value);
            }

            // schedule has changed - all views should be refreshed
            optimizeAndEditPage.OnScheduleChanged(optimizeAndEditPage.CurrentSchedule);

            // update days statuses
            DayStatusesManager.Instance.UpdateDayStatus(App.Current.CurrentDate);
            DayStatusesManager.Instance.UpdateDayStatus(targetDate);
            
            // clear selection in OptimizeAndEditPage
            optimizeAndEditPage.SelectedItems.Clear();
        }

        #endregion
    }
}
