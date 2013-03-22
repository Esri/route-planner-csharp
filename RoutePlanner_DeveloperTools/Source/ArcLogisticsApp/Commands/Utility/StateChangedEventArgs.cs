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
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Commands.Utility
{
    /// <summary>
    /// Provides data for the <see cref="IStateTrackingService.StateChanged"/> event.
    /// </summary>
    internal sealed class StateChangedEventArgs : EventArgs
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the StateChangedEventArgs class.
        /// </summary>
        /// <param name="isEnabled">The value of the
        /// <see cref="IStateTrackingService.IsEnabled"/> property of the
        /// object which fired an event.</param>
        public StateChangedEventArgs(bool isEnabled)
        {
            this.IsEnabled = isEnabled;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a value indicating whether the tracked component is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get;
            private set;
        }
        #endregion
    }
}
