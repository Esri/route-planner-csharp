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
using ESRI.ArcLogistics.DomainObjects;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// ServerMessageType enumeration
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum ServerMessageType
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// ServerMessage class
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ServerMessage
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal ServerMessage(ServerMessageType type, string text)
        {
            _type = type;
            _text = text;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets message type.
        /// </summary>
        public ServerMessageType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets message text.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ServerMessageType _type;
        private string _text;

        #endregion private fields
    }

    /// <summary>
    /// SolveResult class
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SolveResult
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal SolveResult(ServerMessage[] messages, Violation[] violations,
            bool isFailed)
        {
            if (messages != null)
                _messages.AddRange(messages);

            if (violations != null)
                _violations.AddRange(violations);

            _isFailed = isFailed;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets server messages list.
        /// </summary>
        public IList<ServerMessage> Messages
        {
            get { return _messages.AsReadOnly(); }
        }

        /// <summary>
        /// Gets violations list.
        /// </summary>
        public IList<Violation> Violations
        {
            get { return _violations.AsReadOnly(); }
        }

        /// <summary>
        /// Gets a boolean value indicating whether VRP solve failed.
        /// </summary>
        public bool IsFailed
        {
            get { return _isFailed; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<ServerMessage> _messages = new List<ServerMessage>();
        private List<Violation> _violations = new List<Violation>();
        private bool _isFailed;

        #endregion private fields
    }

}