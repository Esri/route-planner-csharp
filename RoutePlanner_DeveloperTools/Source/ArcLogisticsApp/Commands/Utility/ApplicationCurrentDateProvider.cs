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

namespace ESRI.ArcLogistics.App.Commands.Utility
{
    /// <summary>
    /// Implements <see cref="ICurrentDateProvider"/> using <see cref="App"/>
    /// instance as a source for current date values.
    /// </summary>
    internal sealed class ApplicationCurrentDateProvider : ICurrentDateProvider
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ApplicationCurrentDateProvider class
        /// with the specified reference to the application object.
        /// </summary>
        /// <param name="application">Reference to the application object to
        /// take current date values from.</param>
        public ApplicationCurrentDateProvider(App application)
        {
            Debug.Assert(application != null);

            _application = application;
            _application.CurrentDateChanged += (s, e) => _NotifyCurrentDateChanged();
        }
        #endregion

        #region ICurrentDateProvider Members
        /// <summary>
        /// Fired when current date is changed.
        /// </summary>
        public event EventHandler CurrentDateChanged;

        /// <summary>
        /// Gets current date/time value.
        /// </summary>
        public DateTime CurrentDate
        {
            get
            {
                return _application.CurrentDate;
            }
            private set
            {
                _application.CurrentDate = value;
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Notifies about current date changes.
        /// </summary>
        private void _NotifyCurrentDateChanged()
        {
            var temp = this.CurrentDateChanged;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// Reference to the application object to take current date values from.
        /// </summary>
        private App _application;
        #endregion
    }
}
