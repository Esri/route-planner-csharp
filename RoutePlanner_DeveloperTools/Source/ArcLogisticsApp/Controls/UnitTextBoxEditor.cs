using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using Xceed.Wpf.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class to edit formatted values in current locale.
    /// </summary>
    [TemplatePart(Name = "PART_TextBox", Type = typeof(NumericTextBox))]
    internal class UnitTextBoxEditor : Control
    {
        #region Dependency properties

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(UnitTextBoxEditor),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

        #endregion

        #region static constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        static UnitTextBoxEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnitTextBoxEditor), new FrameworkPropertyMetadata(typeof(UnitTextBoxEditor)));
        }

        #endregion

        #region private static methods

        /// <summary>
        /// React on Value property changed.
        /// </summary>
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((UnitTextBoxEditor)obj)._OnValueChanged();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Value in unit TextBox.
        /// </summary>
        public object Value
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Control template apply.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _TextBox = (NumericTextBox)this.GetTemplateChild("PART_TextBox");

            this.Loaded += new RoutedEventHandler(UnitTextBoxEditor_Loaded);
            _TextBox.TextChanged += new TextChangedEventHandler(_TextBox_TextChanged);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Control loaded.
        /// </summary>
        void UnitTextBoxEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Xceed.Wpf.DataGrid.CellContentPresenter cellContentPresenter = this.VisualParent as Xceed.Wpf.DataGrid.CellContentPresenter;
            Xceed.Wpf.DataGrid.DataCell dataCell = cellContentPresenter.TemplatedParent as Xceed.Wpf.DataGrid.DataCell;
            string columnName = dataCell.ParentColumn.FieldName;
            ESRI.ArcLogistics.Data.DataObject dataObject = dataCell.ParentRow.DataContext as ESRI.ArcLogistics.Data.DataObject;

            if (dataObject != null)
            {
                Type type = dataObject.GetType();

                PropertyInfo property = type.GetProperty(columnName);
                if (property != null)
                {
                    Attribute attr = Attribute.GetCustomAttribute(property, typeof(UnitPropertyAttribute));

                    UnitPropertyAttribute unitAttribute = (UnitPropertyAttribute)attr;

                    _typeConverter = TypeDescriptor.GetConverter(property.PropertyType);

                    _displayUnits = (RegionInfo.CurrentRegion.IsMetric) ? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                    _valueUnits = unitAttribute.ValueUnits;
                }

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

                        _typeConverter = TypeDescriptor.GetConverter(property.PropertyType);

                        _displayUnits = (RegionInfo.CurrentRegion.IsMetric) ? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
                        _valueUnits = unitAttribute.ValueUnits;
                    }

                    _inited = true;

                    _SetTextToInnerTextBox();
                }
            }
        }

        /// <summary>
        /// React on inner textbox value changed.
        /// </summary>
        private void _TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If value changed from UI - set new value of Text property.
            if (_pauseTextChangeEventHandler)
                return;

            object obj = null;
            try
            {
                if (_typeConverter == null)
                {
                    obj = double.Parse(_TextBox.Text);
                }
                else
                {
                    obj = _typeConverter.ConvertFromString(_TextBox.Text);
                }

                if (_valueUnits != _displayUnits)
                {
                    obj = UnitConvertor.Convert((double)obj, _displayUnits, _valueUnits);
                }

                if ((double)this.Value != (double)obj)
                    this.Value = (double)obj;
            }
            catch
            { }
        }

        /// <summary>
        /// React on dependency property changing.
        /// </summary>
        private void _OnValueChanged()
        {
            _pauseTextChangeEventHandler = true;
            if (_TextBox != null)
                _SetTextToInnerTextBox();
            _pauseTextChangeEventHandler = false;
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
                    _TextBox.Text = "";
                }

                return;
            }

            object obj = this.Value;
            if (_valueUnits != _displayUnits)
                obj = UnitConvertor.Convert((double)obj, _valueUnits, _displayUnits);

            _TextBox.Text = obj.ToString();

            if (!_needAfterInit)
                _needAfterInit = true;
        }

        #endregion

        #region private members

        private bool _pauseTextChangeEventHandler = false;

        private NumericTextBox _TextBox;

        private TypeConverter _typeConverter;
        private Unit _valueUnits;
        private Unit _displayUnits;
        private bool _inited;
        private bool _needAfterInit;

        #endregion
    }
}