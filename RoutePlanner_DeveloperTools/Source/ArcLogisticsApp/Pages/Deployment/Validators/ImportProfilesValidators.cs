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
using System.Collections.Generic;

using EsriShapefile = ESRI.ArcLogistics.ShapefileReader;

using ESRI.ArcLogistics.App.Import;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Pages
{
    #region ImportProfileWrap class

    /// <summary>
    /// Class import profile validators wrapper.
    /// </summary>
    internal sealed class ImportProfileWrap
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>ImportProfileWrap</c> class.
        /// </summary>
        public ImportProfileWrap()
        { }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of profile.
        /// </summary>
        public string ProfileName
        {
            set { _currName = value; }
            get { return _currName; }
        }

        /// <summary>
        /// Type of profile.
        /// </summary>
        public string ProfileType
        {
            set { _type = value; }
            get { return _type; }
        }

        /// <summary>
        /// Source link of profile.
        /// </summary>
        public string SourceLink
        {
            set { _source = value; }
            get { return _source; }
        }

        /// <summary>
        /// Table name in source link.
        /// </summary>
        public string TableName
        {
            set { _table = value; }
            get { return _table; }
        }

        /// <summary>
        /// Start profile name.
        /// </summary>
        public static string StartProfileName
        {
            set { _prevName = value; }
            get { return _prevName; }
        }

        #endregion // Public properties

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current name.
        /// </summary>
        private string _currName;
        /// <summary>
        /// Source link.
        /// </summary>
        private string _source;
        /// <summary>
        /// Table name.
        /// </summary>
        private string _table;
        /// <summary>
        /// Type.
        /// </summary>
        private string _type;
        /// <summary>
        /// Previously name.
        /// </summary>
        private static string _prevName;

        #endregion // Private fields
    }

    #endregion // ImportProfileWrap class

    #region Validation Rule classes

    /// <summary>
    /// Class defines validation rule for import profile not empty property.
    /// </summary>
    internal sealed class ProfileNotEmptyValidationRule : ValidationRule
    {
        /// <summary>
        /// Does validate profile value.
        /// </summary>
        /// <param name="value">Profile value to validation.</param>
        /// <param name="cultureInfo">Ignored.</param>
        /// <returns>Validation result.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool isObjectEmpty = true;
            if (null != value)
                isObjectEmpty = string.IsNullOrEmpty(value.ToString().Trim());

            ValidationResult result = ValidationResult.ValidResult;
            if (isObjectEmpty)
            {
                string message = App.Current.FindString("ImportProfileValueIsEmpty");
                result = new ValidationResult(false, message);
            }

            return result;
        }
    }

    /// <summary>
    /// Class defines validation rule for import profile source link.
    /// </summary>
    internal sealed class ProfileSourceLinkValidationRule : ValidationRule
    {
        #region Internal properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Import type.
        /// </summary>
        internal static ImportType ImportType
        {
            set { _importType = value; }
        }

        #endregion // Internal properties

        #region Public overrided methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does validate profile source link.
        /// </summary>
        /// <param name="value">Profile source link to validation.</param>
        /// <param name="cultureInfo">Ignored.</param>
        /// <returns>Validation result.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // NOTE: null value is valid
            string message = null;
            if ((null != value) && !string.IsNullOrEmpty(value.ToString()))
            {
                string testString = value.ToString();

                if (DataSourceOpener.IsConnectionString(testString))
                    DataSourceOpener.IsConnectionPossible(testString, out message); // NOTE: ignore result
                else
                {
                    if (!System.IO.File.Exists(testString))
                        message = App.Current.FindString("ImportProfileNotFound");
                    else
                    {
                        if (FileHelpers.IsShapeFile(testString))
                            message = _CheckShapeFile();
                        else if ((ImportType.Zones == _importType) ||
                                 (ImportType.Barriers == _importType))
                            message = App.Current.FindString("ImportProfileInvalidDataSourceFormat");
                        else
                        {
                            string file = DataSourceOpener.FilePath; // store state
                            DataSourceOpener.FilePath = testString;
                            DataSourceOpener.IsConnectionPossible(DataSourceOpener.ConnectionString,
                                                                  out message); // NOTE: ignore result
                            DataSourceOpener.FilePath = file; // restore state
                        }
                    }
                }
            }

            return (null == message) ? ValidationResult.ValidResult :
                                      new ValidationResult(false, message);
        }

        #endregion // Public overrided methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks shape file.
        /// Check - shape file type is supported to selected import type.
        /// </summary>
        /// <returns></returns>
        private string _CheckShapeFile()
        {
            string message = null;
            if ((ImportType.Orders != _importType) &&
                (ImportType.Locations != _importType) &&
                (ImportType.Zones != _importType) &&
                (ImportType.Barriers != _importType))
                message = App.Current.FindString("ImportProfileNotSupportedSHPType");
            else
            {
                if (!SHPProvider.ProjectionIsRight(DataSourceOpener.FilePath, out message))
                    return message;

                EsriShapefile.ShapeType shapeType =
                    SHPProvider.GetShapeType(DataSourceOpener.FilePath, out message);
                if ((ImportType.Orders == _importType) ||
                    (ImportType.Locations == _importType))
                {
                    if (EsriShapefile.ShapeType.Point != shapeType)
                        message = Application.Current.FindString("ImportProfileInvalidSHPTypePoint");
                }
                else if (ImportType.Barriers == _importType)
                {
                    if ((EsriShapefile.ShapeType.PolyLine != shapeType) &&
                        (EsriShapefile.ShapeType.Polygon != shapeType) &&
                        (EsriShapefile.ShapeType.Point != shapeType))
                        message = Application.Current.FindString("ImportProfileInvalidSHPTypeBarrier");
                }
                else if (ImportType.Zones == _importType)
                {
                    if ((EsriShapefile.ShapeType.Polygon != shapeType) &&
                        (EsriShapefile.ShapeType.Point != shapeType))
                        message = Application.Current.FindString("ImportProfileInvalidSHPTypeZone");
                }
                else
                    message = App.Current.FindString("ImportProfileNotSupportedSHPType");
            }

            return message;
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Imported type.
        /// </summary>
        private static ImportType _importType = ImportType.Orders;

        #endregion // Private members
    }

    /// <summary>
    /// Class defines validation rule for import profile source link.
    /// </summary>
    internal class ProfileTableNameValidationRule : ValidationRule
    {
        /// <summary>
        /// Does validate profile table name.
        /// </summary>
        /// <param name="value">Profile table name to validation.</param>
        /// <param name="cultureInfo">Ignored.</param>
        /// <returns>Validation result.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ProfileNotEmptyValidationRule emptyValidation = new ProfileNotEmptyValidationRule();
            ValidationResult result = emptyValidation.Validate(value, cultureInfo);
            if (!result.IsValid)
                return result;

            result = ValidationResult.ValidResult;

            string message = null;
            IList<string> tabels = DataSourceOpener.GetTableNameList(out message);
            if (!tabels.Contains(value.ToString()))
            {
                message = App.Current.FindString("ImportProfileInvalidTableName");
                result = new ValidationResult(false, message);
            }

            return result;
        }
    }

    #endregion // Validation Rule classes
}
