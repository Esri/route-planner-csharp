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
using System.Linq;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Represents a collection of disposable objects to be disposed together.
    /// </summary>
    internal sealed class CompositeDisposable : IDisposable
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the CompositeDisposable class.
        /// </summary>
        /// <param name="disposables">The collection of disposable objects to
        /// be disposed together.</param>
        public CompositeDisposable(params IDisposable[] disposables)
        {
            if (disposables == null)
            {
                throw new ArgumentNullException("disposables");
            }

            if (disposables.Any(disposable => disposable == null))
            {
                throw new ArgumentOutOfRangeException("disposables");
            }

            _disposables = disposables.ToList();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes all contained objects.
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }
        #endregion

        #region private fields
        /// <summary>
        /// The collection of all disposable objects.
        /// </summary>
        private List<IDisposable> _disposables = new List<IDisposable>();
        #endregion
    }
}
