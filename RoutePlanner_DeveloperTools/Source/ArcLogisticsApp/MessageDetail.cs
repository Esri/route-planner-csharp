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
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.GridHelpers;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// MessageDetail class
    /// </summary>
    public class MessageDetail
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageDetail(MessageType type, string text)
        {
            _Initialize(type, text, null);
        }

        public MessageDetail(MessageType type, string text, string helpLink)
        {
            _Initialize(type, text, _InitializeHelpLink(helpLink));
        }

        public MessageDetail(MessageType type, string format, params DataObject[] args)
        {
            _Initialize(type, null, format, args);
        }

        public MessageDetail(MessageType type, string format, string helpLink, params DataObject[] args)
        {
            _Initialize(type, _InitializeHelpLink(helpLink), format, args);
        }

        public MessageDetail(MessageType type, string text, Link link)
        {
            _Initialize(type, text, link);
        }

        internal MessageDetail(MessageType type, string format, Link link, params DataObject[] args)
        {
            _Initialize(type, link, format, args);
        }
        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Priority type of message
        /// </summary>
        public MessageType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Fromat message text
        /// </summary>
        /// <remarks>Return only if IsConstructedMessage is true</remarks>
        public string Format
        {
            get { return _description.Format; }
        }

        /// <summary>
        /// Message text
        /// </summary>
        /// <remarks>Return only if IsConstructedMessage is false</remarks>
        public string Text
        {
            get { return _description.Text; }
        }

        /// <summary>
        /// Description
        /// </summary>
        internal MessageDescription Description
        {
            get { return _description; }
        }

        #endregion // Public methods

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Link _InitializeHelpLink(string helpLink)
        {
            Link link = null;
            if (!string.IsNullOrEmpty(helpLink))
            {
                string linkCaption = (string)App.Current.FindResource("MessageDetailsHelpLinkCaption");
                link = new Link(linkCaption, helpLink, LinkType.Url);
            }

            return link;
        }

        private void _Initialize(MessageType type, string text, Link link)
        {
            Debug.Assert(!string.IsNullOrEmpty(text));

            _type = type;
            _description = new MessageDescription(text, link, null);
        }

        private void _Initialize(MessageType type, Link link, string format, params DataObject[] args)
        {
            Debug.Assert(!string.IsNullOrEmpty(format));

            List <MessageObjectContext> objects = null;
            if (null != args)
            {
                if (0 < args.Length)
                {
                    objects = new List<MessageObjectContext>(args.Length);
                    for (int i = 0; i < args.Length; ++i)
                        objects.Add(new MessageObjectContext(args[i]));
                }
            }

            _type = type;
            _description = new MessageDescription(format, link, objects);
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MessageType _type;
        private MessageDescription _description;

        #endregion // Private members
    }
}
