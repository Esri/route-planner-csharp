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

namespace ESRI.ArcLogistics.DomainObjects.Attributes
{
    /// <summary>
    /// This attribute is applied to any property that has physical quantity semantics.  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class UnitPropertyAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public UnitPropertyAttribute(Unit units, Unit displayUnitUS, Unit displayUnitMetric)
        {
            _valueUnits = units;
            _displayUnitUS = displayUnitUS;
            _displayUnitMetric = displayUnitMetric;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Target property is returned in units specified by this property value.
        /// </summary>
        public Unit ValueUnits
        {
            get { return _valueUnits; }
        }

        /// <summary>
        /// Target property should be displayed in UI using units specified by this property value (for US locale).
        /// </summary>
        public Unit DisplayUnitUS
        {
            get { return _displayUnitUS; }
        }

        /// <summary>
        /// Target property should be displayed in UI using units specified by this property value (for Metric locale).
        /// </summary>
        public Unit DisplayUnitMetric
        {
            get { return _displayUnitMetric; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Unit _valueUnits = Unit.Unknown;
        private Unit _displayUnitUS = Unit.Unknown;
        private Unit _displayUnitMetric = Unit.Unknown;

        #endregion // Private members
    }
}
