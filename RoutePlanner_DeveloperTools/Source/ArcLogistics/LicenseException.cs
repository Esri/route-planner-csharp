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