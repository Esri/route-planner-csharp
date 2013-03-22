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

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// UI Manager interface
    /// </summary>
    internal interface IUIManager
    {
        /// <summary>
        /// Lock MainWindow UI.
        /// </summary>
        /// <param name="lockPageFrame">Lock page frame flag.</param>
        void Lock(bool lockPageFrame);

        /// <summary>
        /// Unlock MainWindow UI.
        /// </summary>
        void Unlock();

        /// <summary>
        /// MainWindow UI is loked flag.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Lock message window UI.
        /// </summary>
        void LockMessageWindow();

        /// <summary>
        /// Unlock message window UI.
        /// </summary>
        void UnlockMessageWindow();

        /// <summary>
        /// Fires when MainWindow UI locked.
        /// </summary>
        event EventHandler Locked;

        /// <summary>
        /// Fires when MainWindow UI unlocked.
        /// </summary>
        event EventHandler UnLocked;
    }
}
