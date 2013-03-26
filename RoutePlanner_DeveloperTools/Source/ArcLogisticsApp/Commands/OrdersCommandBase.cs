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
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Commands
{
    class OrdersCommandBase : CommandBase
    {
        #region Override Members

        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = false;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);
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
                _NotifyPropertyChanged("IsEnabled");
            }
        }

        protected override void _Execute(params object[] args)
        {
            //throw new NotImplementedException();
        }

        public override string Name
        {
            get { return null; }
        }

        public override string Title
        {
            get { return null; }
        }

        public override string TooltipText
        {
            get { return null; }
            protected set { }
        }

        #endregion

        #region Event Handlers

        protected void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _optimizePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            
            _optimizePage.CurrentScheduleChanged += new EventHandler(optimizePage_CurrentScheduleChanged);
            _optimizePage.SelectionChanged += new EventHandler(_schedulePage_SelectionChanged);
            _optimizePage.EditBegun += new DataObjectEventHandler(_optimizePage_EditBegun);
            _optimizePage.EditCommitted += new DataObjectEventHandler(_optimizePage_EditCommitted);
            _optimizePage.EditCanceled += new ESRI.ArcLogistics.App.Pages.DataObjectEventHandler(_optimizePage_EditCanceled);
        }

        private void _optimizePage_EditCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_EditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        protected void _optimizePage_EditCanceled(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        protected void optimizePage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            _currentSchedule = _optimizePage.CurrentSchedule;
            _CheckEnabled();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///  Method checks is command enabled
        /// </summary>
        protected virtual void _CheckEnabled()
        {
            if (_GetUnassignedOrdersFromSelection().Count > 0
               && CurrentSchedule.UnassignedOrders != null
               && CurrentSchedule.UnassignedOrders.Count > 0
               && !OptimizePage.IsEditingInProgress)
                IsEnabled = true;
            else
                IsEnabled = false;
        }

        /// <summary>
        /// Methods returns Unassigned orders from selection 
        /// </summary>
        /// <returns></returns>
        protected Collection<Order> _GetUnassignedOrdersFromSelection()
        {
            Collection<Order> unassignedOrders = new Collection<Order>();

            foreach (Object obj in OptimizePage.SelectedItems)
            {
                if (obj is Order)
                {
                    unassignedOrders.Add((Order)obj);
                }
            }

            return unassignedOrders;
        }

        protected Schedule CurrentSchedule
        {
            get { return _currentSchedule; }
        }

        protected OptimizeAndEditPage OptimizePage
        {
            get { return _optimizePage; }
        }

        #endregion

        #region Private Fields

        Schedule _currentSchedule;
        bool _isEnabled;
        OptimizeAndEditPage _optimizePage;

        #endregion
    }
}
