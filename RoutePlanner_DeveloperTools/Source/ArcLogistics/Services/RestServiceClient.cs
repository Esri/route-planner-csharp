using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Encapsulates interaction with REST services.
    /// </summary>
    /// <typeparam name="TService">The type of the REST service to communicate with.</typeparam>
    internal sealed class RestServiceClient<TService>
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RestServiceClient&lt;TService&gt;"/> class.
        /// </summary>
        /// <param name="channelFactory">The reference to the channel factory instance
        /// to be used for creating communication channels for the REST service.</param>
        /// <param name="serviceTitle">The title of the REST service.</param>
        /// <param name="exceptionHandler">Exception handler.</param>
        /// <exception cref="ArgumentNullException"><paramref name="channelFactory"/> or
        /// <paramref name="serviceTitle"/> is a null reference.</exception>
        public RestServiceClient(
            ChannelFactory<TService> channelFactory,
            string serviceTitle,
            IServiceExceptionHandler exceptionHandler)
        {
            CodeContract.RequiresNotNull("channelFactory", channelFactory);
            CodeContract.RequiresNotNull("serviceTitle", serviceTitle);
            CodeContract.RequiresNotNull("exceptionHandler", exceptionHandler);

            _connectionPool = new WcfClientConnectionPool<TService>(channelFactory);
            _serviceTitle = serviceTitle;
            _exceptionHandler = exceptionHandler;
        }
        #endregion

        #region public methods

        /// <summary>
        /// Cancels asynchronous operation associated with the specified object.
        /// </summary>
        /// <param name="userState">The user state object to cancel asynchronous operation
        /// for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="userState"/> is a null
        /// reference.</exception>
        public void CancelAsync(object userState)
        {
            CodeContract.RequiresNotNull("userState", userState);

            // Remove operation associated with the user state from a collection of running
            // asynchronous operations.
            var asyncState = default(AsyncState);
            lock (_operationsGuard)
            {
                if (!_operations.TryGetValue(userState, out asyncState))
                {
                    return;
                }

                _operations.Remove(userState);
            }

            // Request cancelling operation execution.
            asyncState.CancellationTokenSource.Cancel();

            // Notify clients that operation was canceled.
            asyncState.CancellationNotifier();
        }

        /// <summary>
        /// Starts asynchronous operation execution.
        /// </summary>
        /// <param name="method">The method for starting asynchronous operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is a null
        /// reference.</exception>
        public void InvokeAsync<TResult>(
            Func<TService, TResult> method,
            EventHandler<AsyncOperationCompletedEventArgs<TResult>> operationCompletedNotifier,
            object userState)
            where TResult : IFaultInfo
        {
            CodeContract.RequiresNotNull("method", method);
            CodeContract.RequiresNotNull("operationCompletedNotifier", operationCompletedNotifier);
            CodeContract.RequiresNotNull("userState", userState);

            // Prepare asynchronous operation state object.
            var asyncState = new AsyncState
            {
                UserState = userState,

                CancellationNotifier = () =>
                {
                    var eventArguments = AsyncOperationCompletedEventArgs.Create(
                        default(TResult),
                        null,
                        true,
                        userState);
                    if (operationCompletedNotifier != null)
                    {
                        operationCompletedNotifier(this, eventArguments);
                    }
                },

                CancellationTokenSource = new CancellationTokenSource(),
            };

            // Make asynchronous operation cancellable.
            var cancellationToken = asyncState.CancellationTokenSource.Token;

            // Register operation.
            lock (_operationsGuard)
            {
                if (userState != null)
                {
                    _operations.Add(userState, asyncState);
                }
            }

            // Finally invoke the operation.
            Task.Factory
                .StartNew(
                    () => _Invoke(method),
                    cancellationToken)
                .ContinueWith(
                    task => _HandleOperationCompletion(task, operationCompletedNotifier, asyncState));
        }

        /// <summary>
        /// Invokes service method that returns a value of type T.
        /// </summary>
        /// <typeparam name="TResult">The type of the operation result.</typeparam>
        /// <param name="method">The method to be invoked.</param>
        /// <returns>Result of the specified method invocation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is a null
        /// reference.</exception>
        /// <exception cref="CommunicationException">Failed to communicate with the REST
        /// service.</exception>
        public TResult Invoke<TResult>(Func<TService, TResult> method)
            where TResult : IFaultInfo
        {
            CodeContract.RequiresNotNull("method", method);

            var response = _Invoke(method);

            var error = _ValidateResponse(response);
            if (error != null)
            {
                throw error;
            }

            return response;
        }
        #endregion

        #region private classes
        /// <summary>
        /// Represents state of a single asynchronous operation.
        /// </summary>
        private sealed class AsyncState
        {
            /// <summary>
            /// Gets or sets the reference to the user-provided operation state.
            /// </summary>
            public object UserState
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the source of cancellation tokens for the asynchronous operation.
            /// </summary>
            public CancellationTokenSource CancellationTokenSource
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a callback to be used for operation cancellation.
            /// </summary>
            public Action CancellationNotifier
            {
                get;
                set;
            }
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Checks if the specified exception denotes a transient error so
        /// request sending could be retried.
        /// </summary>
        /// <param name="exception">The reference to the exception object
        /// to be checked.</param>
        /// <returns>True if and only if the exception was considered transient.</returns>
        private static bool _IsTransientError(Exception exception)
        {
            var webException = exception as WebException;
            if (webException != null)
            {
                if (!WebHelper.IsTransientError(webException))
                {
                    return false;
                }

                return true;
            }

            // Sometimes a transient connection error might result in corrupted (non-deserializable)
            // data, so we attempt retrying operation in such cases.
            var serializationException = exception as SerializationException;
            if (serializationException != null)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Attempts to prepare for retrying request sending upon catching the
        /// specified exception.
        /// </summary>
        /// <param name="exception">The exception caused failure of the previous
        /// request sending.</param>
        /// <returns>True if and only if the request sending could be repeated.</returns>
        private bool _PrepareRetry(Exception exception)
        {
            var restException = exception as RestException;
            if (restException != null)
            {
                return false;
            }

            if (ProxyAuthenticationErrorHandler.HandleError(exception))
            {
                return true;
            }

            if (_IsTransientError(exception))
            {
                Thread.Sleep(RETRY_WAIT_TIME);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Translates the specified exception into the more application-specific
        /// one.
        /// </summary>
        /// <param name="exception">The exception to be translated.</param>
        /// <returns>Translated exception or null reference if the exception
        /// cannot be translated.</returns>
        private Exception _TranslateException(Exception exception)
        {
            if (ServiceHelper.IsCommunicationError(exception) ||
                exception is SerializationException)
            {
                return ServiceHelper.CreateCommException(_serviceTitle, exception);
            }

            return null;
        }

        /// <summary>
        /// Validates the specified REST service response.
        /// </summary>
        /// <param name="faultInfo">The REST service response to be validated.</param>
        /// <returns>An instance of the <see cref="CommunicationException"/> class representing
        /// REST service failure or null reference if there were no errors.</returns>
        private CommunicationException _ValidateResponse(IFaultInfo faultInfo)
        {
            if (faultInfo == null || !faultInfo.IsFault)
            {
                return null;
            }

            var error = RestHelper.CreateRestException(faultInfo);
            Logger.Warning(error);

            return new CommunicationException(
                error.Message,
                _serviceTitle,
                CommunicationError.Unknown,
                error);
        }

        /// <summary>
        /// Handles completion of the asynchronous operation.
        /// </summary>
        /// <typeparam name="TResult">The type of the operation result.</typeparam>
        /// <param name="task">The task which executed the operation.</param>
        /// <param name="operationCompletedNotifier">The event to be used for
        /// notifying about operation completion.</param>
        /// <param name="asyncState">The information about completed asynchronous operation.</param>
        private void _HandleOperationCompletion<TResult>(
            Task<TResult> task,
            EventHandler<AsyncOperationCompletedEventArgs<TResult>> operationCompletedNotifier,
            AsyncState asyncState)
            where TResult : IFaultInfo
        {
            Debug.Assert(task != null);
            Debug.Assert(operationCompletedNotifier != null);
            Debug.Assert(asyncState != null);

            // Check if the task was canceled.
            var canceled = task.IsCanceled;

            lock (_operationsGuard)
            {
                canceled |= !_operations.Remove(asyncState.UserState);
            }

            asyncState.CancellationTokenSource.Dispose();

            if (canceled)
            {
                return;
            }

            // Retrieve task exception if any.
            var error = default(Exception);
            if (task.IsFaulted)
            {
                error = task.Exception.InnerExceptions.Single();
            }
            else
            {
                error = _ValidateResponse(task.Result);
            }

            // Notify about operation completion.
            error = _TranslateException(error) ?? error;

            var eventArguments = AsyncOperationCompletedEventArgs.Create(
                task.Result,
                error,
                false, // Operation was not cancelled.
                asyncState.UserState);
            operationCompletedNotifier(this, eventArguments);
        }

        /// <summary>
        /// Invokes the specified service method.
        /// </summary>
        /// <param name="method">The method to be invoked.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is a null
        /// reference.</exception>
        private TResult _Invoke<TResult>(Func<TService, TResult> method)
        {
            CodeContract.RequiresNotNull("method", method);

            var invocationWrapper = new RetriableInvocationWrapper(
                MAX_RETRY_COUNT,
                _PrepareRetry,
                _TranslateException);

            var result = default(TResult);
            invocationWrapper.Invoke(() =>
            {
                try
                {
                    using (var connection = _connectionPool.AcquireConnection())
                    {
                        result = method(connection.Client);
                    }
                }
                catch (Exception ex)
                {
                    if (!_exceptionHandler.HandleException(ex, _serviceTitle))
                    {
                        throw;
                    }
                }
            });

            return result;
        }
        #endregion

        #region private constants
        // Specifies the maximum number of request sending retries to be attempted.
        private const int MAX_RETRY_COUNT = 4;

        // Time in milliseconds to wait before retrying request upon connection
        // failure.
        private const int RETRY_WAIT_TIME = 500;
        #endregion

        #region private fields
        /// <summary>
        /// The pool of REST service connections.
        /// </summary>
        private WcfClientConnectionPool<TService> _connectionPool;

        /// <summary>
        /// The object to be used for serializing access to the _operations field.
        /// </summary>
        private object _operationsGuard = new object();

        /// <summary>
        /// Maps userState instances to corresponding asynchronous operation states.
        /// </summary>
        private Dictionary<object, AsyncState> _operations = new Dictionary<object, AsyncState>();

        /// <summary>
        /// The title of the REST service.
        /// </summary>
        private string _serviceTitle;

        /// <summary>
        /// Exceptions handler.
        /// </summary>
        private IServiceExceptionHandler _exceptionHandler;
        #endregion
    }
}
