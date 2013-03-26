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
using System.Text;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// MessageWindowTextDataWrapper struct
    /// </summary>
    internal struct MessageWindowTextDataWrapper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageWindowTextDataWrapper(string message, Link link)
        {
            _link = link;
            _message = message;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string Message
        {
            get { return _message; }
        }

        public Link Link
        {
            get { return _link; }
        }

        #endregion // Public methods

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            string text = this.Message;
            if ((null != this.Link) && !string.IsNullOrEmpty(this.Link.Text))
            {
                text = (this.Message.Contains("{0}"))?
                            string.Format(this.Message, this.Link.Text) :
                            string.Format("{0} {1}", this.Message, this.Link.Text);
            }

            return text.Trim();
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Link _link;
        private string _message;

        #endregion // Private members
    }
}
