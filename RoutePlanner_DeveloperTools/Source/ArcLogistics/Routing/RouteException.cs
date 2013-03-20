using System;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// RouteException class
    /// </summary>
    internal class RouteException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RouteException()
            : base("Routing operation failed.")
        {
        }

        public RouteException(string message)
            : base(message)
        {
        }

        public RouteException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RouteException(string message, DataObject[] invalidObjects)
            : base(message)
        {
            _invalidObjects = invalidObjects;
        }

        public RouteException(string message, string[] details, int code)
            : base(message)
        {
            _details = details;
            _code = code;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public DataObject[] InvalidObjects
        {
            get { return _invalidObjects; }
        }

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

        private DataObject[] _invalidObjects;
        private string[] _details;
        private int _code;

        #endregion private fields
    }

}