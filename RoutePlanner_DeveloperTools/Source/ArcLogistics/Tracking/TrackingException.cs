using System;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// TrackingException class
    /// </summary>
    internal class TrackingException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public TrackingException()
        {
        }

        public TrackingException(string message)
            : base(message)
        {
        }

        public TrackingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion constructors
    }

}