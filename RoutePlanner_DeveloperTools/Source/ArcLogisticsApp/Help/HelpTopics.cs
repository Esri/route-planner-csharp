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
using System.Diagnostics;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Help
{
    internal enum LinkType
    {
        Chm,
        Html
    }

    /// <summary>
    /// Help topics class.
    /// </summary>
    internal class HelpTopics
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of HelpTopics class.
        /// </summary>
        public HelpTopics(LinkType type, string path, IDictionary<string, HelpTopic> topics)
        {
            _type = type;
            _path = path;
            _topics = topics;
        }

        #endregion // Constructors

        #region Public interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Help link type
        /// </summary>
        public LinkType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Help file path
        /// </summary>
        /// <remarks>Can be null</remarks>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Quick help string
        /// </summary>
        /// <remarks>Can be null</remarks>
        public HelpTopic GetTopic(string name)
        {
            HelpTopic topic = null;
            if ((null != _topics) && (_topics.ContainsKey(name)))
                topic = _topics[name];

            return topic;
        }

        #endregion // Public interface

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private LinkType _type = LinkType.Html;
        private string _path = string.Empty;
        private IDictionary<string, HelpTopic> _topics = null;

        #endregion // Private members
    }
}
