/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Base class for routing command option. 
    /// </summary>
    internal abstract class MoveToCommandOptionBase : RoutingCommandBase, ICommandOption
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of MoveToCommandOptionBase and add handler to schedulePage.Selection changed.
        /// </summary>
        public MoveToCommandOptionBase()
        {
            _schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            Debug.Assert(_schedulePage != null);
            _schedulePage.SelectionChanged += new EventHandler(_SelectionChanged);

            Debug.Assert(App.Current.Solver != null);
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(_AsyncSolveCompleted);

            TooltipText = EnabledTooltip;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets processed object ID.
        /// </summary>
        public Guid ObjectId
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets/sets isEnabled property.
        /// </summary>
        bool ICommandOption.IsEnabled
        {
            get 
            {
                return _isEnabled; 
            }
            set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(IS_ENABLED_PROPERTY_NAME);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts process command. 
        /// </summary>
        public void Execute()
        {
            _Execute();
        }

        #endregion

        #region Overrided members

        public override string Name
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Overrided ToString to show title.
        /// </summary>
        /// <returns>Option title.</returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion

        #region ICommandOption Members

        /// <summary>
        /// Gets option ID.
        /// </summary>
        public int Id
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets option Group ID.
        /// </summary>
        public int GroupID
        {
            get;
            protected set;
        }

        #endregion

        #region RoutingCommandBase Members

        /// <summary>
        /// Gets message string about solve completed successfully. 
        /// </summary>
        /// <param name="schedule">Edited schedule.</param>
        /// <param name="info">Operation info.</param>
        /// <returns>Success solve string.</returns>
        protected override string _FormatSuccessSolveCompletedMsg(Schedule schedule, ESRI.ArcLogistics.Routing.AsyncOperationInfo info)
        {
            return RoutingMessagesHelper.GetAssignOperationCompletedMessage(info);
        }

        /// <summary>
        /// Excecutes command.
        /// </summary>
        /// <param name="args"></param>
        protected override void _Execute(params object[] args)
        {
            try
            {
                OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
                Schedule schedule = schedulePage.CurrentSchedule;

                // Get selected orders.
                Collection<Order> selectedOrders = _GetOrdersWhichCanBeMovedFromSelection(schedulePage.SelectedItems);
                selectedOrders = RoutingCmdHelpers.GetOrdersIncludingPairs(schedule, selectedOrders) as Collection<Order>;
                bool keepViolOrdrsUnassigned = false;

                Debug.Assert(args[0] != null);
                ICollection<Route> targetRoutes = args[0] as ICollection<Route>;
                Debug.Assert(targetRoutes != null);

                string routeName = args[1] as string;

                if (_CheckRoutingParams(schedule, targetRoutes, selectedOrders))
                {
                    SolveOptions options = new SolveOptions();
                    options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                    options.FailOnInvalidOrderGeoLocation = false;

                    _SetOperationStartedStatus((string)App.Current.FindResource(ASSIGN_ORDERS), (DateTime)schedule.PlannedDate);

                    // Start "Assign to best other route" operation.
                    OperationsIds.Add(App.Current.Solver.AssignOrdersAsync(schedule, selectedOrders,
                                                                           targetRoutes, null,
                                                                           keepViolOrdrsUnassigned,
                                                                           options));
                    // Set solve started message
                    string infoMessage = RoutingMessagesHelper.GetAssignOperationStartedMessage(selectedOrders, routeName);

                    if (!string.IsNullOrEmpty(infoMessage))
                        App.Current.Messenger.AddInfo(infoMessage);
                }
            }
            catch (RouteException e)
            {
                if (e.InvalidObjects != null) // If exception throw because any Routes or Orders are invalid
                    _ShowSolveValidationResult(e.InvalidObjects);
                else
                    _ShowErrorMsg(RoutingCmdHelpers.FormatRoutingExceptionMsg(e));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                if ((e is LicenseException) || (e is AuthenticationException) || (e is CommunicationException))
                    CommonHelpers.AddRoutingErrorMessage(e);
                else
                    throw;
            }           
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets/sets enabled tooltip property.
        /// </summary>
        protected string EnabledTooltip
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets disabled tooltip property.
        /// </summary>
        protected string DisabledTooltip
        {
            get;
            set;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Checks whether command option is enabled.
        /// </summary>
        protected abstract void _CheckEnabled(OptimizeAndEditPage schedulePage);

        #endregion

        #region Private Methods

        /// <summary>
        /// Method gets orders collection from selection collection
        /// </summary>
        /// <param name="selection">Selected items.</param>
        /// <returns>Orders from selection.</returns>
        private Collection<Order> _GetOrdersWhichCanBeMovedFromSelection(IList selection)
        {
            Collection<Order> orders = new Collection<Order>();
            foreach (Object obj in selection)
            {
                // If selected object is order - add it to collection.
                if (obj is Order)
                    orders.Add((Order)obj);

                // If selected object is stop and not locked - add it to collection.
                else if (obj is Stop && !((Stop)obj).IsLocked && ((Stop)obj).StopType == StopType.Order)
                    orders.Add((Order)((Stop)obj).AssociatedObject);

                // If selected object is route - add to collection all it's unlocked stops.
                else if (obj is Route && !((Route)obj).IsLocked) 
                {
                    foreach (Stop stop in ((Route)obj).Stops)
                    {
                        if (!stop.IsLocked && stop.AssociatedObject is Order)
                            orders.Add((Order)stop.AssociatedObject);
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// Updates tooltip.
        /// </summary>
        private void _UpdateTooltip()
        {
            if (IsEnabled)
                TooltipText = EnabledTooltip;
            else
                TooltipText = DisabledTooltip;

            _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Checks whether command is enabled in current selection.
        /// </summary>
        /// <param name="sender">Schedule page.</param>
        /// <param name="e">Event args.</param>
        private void _SelectionChanged(object sender, EventArgs e)
        {
            Debug.Assert(_schedulePage != null);
            _CheckEnabled(_schedulePage);
            _UpdateTooltip();
        }

        /// <summary>
        /// Updates IsEnabled state and tooltip when solve completed.
        /// </summary>
        /// <param name="sender">Solver.</param>
        /// <param name="e">Event args.</param>
        private void _AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            Debug.Assert(_schedulePage != null);
            _CheckEnabled(_schedulePage);
            _UpdateTooltip();
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// TooltipText text.
        /// </summary>
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        /// <summary>
        /// IsEnabled string.
        /// </summary>
        private const string IS_ENABLED_PROPERTY_NAME = "IsEnabled";

        /// <summary>
        /// AssignOrders string.
        /// </summary>
        private const string ASSIGN_ORDERS = "AssignOrders";

        #endregion

        #region Private Fields

        /// <summary>
        /// IsEnabled flag.
        /// </summary>
        private bool _isEnabled = false;

        /// <summary>
        /// OptimizeAndEdit page.
        /// </summary>
        private OptimizeAndEditPage _schedulePage = null;

        #endregion
    }
}
