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
using System.Windows;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Represents multiple custom order symbols adornment.
    /// </summary>
    class MultiMapSymbolAdornment : MultiOrderAdornmentBase
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>MultiMapSymbolAdornment</c> class.
        /// </summary>
        public MultiMapSymbolAdornment(IList<object> ordersAndStops)
            : base (ordersAndStops)
        {

        }

        #endregion

        #region Protected Overriden Methods

        protected override FrameworkElement CreateOrderElement(object orderOrStop)
        {
            if (orderOrStop is Stop && App.Current.MapDisplay.LabelingEnabled)
                return AdornHelpers.CreateLabelSequenceSymbol(orderOrStop as Stop);
            else
                return AdornHelpers.CreateCustomOrderSymbol(orderOrStop);
        }

        #endregion
    }
}
