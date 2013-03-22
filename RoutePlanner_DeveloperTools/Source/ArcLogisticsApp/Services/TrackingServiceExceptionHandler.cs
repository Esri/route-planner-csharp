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

namespace ESRI.ArcLogistics.App.Services
{
    /// <summary>
    /// Exception handler for communication exceptions occurred during interactions with the
    /// tracking service.
    /// </summary>
    internal sealed class TrackingServiceExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Handles communication related exception assuming it was generated during interaction
        /// with tracking service.
        /// </summary>
        /// <param name="exceptionToHandle">The exception to be handled.</param>
        /// <returns>True if and only if the exception was handled and need not
        /// be rethrown.</returns>
        public bool HandleException(Exception exceptionToHandle)
        {
            Debug.Assert(exceptionToHandle != null);

            Logger.Error(exceptionToHandle);

            var isTrackingError =
                exceptionToHandle is AuthenticationException ||
                exceptionToHandle is CommunicationException;

            if (!isTrackingError)
            {
                return false;
            }

            CommonHelpers.AddTrackingErrorMessage(exceptionToHandle);

            return true;
        }
    }
}
