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

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Helper class simplifying creation of the
    /// <see cref="AsyncOperationCompletedEventArgs&lt;TResult&gt;"/> class objects.
    /// </summary>
    internal static class AsyncOperationCompletedEventArgs
    {
        /// <summary>
        /// Creates new instance of the
        /// <see cref="AsyncOperationCompletedEventArgs&lt;TResult&gt;"/> class.
        /// </summary>
        /// <typeparam name="TResult">The type of the asynchronous operation result.</typeparam>
        /// <param name="result">The result of the asynchronous operation.</param>
        /// <param name="error">The error occurred during asynchronous operation execution
        /// if any.</param>
        /// <param name="cancelled">The value indicating if the operation was cancelled.</param>
        /// <param name="userState">The user-provided operation state object.</param>
        /// <returns>A new instance of the
        /// <see cref="AsyncOperationCompletedEventArgs&lt;TResult&gt;"/> class.</returns>
        public static AsyncOperationCompletedEventArgs<TResult> Create<TResult>(
            TResult result,
            Exception error,
            bool cancelled,
            object userState)
        {
            return new AsyncOperationCompletedEventArgs<TResult>(
                result,
                error,
                cancelled,
                userState);
        }
    }

    /// <summary>
    /// Provides data for asynchronous operation completed event.
    /// </summary>
    /// <typeparam name="TResult">The type of the asynchronous operation result.</typeparam>
    internal sealed class AsyncOperationCompletedEventArgs<TResult> : AsyncCompletedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AsyncOperationCompletedEventArgs&lt;TResult&gt;"/> class.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        /// <param name="error">The error occurred during asynchronous operation execution
        /// if any.</param>
        /// <param name="cancelled">The value indicating if the operation was cancelled.</param>
        /// <param name="userState">The user-provided operation state object.</param>
        public AsyncOperationCompletedEventArgs(
            TResult result,
            Exception error,
            bool cancelled,
            object userState)
            : base(error, cancelled, userState)
        {
            this.Result = result;
        }

        /// <summary>
        /// Gets result of the asynchronous operation.
        /// </summary>
        public TResult Result
        {
            get;
            private set;
        }
    }
}
