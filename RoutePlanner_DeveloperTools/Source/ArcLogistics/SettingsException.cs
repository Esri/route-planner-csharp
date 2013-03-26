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

