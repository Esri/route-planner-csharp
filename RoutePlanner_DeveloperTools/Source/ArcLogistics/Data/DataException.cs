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