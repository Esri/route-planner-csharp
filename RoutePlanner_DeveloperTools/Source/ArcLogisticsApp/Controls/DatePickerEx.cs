using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;

using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class inherited from WpfToolkit DatePicker. 
    /// Overloaded to allow user commit changes in date time fields by press "Enter" and "Tab" keys
    /// </summary>
    internal class DatePickerEx : DatePicker
    {
        #region Constructors

        public DatePickerEx()
        {
            // add handler to key down event to react when user press "Enter". 
            this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(DatePickerEx_PreviewKeyDown);
        }

        #endregion

        #region Private Event Handlers

        private void DatePickerEx_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Tab && e.Key != System.Windows.Input.Key.Enter)
                return;

            // get current value of control's text (need to save changes)
            DateTime? editedDate = _StringToDateTime(this.Text);

            if (editedDate != null)
                this.SelectedDate = editedDate;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                DataGridControl parentControl = XceedVisualTreeHelper.GetGridByEditor(this);
                if (parentControl != null) // if parent grid is found - call "EndEdit" method. It commits new item or saves changes.
                {
                    try
                    {
                        parentControl.EndEdit();
                    }
                    catch
                    {
                        // NOTE : if current object is invalid and cannot be commited only stay focus in control
                        parentControl.Focus(); 
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method converts text to DateTime. Returns "null" if text has invalid format.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private DateTime? _StringToDateTime(string text)
        {
            DateTime? editedDate = null;

            // try to convert
            try
            {
                editedDate = Convert.ToDateTime(this.Text);
            }
            catch
            {
                // NOTE : input date is invalid format
            }

            return editedDate;
        }

        #endregion
    }
}
