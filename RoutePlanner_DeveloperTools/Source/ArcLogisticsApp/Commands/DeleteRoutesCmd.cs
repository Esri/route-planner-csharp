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
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    class DeleteRoutesCmd : RoutesCommandBase
    {
        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteRoutes";

        #region Public Override Members

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource("DeleteRoutesCommandTitle");
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(IS_ENABLED_PROPERTY_NAME);

                if (value)
                    TooltipText = (string)App.Current.FindResource("DeleteCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DeleteCommandDisabledTooltip");
            }
        }

        public override string TooltipText
        {
            get
            {
                return _tooltipText;
            }
            protected set
            {
                _tooltipText = value;
                _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
            }
        }

        #endregion

        #region Protected Override Members

        protected override void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            base.Current_ApplicationInitialized(sender, e);
            OptimizePage.NewObjectCreated += new ESRI.ArcLogistics.App.Pages.DataObjectEventHandler(OptimizePage_NewObjectCreated);
            OptimizePage.NewObjectCommitted += new ESRI.ArcLogistics.App.Pages.DataObjectEventHandler(OptimizePage_NewObjectCommitted);
            OptimizePage.NewObjectCanceled += new ESRI.ArcLogistics.App.Pages.DataObjectEventHandler(OptimizePage_NewObjectCanceled);

            KeyGesture = new KeyGesture(INVOKE_KEY);
        }

        protected override void _Execute(params object[] args)
        {
            // If editing is process in optimize and edit page - cancel editing.
            if (OptimizePage.IsEditingInProgress)
                ((ICancelDataObjectEditing)OptimizePage).CancelObjectEditing();

            Collection<Route> selected = _GetRoutesFromSelection();
            if (0 < selected.Count)
            {
                bool doProcess = true;
                if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
                    // show warning dialog
                    doProcess = DeletingWarningHelper.Execute(selected, "Route", "Routes");

                if (doProcess)
                {
                    WorkingStatusHelper.SetBusy((string)App.Current.FindResource("DeletingRoutesStatus"));

                    // Workaround: Xceed throw exception when deleting expanded details view.
                    //      Collapse route details before deleting.
                    List<Xceed.Wpf.DataGrid.DataGridContext> dataGridContexts = new List<Xceed.Wpf.DataGrid.DataGridContext>(OptimizePage.RoutesView.RoutesGrid.GetChildContexts());
                    foreach (Xceed.Wpf.DataGrid.DataGridContext dataGridContext in dataGridContexts)
                    {
                        for (int index = 0; index < selected.Count; ++index)
                        {
                            if (dataGridContext.ParentItem.Equals(selected[index]))
                            {
                                dataGridContext.ParentDataGridContext.CollapseDetails(dataGridContext.ParentItem);
                                break; // Exit. For this data grid context all done.
                            }
                        }
                    }

                    // Save current selected index.
                    int previousSelectedIndex = OptimizePage.RoutesView.RoutesGrid.SelectedIndex;

                    // Delete routes.
                    for (int index = 0; index < selected.Count; ++index)
                        CurrentSchedule.Routes.Remove(selected[index]);

                    Project project = App.Current.Project;
                    project.Save();

                    // Set unassigned orders.
                    if (CurrentSchedule.UnassignedOrders != null)
                        CurrentSchedule.UnassignedOrders.Dispose();

                    CurrentSchedule.UnassignedOrders = project.Orders.SearchUnassignedOrders(CurrentSchedule, true);

                    WorkingStatusHelper.SetReleased();

                    // Schedule has changed - all views should be refreshed.
                    OptimizePage.OnScheduleChanged(CurrentSchedule);

                    // Select item, which goes after deleted. Special logic used for routes, because schedule reloads.
                    int newSelectedIndex = OptimizePage.RoutesView.RoutesGrid.Items.Count - 1;
                    if (OptimizePage.RoutesView.RoutesGrid.Items.Count > previousSelectedIndex)
                    {
                        newSelectedIndex = previousSelectedIndex;
                    }

                    if (newSelectedIndex != -1)
                    {
                        OptimizePage.Dispatcher.BeginInvoke(new ParamsDelegate(_SelectItem),
                            DispatcherPriority.Input, OptimizePage.RoutesView.RoutesGrid.Items[newSelectedIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Select item in routes grid.
        /// </summary>
        /// <param name="item">Item to select.</param>
        private void _SelectItem(object item)
        {
            OptimizePage.RoutesView.RoutesGrid.SelectedItem = item;
        }

        protected override void _CheckEnabled()
        {
            bool isRouteEditing = (OptimizePage.IsEditingInProgress && OptimizePage.EditingManager.EditedObject != null && OptimizePage.EditingManager.EditedObject is Route);

            Schedule schedule = CurrentSchedule;
            IsEnabled = ((null != schedule) && (null != schedule.Routes) && (0 < schedule.Routes.Count)
                        && (_GetRoutesFromSelection().Count > 0 || isRouteEditing));
        }

        #endregion

        #region Event handlers

        private void OptimizePage_NewObjectCanceled(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void OptimizePage_NewObjectCommitted(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void OptimizePage_NewObjectCreated(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Key to invoke command.
        /// </summary>
        private const Key INVOKE_KEY = Key.Delete;

        #endregion

        #region Private Members

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";
        private const string IS_ENABLED_PROPERTY_NAME = "IsEnabled";

        private string _tooltipText = null;
        private bool _isEnabled;

        /// <summary>
        /// Delegate with item parameter.
        /// </summary>
        private delegate void ParamsDelegate(object item);

        #endregion
    }
}
