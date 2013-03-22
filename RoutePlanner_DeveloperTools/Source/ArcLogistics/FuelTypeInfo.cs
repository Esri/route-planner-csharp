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
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// FuelTypeInfo class contains information about a fuel type used in the application.
    /// </summary>
    /// <remarks>Read only class</remarks>
    internal class FuelTypeInfo : ICloneable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new instance of the <c>FuelTypeInfo</c> class.
        /// </summary>
        public FuelTypeInfo(string name, double price, double Co2Emission)
        {
            _name = name;
            _price = price;
            _Co2Emission = Co2Emission;
        }
        #endregion // Constructors

        #region Public members

        /// <summary>
        /// Name of the fuel type.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Price of the fuel type.
        /// </summary>
        public double Price
        {
            get { return _price; }
        }

        /// <summary>
        /// CO2 emission of the fuel type.
        /// </summary>
        public double Co2Emission
        {
            get { return _Co2Emission; }
        }
        #endregion // Public members

        #region ICloneable members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            return new FuelTypeInfo(this.Name, this.Price, this.Co2Emission);
        }
        #endregion // ICloneable members

        #region Private members

        private readonly string _name;
        private readonly double _price;
        private readonly double _Co2Emission;

        #endregion // Private members
    }
}
