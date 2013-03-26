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

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Database specific error codes.
    /// </summary>
    public enum DataError
    {
        Internal,
        FileSharingViolation,
        ObjectRemovalRestricted,
        NotSupportedFileVersion,
        DatabaseUpdateError
    }

    /// <summary>
    /// The exception that is thrown when database error has occured. 
    /// </summary>
    public class DataException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>DataException</c> class.
        /// </summary>
        public DataException()
            : base("Data operation failed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DataException</c> class.
        /// </summary>
        public DataException(DataError code)
            : base("Data operation failed.")
        {
            _code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <c>DataException</c> class.
        /// </summary>
        public DataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DataException</c> class.
        /// </summary>
        public DataException(string message, DataError code)
            : base(message)
        {
            _code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <c>DataException</c> class.
        /// </summary>
        public DataException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DataException</c> class.
        /// </summary>
        public DataException(string message, Exception inner, DataError code)
            : base(message, inner)
        {
            _code = code;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets specific code of the database error.
        /// </summary>
        public DataError ErrorCode
        {
            get { return _code; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataError _code = DataError.Internal;

        #endregion private fields
    }

}