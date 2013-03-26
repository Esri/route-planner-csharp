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
using Xceed.Wpf.DataGrid;
using ESRI.ArcLogistics.App.OrderSymbology;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid.ValidationRules;

namespace ESRI.ArcLogistics.App.Validators
{
    class QuantityValidationRule : CellValidationRule
    {
        public QuantityValidationRule()
        {
            
        }

        public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo culture, Xceed.Wpf.DataGrid.CellValidationContext context)
        {
            if (value == null)
                return ValidationResult.ValidResult;

            Cell cell = context.Cell;

            OrderQuantity checkingObject = cell.DataContext as OrderQuantity;
            double minValue = checkingObject.MinValue;
            double maxValue = checkingObject.MaxValue;

            if (value is string)
                value = double.Parse((string)value);
            if (cell.FieldName == OrderQuantity.PROP_NAME_MinValue)
                minValue = (double)value;
            else if (cell.FieldName == OrderQuantity.PROP_NAME_MaxValue)
                maxValue = (double)value;
            else
                System.Diagnostics.Debug.Assert(false);

            string newValue = value.ToString();
            if (minValue > maxValue)
                return new ValidationResult(false, (string)App.Current.FindResource("NotValidRangeText"));

            return ValidationResult.ValidResult;
        }
    }
}
