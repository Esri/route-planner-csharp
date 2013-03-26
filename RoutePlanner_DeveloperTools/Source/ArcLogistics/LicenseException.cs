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

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// License-related error codes.
    /// </summary>
    public enum LicenseError
    {
        InvalidCredentials,
        NoAllowedRoles,
        MaxRoutesPermission,
        MaxOrdersPermission,
        InvalidLicenseData,
        ServiceFault,
        LicenseComponentNotFound,
        InvalidComponentSignature,
        LicenseNotActivated,

        /// <summary>
        /// The license is expired already.
        /// </summary>
        LicenseExpired,
    }

    /// <summary>
    /// LicenseException class
    /// </summary>
    public class LicenseException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public LicenseException(LicenseError code)
        {
            _code = code;
        }

        public LicenseException(LicenseError code, string message)
            : base(message)
        {
            _code = code;
        }

        public LicenseException(LicenseError code, string message, Exception inner)
            : base(message, inner)
        {
            _code = code;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public LicenseError ErrorCode
        {
            get { return _code; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private LicenseError _code;

        #endregion private fields
    }

}