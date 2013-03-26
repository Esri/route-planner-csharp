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
    /// Implements <see cref="T:ESRI.ArcLogistics.App.IWorkingStatusController"/>
    /// interface by changing application status with
    /// <see cref="T:ESRI.ArcLogistics.App.WorkingStatusHelper"/> class.
    /// </summary>
    internal sealed class ApplicationWorkingStatusController : IWorkingStatusController
    {
        #region IWorkingStatusController Members
        /// <summary>
        /// Sets application busy state with the specified status.
        /// </summary>
        /// <param name="status">The string describing busy state reason.</param>
        /// <returns><see cref="System.IDisposable"/> instance which should
        /// be disposed upon exiting from the busy state.</returns>
        /// <remarks><see cref="M:WorkingStatusHelper.EnterBusyState"/> method
        /// description for more details.
        /// </remarks>
        public IDisposable EnterBusyState(string status)
        {
            return WorkingStatusHelper.EnterBusyState(status);
        }
        #endregion
    }

}
