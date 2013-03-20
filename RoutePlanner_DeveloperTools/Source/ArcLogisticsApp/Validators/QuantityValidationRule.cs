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
