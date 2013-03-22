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
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Commands
{
    class DuplicateOrdersCmd: OrdersCommandBase
    {
        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateOrders";

        #region Public Override Members

        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            protected set
            {
                base.IsEnabled = value;

                if (value)
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DuplicateCommandDisabledTooltip");
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
                return (string)App.Current.FindResource("DuplicateOrdersCommandTitle");
            }
        }

        #endregion

        #region Portected Override Members

        protected override void _Execute(params object[] args)
        {
            Collection<Order> selectedOrders = new Collection<Order>();

            foreach (Order order in _GetUnassignedOrdersFromSelection())
            {
                Order newOrder = order.Clone() as Order;
                selectedOrders.Add(newOrder);
            }

            Project project = App.Current.Project;
            foreach (Order order in selectedOrders)
            {
                order.Name = DataObjectNamesConstructor.GetDuplicateName(order.Name, _GetUnassignedOrdersFromSelection());
                project.Orders.Add(order);
            }

            project.Save();

            StatusBuilder statusBuilder = new StatusBuilder();
            statusBuilder.FillSelectionStatusWithoutCollectionSize(CurrentSchedule.UnassignedOrders.Count, (string)App.Current.FindResource("Order"), 0, OptimizePage);
        }

        protected override void _CheckEnabled()
        {
            if (_GetUnassignedOrdersFromSelection().Count == 1
               && CurrentSchedule.UnassignedOrders != null
               && CurrentSchedule.UnassignedOrders.Count > 0
               && !OptimizePage.IsEditingInProgress)
                IsEnabled = true;
            else
                IsEnabled = false;
        }

        #endregion

        #region Private Members

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;

        #endregion

    }
}
