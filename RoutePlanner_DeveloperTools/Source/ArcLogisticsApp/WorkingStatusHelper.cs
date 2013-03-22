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
using ESRI.ArcLogistics.App.Controls;
using System.Diagnostics;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class for setting the mouse cursor and status bar to a a "busy" status during long operations.
    /// </summary>
    internal static class WorkingStatusHelper
    {
        #region Public Methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets application state to Busy.
        /// </summary>
        public static void SetBusy(string status)
        {
            if (string.IsNullOrEmpty(status))
                status = (string)App.Current.FindResource("DefaultBusyStatus");

            App.Current.MainWindow.StatusBar.WorkingStatus = status;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        /// <summary>
        /// Sets application state to Free.
        /// </summary>
        public static void SetReleased()
        {
            App.Current.MainWindow.StatusBar.WorkingStatus = null;
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Sets application busy state with the specified status.
        /// </summary>
        /// <param name="status">A string describing the reason for the busy state.</param>
        /// <returns><see cref="System.IDisposable"/> instance which should
        /// be disposed upon exiting from the busy state.</returns>
        /// <remarks>This method is intended to be used with using statement. For example:
        /// <example><![CDATA[
        /// using (WorkingStatusHelper.EnterBusyState("The application is currently Busy."))
        /// {
        ///     ...
        /// }
        /// ]]>
        /// </example>
        /// The application will leave busy state when the returned object has been disposed.
        /// </remarks>
        public static IDisposable EnterBusyState(string status)
        {
            var disposable = new DelegateDisposable(WorkingStatusHelper.SetReleased);

            WorkingStatusHelper.SetBusy(status);

            return disposable;
        }

        #endregion
    }
}
