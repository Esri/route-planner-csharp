using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// AuthenticationException class
    /// </summary>
    public class AuthenticationException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AuthenticationException()
        {
        }

        public AuthenticationException(string message)
            : base(message)
        {
        }

        public AuthenticationException(string message, string serviceName)
            : base(message)
        {
            _serviceName = serviceName;
        }

        public AuthenticationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AuthenticationException(string message, string serviceName, Exception inner)
            : base(message, inner)
        {
            _serviceName = serviceName;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string ServiceName
        {
            get { return _serviceName; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _serviceName;

        #endregion private fields
    }

}