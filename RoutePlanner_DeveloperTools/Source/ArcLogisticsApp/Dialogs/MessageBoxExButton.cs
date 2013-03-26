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
using System.Windows;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Standard MessageBoxEx buttons wrap.
    /// </summary>
    internal enum MessageBoxExButtonType
    {
        Ok,
        Cancel,
        Yes,
        No,
        Abort,
        Retry,
        Ignore
    }

    /// <summary>
    /// Internal DataStructure used to represent a button.
    /// </summary>
    internal class MessageBoxExButton
    {
        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the text of the button.
        /// </summary>
        public string Caption
        {
            get{ return _caption; }
            set{ _caption = value; }
        }

        /// <summary>
        /// Gets or sets the return value when this button is clicked.
        /// </summary>
        public MessageBoxExButtonType Value
        {
            get{ return _value; }
            set{_value = value; }
        }

        /// <summary>
        /// Gets or sets the help text of the button.
        /// </summary>
        public string HelpText
        {
            get { return _helpText; }
            set { _helpText = value; }
        }

        /// <summary>
        /// Gets or sets wether this button is a cancel button.
        /// </summary>
        /// <remarks>The button that will be assumed to have been clicked
        /// if the user closes the message box without pressing any button.
        /// </remarks>
        public bool IsCancelButton
        {
            get { return _isCancelButton; }
            set { _isCancelButton = value; }
        }

        #endregion // Public properties

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Button caption.
        /// </summary>
        private string _caption = null;

        /// <summary>
        /// Button value.
        /// </summary>
        private MessageBoxExButtonType _value = MessageBoxExButtonType.Cancel;

        /// <summary>
        /// Cancel button flag.
        /// </summary>
        private bool _isCancelButton = false;

        /// <summary>
        /// Tooltip text.
        /// </summary>
        private string _helpText = null;

        #endregion // Private fields
    }
}
