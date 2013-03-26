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
