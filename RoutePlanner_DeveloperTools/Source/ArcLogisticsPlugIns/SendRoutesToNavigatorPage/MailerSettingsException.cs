using System;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// MailerSettingsException class
    /// </summary>
    internal class MailerSettingsException : Exception
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MailerSettingsException(string message)
            : base(message)
        {
        }

        public MailerSettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion constructors
    }
}

