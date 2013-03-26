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
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command geocode locations
    /// </summary>
    class GeocodeOrdersCmd : OrdersCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.GeocodeOrdersCmd";

        #endregion

        #region Override members

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
                return (string)App.Current.FindResource("GeocodeCommandTitle");
            }
        }

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
                    TooltipText = (string)App.Current.FindResource("RematchAddressCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("RematchAddressCommandDisabledTooltip");
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

        protected override void _Execute(params object[] args)
        {
            _Geocode();
        }

        /// <summary>
        ///  Method checks is command enabled
        /// </summary>
        protected override void _CheckEnabled()
        {
            IsEnabled = !OptimizePage.IsLocked && !OptimizePage.IsEditingInProgress && 
                OptimizePage.SelectedItems.Count == 1 && OptimizePage.SelectedItems[0] is Order;
        }

        #endregion

        #region GeocodeCommandBase Protected Methods

        /// <summary>
        /// Geocode order
        /// </summary>
        private void _Geocode()
        {
            if (OptimizePage.SelectedItems.Count == 1)
            {
                IGeocodable order = (IGeocodable)OptimizePage.SelectedItems[0];
                OptimizePage.StartGeocoding(order);
            }
        }

        #endregion GeocodeCommandBase Protected Methods

        #region Private Fields

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;

        #endregion
    }
}