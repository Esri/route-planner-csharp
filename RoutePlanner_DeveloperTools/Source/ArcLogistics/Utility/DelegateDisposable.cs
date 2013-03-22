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

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Implements <see cref="System.IDisposable"/> by running the provided delegate.
    /// </summary>
    internal class DelegateDisposable : IDisposable
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DelegateDisposable class with
        /// the specified action.
        /// </summary>
        /// <param name="disposeAction">The action to be performed upon
        /// disposal.</param>
        public DelegateDisposable(Action disposeAction)
        {
            if (disposeAction == null)
            {
                throw new ArgumentNullException("disposeAction");
            }

            _disposeAction = disposeAction;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Implements disposing by calling associated action.
        /// </summary>
        public void Dispose()
        {
            if (_disposeAction != null)
            {
                _disposeAction();
                _disposeAction = null;
            }
        }
        #endregion

        #region private fields
        /// <summary>
        /// The action to be performed upon disposal.
        /// </summary>
        private Action _disposeAction;
        #endregion
    }
}
