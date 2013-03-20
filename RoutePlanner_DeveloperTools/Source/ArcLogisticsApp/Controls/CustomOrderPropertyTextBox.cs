using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

using Xceed.Wpf.DataGrid;
using Xceed.Wpf.Controls;

using ESRI.ArcLogistics.DomainObjects;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class to edit text with user's dynamical length
    /// </summary>
    internal class CustomOrderPropertyTextBox : AutoSelectTextBox
    {
        #region Static constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public CustomOrderPropertyTextBox()
        {
            this.Loaded += new RoutedEventHandler(_Loaded);
        }

        #endregion // Static constructor

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Control loaded
        /// </summary>
        private void _Loaded(object sender, RoutedEventArgs e)
        {
            CellContentPresenter cellContentPresenter = this.VisualParent as CellContentPresenter;
            if (cellContentPresenter == null)
                return;

            // initialize control
            DataCell dataCell = cellContentPresenter.TemplatedParent as DataCell;
            string columnName = dataCell.ParentColumn.FieldName;
            ESRI.ArcLogistics.Data.DataObject dataObject = dataCell.ParentRow.DataContext as ESRI.ArcLogistics.Data.DataObject;

            if (dataObject != null)
            {
                int index = OrderCustomProperties.GetCustomPropertyIndex(columnName);
                if (-1 != index)
                {
                    OrderCustomProperty info = App.Current.Project.OrderCustomPropertiesInfo[index];
                    this.MaxLength = info.Length;
                }
                else
                {
                    Debug.Assert(false); // NOTE: not supported
                }
            }
            Keyboard.Focus(this);
        }

        #endregion // Private methods
    }

    /// <summary>
    /// Class to edit numeric with user's dynamical length
    /// </summary>
    internal class CustomOrderPropertyNumericTextBox : NumericTextBox
    {
        #region Static constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public CustomOrderPropertyNumericTextBox()
        {
            this.Loaded += new RoutedEventHandler(_Loaded);
        }

        #endregion // Static constructor

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Control loaded
        /// </summary>
        private void _Loaded(object sender, RoutedEventArgs e)
        {
            CellContentPresenter cellContentPresenter = this.VisualParent as CellContentPresenter;
            if (cellContentPresenter == null)
                return;

            // initialize control
            DataCell dataCell = cellContentPresenter.TemplatedParent as DataCell;
            string columnName = dataCell.ParentColumn.FieldName;
            ESRI.ArcLogistics.Data.DataObject dataObject = dataCell.ParentRow.DataContext as ESRI.ArcLogistics.Data.DataObject;

            if (dataObject != null)
            {
                int index = OrderCustomProperties.GetCustomPropertyIndex(columnName);
                if (-1 != index)
                {
                    OrderCustomProperty info = App.Current.Project.OrderCustomPropertiesInfo[index];
                    this.MaxLength = info.Length;
                }
                else
                {
                    Debug.Assert(false); // NOTE: not supported
                }
            }
            Keyboard.Focus(this);

            this.GotFocus += new RoutedEventHandler(_GotFocus);
        }

        private void _GotFocus(object sender, RoutedEventArgs e)
        {
            this.SelectAll();
        }

        protected override void ValidateValue(object value)
        {
            if ((null != value) && (value is double))
                base.ValidateValue(value);
        }

        #endregion // Private methods
    }
}