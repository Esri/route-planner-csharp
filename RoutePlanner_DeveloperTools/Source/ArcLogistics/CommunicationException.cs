using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Communication error codes.
    /// </summary>
    public enum CommunicationError
    {
        /// <summary>
        /// Communication error of unknown nature.
        /// </summary>
        Unknown,

        /// <summary>
        /// Communication error due to proxy-server requiring authentication.
        /// </summary>
        ProxyAuthenticationRequired,

        /// <summary>
        /// Communication error due to transient failure.
        /// </summary>
        ServiceTemporaryUnavailable,

        /// <summary>
        /// Communication error due to inability to get a response from service
        /// during time-out period.
        /// </summary>
        ServiceResponseTimeout,
    }

    /// <summary>
    /// CommunicationException class
    /// </summary>
    public class CommunicationException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public CommunicationException()
        {
        }

        public CommunicationException(string message)
            : base(message)
        {
        }

        public CommunicationException(string message, string serviceName)
            : base(message)
        {
            _serviceName = serviceName;
        }

        public CommunicationException(string message, string serviceName,
            CommunicationError code)
            : base(message)
        {
            _serviceName = serviceName;
            _code = code;
        }

        public CommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public CommunicationException(string message, CommunicationError code,
            Exception inner)
            : base(message, inner)
        {
            _code = code;
        }

        public CommunicationException(string message, string serviceName, Exception inner)
            : base(message, inner)
        {
            _serviceName = serviceName;
        }

        public CommunicationException(string message, string serviceName,
            CommunicationError code,
            Exception inner)
            : base(message, inner)
        {
            _serviceName = serviceName;
            _code = code;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string ServiceName
        {
            get { return _serviceName; }
        }

        public CommunicationError ErrorCode
        {
            get { return _code; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _serviceName;
        private CommunicationError _code = CommunicationError.Unknown;

        #endregion private fields
    }

}