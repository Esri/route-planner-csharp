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
using System.IO;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// App.xaml.cs implementation of SaveExceptionHandler.
    /// If there was exceptions with DB or project config access - show message in Message Window.
    /// </summary>
    internal class AppSaveExceptionHandler : ESRI.ArcLogistics.IProjectSaveExceptionHandler
    {
        #region IProjectSaveExceptionHandler methods

        /// <summary>
        /// Exception handler.
        /// </summary>
        /// <param name="e">Exception.</param>
        /// <returns>Returns 'false' if this exeption must be thrown, 'true' otherwise</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="e"/>
        /// argument is a null reference.</exception>
        public bool HandleException(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            bool handled = false;

            // If we have problem with configuration file - show the message.
            if (e is IOException || e is UnauthorizedAccessException)
            {
                App.Current.Messenger
                    .AddError((string)App.Current.FindResource("CantWriteToProjectConfiguration"));
                handled = true;
            }
            // If we have problem with database file - show the message.
            if (e is System.Data.UpdateException || e is System.Data.EntityException)
            {
                App.Current.Messenger.AddError((string)App.Current.FindResource("CantWriteToDB"));
                handled = true;
            }

            Logger.Error(e);
            return handled;
        }

        #endregion
    }
}
