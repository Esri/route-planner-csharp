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
using System.Windows.Controls;
using System.Globalization;

namespace ESRI.ArcLogistics.App.Pages
{
    #region ExportProfileWrap class
    /// <summary>
    /// Class export profile validators wrap
    /// </summary>
    internal class ExportProfileWrap
    {
        public ExportProfileWrap()
        { }

        public string Name
        {
            set { _currName = value; }
            get { return _currName; }
        }

        public string Type
        {
            set { _type = value; }
            get { return _type; }
        }

        public string Format
        {
            set { _format = value; }
            get { return _format; }
        }

        public string FilePath
        {
            set { _file = value; }
            get { return _file; }
        }

        public static string StartName
        {
            set { _prevName = value; }
            get { return _prevName; }
        }

        private string _currName = null;
        private string _file = null;
        private string _type = null;
        private string _format = null;

        private static string _prevName = null;
    }

    #endregion // ExportProfileWrap class

    #region Validation Rule classes
    /// <summary>
    /// Class defines validation rule for export profile not empty property.
    /// </summary>
    internal class ExportProfileNotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isObjectEmpty = true;
            if (null != value)
                isObjectEmpty = string.IsNullOrEmpty(value.ToString().Trim());

            return (isObjectEmpty)? new ValidationResult(false, App.Current.FindString("ExportProfileValueIsEmpty")) :
                                    ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// Class defines validation rule for export file name.
    /// </summary>
    internal class ExportProfileFileNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // NOTE: null value is valid
            string message = null;
            if ((null != value) && !string.IsNullOrEmpty(value.ToString()))
            {
                if (!FileHelpers.ValidateFilepath(value.ToString()))
                    message = App.Current.FindString("ExportProfileVFileNameInvalid");
            }

            return (null == message)? ValidationResult.ValidResult :
                                      new ValidationResult(false, message);
        }
    }

    #endregion // Validation Rule classes
}
