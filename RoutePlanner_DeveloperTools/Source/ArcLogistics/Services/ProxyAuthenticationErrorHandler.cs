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

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to proxy authentication error handling.
    /// </summary>
    internal static class ProxyAuthenticationErrorHandler
    {
        /// <summary>
        /// Handles proxy authentication error.
        /// </summary>
        /// <param name="exception">The exception to be handled.</param>
        /// <returns>true if and only if the error was handled successfully.</returns>
        public static bool HandleError(Exception exception)
        {
            var error = ServiceHelper.GetCommunicationError(exception);
            if (error != CommunicationError.ProxyAuthenticationRequired)
            {
                return false;
            }

            return _currentHandler();
        }

        /// <summary>
        /// Replaces current proxy authentication error handler with another one.
        /// </summary>
        /// <param name="newHandler">The reference to the new handler function.</param>
        /// <returns>A reference to the previous handler function.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="newHandler"/> is a null
        /// reference.</exception>
        public static Func<bool> SetupHandler(Func<bool> newHandler)
        {
            if (newHandler == null)
            {
                throw new ArgumentNullException("newHandler");
            }

            var oldHandler = _currentHandler;
            _currentHandler = newHandler;

            return oldHandler;
        }

        /// <summary>
        /// Stores current proxy error handler function.
        /// </summary>
        private static Func<bool> _currentHandler = () => false;
    }
}
