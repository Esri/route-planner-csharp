using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// SettingsException class
    /// </summary>
    public class SettingsException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SettingsException()
            : base(Properties.Messages.Error_InvalidSettings)
        {
        }

        public SettingsException(string message)
            : base(message)
        {
        }

        public SettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The original exception.</param>
        /// <param name="source">The string which describe the source of exception.</param>
        public SettingsException(string message, Exception inner, string source)
            : base(message, inner)
        {
            Source = source;
        }

        #endregion constructors
    }
}

