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