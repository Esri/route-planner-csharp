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

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents geocoding exception.
    /// </summary>
    public class GeocodeException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>GeocodeException</c> class.
        /// </summary>
        public GeocodeException()
            : base(Properties.Resources.GeocodeOperationFailed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>GeocodeException</c> class.
        /// </summary>
        /// <param name="message">Exception description.</param>
        public GeocodeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>GeocodeException</c> class.
        /// </summary>
        /// <param name="message">Exception description.</param>
        /// <param name="inner">Inner exception.</param>
        public GeocodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion constructors
    }

}