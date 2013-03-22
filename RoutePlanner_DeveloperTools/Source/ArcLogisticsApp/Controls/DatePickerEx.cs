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
