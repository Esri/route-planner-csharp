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
    class CategoryValidationRule: CellValidationRule
    {
        public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo culture, Xceed.Wpf.DataGrid.CellValidationContext context)
        {
            Cell cell = context.Cell;

            OrderCategory checkingObject = cell.DataContext as OrderCategory;

            if (checkingObject != null)
            {
                if (value != null)
                {
                    string newValue = value.ToString();

                    foreach (OrderCategory element in SymbologyManager.OrderCategories)
                        if (!element.Equals(checkingObject) && element.Value != null && element.Value.Equals(newValue))
                            return new ValidationResult(false, (string)App.Current.FindResource("ValueValidationRuleText"));
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}
