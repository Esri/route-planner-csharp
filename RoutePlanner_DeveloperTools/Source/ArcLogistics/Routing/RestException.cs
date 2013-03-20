using System;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// RestException class
    /// </summary>
    internal class RestException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RestException()
        {
        }

        public RestException(string message)
            : base(message)
        {
        }

        public RestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RestException(string message, int code, string[] details)
            : base(message)
        {
            _details = details;
            _code = code;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string[] Details
        {
            get { return _details; }
        }

        public int ErrorCode
        {
            get { return _code; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string[] _details;
        private int _code;

        #endregion private fields
    }

}