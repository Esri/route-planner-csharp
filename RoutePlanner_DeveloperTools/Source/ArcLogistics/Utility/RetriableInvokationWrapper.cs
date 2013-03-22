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

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides facilities for retrying invocations upon failure.
    /// </summary>
    internal class RetriableInvocationWrapper : IInvocationWrapper
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the RetriableInvokationWrapper class
        /// with the specified maximum number of retries and retry preparation
        /// delegate.
        /// </summary>
        /// <param name="maxRetryCount">The maximum number of retries to be
        /// attempted.</param>
        /// <param name="prepareForRetry">Function for preparing for the next
        /// retry. Should return true if and only if the new retry could be
        /// attempted.</param>
        public RetriableInvocationWrapper(
            int maxRetryCount,
            Func<Exception, bool> prepareForRetry)
            : this(maxRetryCount, prepareForRetry, (e) => null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RetriableInvokationWrapper class
        /// with the specified maximum number of retries, retry preparation
        /// delegate and exception translation delegate.
        /// </summary>
        /// <param name="maxRetryCount">The maximum number of retries to be
        /// attempted.</param>
        /// <param name="prepareForRetry">Function for preparing for the next
        /// retry. Should return true if and only if the new retry could be
        /// attempted.</param>
        /// <param name="translateError">Translates exception into another one.
        /// Should return null if the exception could not be translated.</param>
        public RetriableInvocationWrapper(
            int maxRetryCount,
            Func<Exception, bool> prepareForRetry,
            Func<Exception, Exception> translateError)
        {
            Debug.Assert(maxRetryCount >= 0);
            Debug.Assert(prepareForRetry != null);
            Debug.Assert(translateError != null);

            _maxRetryCount = maxRetryCount;
            _prepareForRetry = prepareForRetry;
            _translateException = translateError;
        }
        #endregion

        #region IInvokationWrapper Members
        /// <summary>
        /// Invokes the specified invokationTarget retrying it if necessary.
        /// </summary>
        /// <typeparam name="TResult">The type of the invocation result.</typeparam>
        /// <param name="invocationTarget">The function to be invoked.</param>
        /// <returns>Result of the invocation.</returns>
        public TResult Invoke<TResult>(Func<TResult> invocationTarget)
        {
            var retryCount = 0;
            var result = default(TResult);
            while (true)
            {
                try
                {
                    result = invocationTarget();
                    break;
                }
                catch (Exception e)
                {
                    if (retryCount < _maxRetryCount && _prepareForRetry(e))
                    {
                        ++retryCount;

                        continue;
                    }

                    var newException = _translateException(e);
                    if (newException != null)
                    {
                        throw newException;
                    }

                    throw;
                }
            }

            return result;
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores the maximum number of retries to be attempted.
        /// </summary>
        private int _maxRetryCount;

        /// <summary>
        /// The retrying preparation delegate.
        /// </summary>
        private Func<Exception, bool> _prepareForRetry;

        /// <summary>
        /// The exception translation delegate.
        /// </summary>
        private Func<Exception, Exception> _translateException;
        #endregion
    }
}
