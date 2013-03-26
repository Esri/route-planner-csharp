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
using System.Windows.Input;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Provides helper facilities for the <see cref="System.Windows.Input.Mouse"/>
    /// class.
    /// </summary>
    internal static class MouseHelper
    {
        /// <summary>
        /// Temporarily overrides mouse cursor for the entire application.
        /// </summary>
        /// <param name="newCursor">The new cursor to be used.</param>
        /// <returns>The reference to the <see cref="System.IDisposable"/> object
        /// restoring current mouse cursor upon call to the
        /// <see cref="M:System.IDisposable.Dispose"/> method.</returns>
        public static IDisposable OverrideCursor(Cursor newCursor)
        {
            var currentCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = newCursor;

            return new DelegateDisposable(() => Mouse.OverrideCursor = currentCursor);
        }
    }
}
