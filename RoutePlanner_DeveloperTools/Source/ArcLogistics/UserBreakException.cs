using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// UserBreakException class
    /// </summary>
    public class UserBreakException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public UserBreakException()
            : base(Properties.Messages.Error_UserCancel)
        {
        }

        public UserBreakException(string message)
            : base(message)
        {
        }

        public UserBreakException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion
    }

}