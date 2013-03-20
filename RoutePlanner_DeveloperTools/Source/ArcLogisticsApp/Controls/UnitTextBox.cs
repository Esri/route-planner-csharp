using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Attributes;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class to display formatted values in current locale.
    /// </summary>
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBlock))]
    internal class UnitTextBox : Control
    {
        #region dependency properties

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(UnitTextBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

        #endregion

        #region static constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        static UnitTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnitTextBox), new FrameworkPropertyMetadata(typeof(UnitTextBox)));
        }

        #endregion

        #region private static methods

        /// <summary>
        /// React on Value property changed.
        /// </summary>
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((UnitTextBox)obj)._OnValueChanged();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Value in unit TextBox.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Control template apply.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _TextBox = (TextBlock)this.GetTemplateChild("PART_TextBox");
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(UnitTextBox_DataContextChanged);

            _Init();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Data row context changed.
        /// </summary>
        private void UnitTextBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _Init();
        }

        /// <summary>
        /// Initialize control.
        /// </summary>
        private void _Init()
        {
            Xceed.Wpf.DataGrid.CellContentPresenter cellContentPresenter = this.VisualParent as Xceed.Wpf.DataGrid.CellContentPresenter;
            if (cellContentPresenter == null)
                return;

            Xceed.Wpf.DataGrid.DataCell dataCell = cellContentPresenter.TemplatedParent as Xceed.Wpf.DataGrid.DataCell;
            string columnName = dataCell.ParentColumn.FieldName;
            ESRI.ArcLogistics.Data.DataObject dataObject = dataCell.ParentRow.DataContext as ESRI.ArcLogistics.Data.DataObject;

            if (dataObject != null)
            {
                int capacityPropertyIndex = Capacities.GetCapacityPropertyIndex(columnName);
                if (capacityPropertyIndex != -1)
                {
                    CapacityInfo capacityInfo = App.Current.Project.CapacitiesInfo[capacityPropertyIndex];
                    if (RegionInfo.CurrentRegion.IsMetric)
                        _displayUnits = capacityInfo.DisplayUnitMetric;
                    else
                        _displayUnits = capacityInfo.DisplayUnitUS;

                    _valueUnits = _displayUnits;
                }
                else
                {
                    Type type = dataObject.GetType();
                    PropertyInfo property = type.GetProperty(columnName);

                    UnitPropertyAttribute unitAttribute = (UnitPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(UnitPropertyAttribute));

                    _displayUnits = (RegionInfo.CurrentRegion.IsMetric) ? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                    _valueUnits = unitAttribute.ValueUnits;
                }

                _TextBox.Text = UnitFormatter.Format(0.0, _displayUnits);

                _inited = true;

                _SetTextToInnerTextBox();
            }
            else
            {
                // If this is not DataObject
                Break breakObject = dataCell.ParentRow.DataContext as Break;
                if (breakObject != null)
                {
                    // If this is Break. Get it`s type and initiate control in a proper way.
                    Type type = breakObject.GetType();

                    PropertyInfo property = type.GetProperty(columnName);
                    if (property != null)
                    {
                        Attribute attr = Attribute.GetCustomAttribute(property, typeof(UnitPropertyAttribute));

                        UnitPropertyAttribute unitAttribute = (UnitPropertyAttribute)attr;

                        _displayUnits = (RegionInfo.CurrentRegion.IsMetric) ? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                        _valueUnits = unitAttribute.ValueUnits;
                    }

                    _TextBox.Text = UnitFormatter.Format(0.0, _displayUnits);

                    _inited = true;

                    _SetTextToInnerTextBox();
                }
            }
        }

        /// <summary>
        /// React on dependency property changing.
        /// </summary>
        private void _OnValueChanged()
        {
            if (_TextBox != null)
                _SetTextToInnerTextBox();
        }

        /// <summary>
        /// Set Text property to textbox using correct region format.
        /// </summary>
        private void _SetTextToInnerTextBox()
        {
            if (!_inited || this.Value == null)
            {
                if (_inited)
                {
                    // Clear value.
                    _TextBox.Text = string.Empty;
                }

                return;
            }

            double value = (double)this.Value;

            if (_valueUnits != _displayUnits)
                value = UnitConvertor.Convert(value, _valueUnits, _displayUnits);

            string valueToDisplay = UnitFormatter.Format(value, _displayUnits);

            _TextBox.Text = valueToDisplay;
        }

        #endregion

        #region private members

        private TextBlock _TextBox;

        private Unit _valueUnits;
        private Unit _displayUnits;
        private bool _inited;

        #endregion
    }
}