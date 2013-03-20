using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// MessageWindowDataWrapper struct
    /// </summary>
    internal struct MessageWindowDataWrapper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageWindowDataWrapper(MessageType type, string time, MessageWindowTextDataWrapper message,
                                        IEnumerable<MessageDetailDataWrap> details)
        {
            _type = type;
            _time = time;
            _message = message;
            _details = (null == details) ? new List<MessageDetailDataWrap>() : details;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageType Type
        {
            get { return _type; }
        }

        public string Time
        {
            get { return _time; }
        }

        public MessageWindowTextDataWrapper Message
        {
            get { return _message; }
        }

        public IEnumerable<MessageDetailDataWrap> Details
        {
            get { return _details; }
        }

        #endregion // Public methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MessageType _type;
        private string _time;
        private MessageWindowTextDataWrapper _message;
        private IEnumerable<MessageDetailDataWrap> _details;

        #endregion // Private members
    }
}
