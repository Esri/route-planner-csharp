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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// IAsyncState interface.
    /// </summary>
    internal interface IAsyncState
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets and sets reference count.
        /// </summary>
        int RefCount { get; set; }

        #endregion properties
    }

    /// <summary>
    /// ServiceClientWrap class.
    /// </summary>
    internal abstract class ServiceClientWrap<TClient, TChannel>
        where TClient : ClientBase<TChannel>
        where TChannel : class
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ServiceClientWrap(string url, AgsServerConnection connection)
        {
            Debug.Assert(url != null);
            Debug.Assert(connection != null);

            _baseUrl = url;
            _connection = connection;

            this.PersistConnection = true;
        }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void Close()
        {
            _CloseClient();
        }

        #endregion public methods

        #region protected delegates
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected delegate void ServiceMethod(TClient client);
        protected delegate T ServiceMethod<T>(TClient client);

        #endregion protected delegates

        #region protected abstract methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected abstract TClient CreateInnerClient(string url);

        #endregion protected abstract methods

        #region protected overridable methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected virtual void OnCloseInnerClient(TClient client) {}

        #endregion protected overridable methods

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a value indicating whether service connection should be
        /// kept open after invoking a request or it should be closed.
        /// </summary>
        protected bool PersistConnection
        {
            get;
            set;
        }

        #endregion protected properties

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Invokes service method without return parameter.
        /// </summary>
        protected void Invoke(ServiceMethod method)
        {
            ServiceMethod<Nothing> functionMethod = (client) =>
            {
                method.Invoke(client);
                return new Nothing();
            };

            this.Invoke(functionMethod);
        }

        /// <summary>
        /// Invokes service method that returns a value of type T.
        /// </summary>
        protected T Invoke<T>(ServiceMethod<T> method)
        {
            const int maxRetryCount = 1;
            int retryCount = 0;

            while (true)
            {
                var client = _AcquireClient();
                try
                {
                    return method.Invoke(client);
                }
                catch (Exception e)
                {
                    if (ProxyAuthenticationErrorHandler.HandleError(e))
                    {
                        continue;
                    }

                    var isTokenError = _IsTokenError(e);
                    if (retryCount >= maxRetryCount || !isTokenError)
                    {
                        if (ServiceHelper.IsCommunicationError(e))
                        {
                            throw ServiceHelper.CreateCommException(
                                _connection.Title,
                                e);
                        }

                        throw;
                    }

                    if (isTokenError)
                    {
                        _connection.GenerateToken();
                        _CloseClient(client);
                        client = null;
                    }

                    ++retryCount;
                }
                finally
                {
                    _ReleaseClient(client);
                }
            }
        }

        /// <summary>
        /// Invokes service method asynchronously.
        /// </summary>
        protected void InvokeAsync(ServiceMethod method, Guid id,
            object userState)
        {
            var client = _AcquireClient();

            AsyncContext ctx = new AsyncContext(method, userState);
            _asyncContextMap.Add(id, ctx);

            _IncrementAsyncRefCount(client);
            ctx.InvokeMethod(client);
        }

        /// <summary>
        /// Handles AsyncCompleted event results.
        /// </summary>
        /// <returns>
        /// Returns true if the operation is completed, false if it's retried.
        /// </returns>
        protected bool IsAsyncCompleted(object sender, AsyncCompletedEventArgs args,
            out AsyncCompletedEventArgs outArgs)
        {
            outArgs = null;

            Guid id = Guid.Empty;
            Exception error = args.Error;
            object userState = null;

            bool isCompleted = true;
            try
            {
                TClient client = (TClient)sender;
                _DecrementAsyncRefCount(client);

                if (args.UserState == null)
                    throw new InvalidOperationException();

                id = (Guid)args.UserState;

                AsyncContext ctx = null;
                if (!_asyncContextMap.TryGetValue(id, out ctx))
                    throw new InvalidOperationException();

                userState = ctx.UserState;
                if (_IsAsyncFailed(args))
                {
                    bool needToRetry =
                        ctx.RetryCount == 0 &&
                        _IsTokenError(args.Error);

                    if (ProxyAuthenticationErrorHandler.HandleError(args.Error))
                    {
                        needToRetry = true;
                    }

                    if (needToRetry)
                        _connection.GenerateToken(); // update token

                    if (needToRetry || client.State == CommunicationState.Faulted)
                    {
                        _CloseClient(client);

                        // do retry if needed
                        if (needToRetry)
                        {
                            client = _AcquireClient();
                            _IncrementAsyncRefCount(client);
                            ctx.RetryMethod(client);
                            isCompleted = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error = e;
            }

            if (isCompleted)
            {
                if (error != null)
                {
                    error = _ConvertCommException(error);
                }

                _asyncContextMap.Remove(id);
                outArgs = new AsyncCompletedEventArgs(error, args.Cancelled, userState);
            }

            return isCompleted;
        }

        #endregion protected methods

        #region private classes
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// AsyncContext class.
        /// </summary>
        private class AsyncContext
        {
            public AsyncContext(ServiceMethod method, object userState)
            {
                _method = method;
                _userState = userState;
            }

            public object UserState
            {
                get { return _userState; }
            }

            public int RetryCount
            {
                get { return _retryCount; }
            }

            public void InvokeMethod(TClient client)
            {
                _method.Invoke(client);
            }

            public void RetryMethod(TClient client)
            {
                _retryCount++;
                _method.Invoke(client);
            }

            private ServiceMethod _method;
            private object _userState;
            private int _retryCount = 0;
        }

        #endregion private classes

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Acquires service connection using already opened client or creating
        /// a new one if there is no client available.
        /// </summary>
        /// <returns>Client instance connected to the service.</returns>
        private TClient _AcquireClient()
        {
            if (_client != null)
            {
                if (_client.State != CommunicationState.Faulted)
                {
                    return _client;
                }
                else
                {
                    _CloseClient();
                }
            }

            return _CreateClient();
        }

        /// <summary>
        /// Releases service connection caching the specified client if and only if
        /// <see cref="PersistConnection"/> property is true.
        /// </summary>
        /// <param name="client">Client instance connected to the service.</param>
        private void _ReleaseClient(TClient client)
        {
            if (this.PersistConnection)
            {
                _client = client;
            }
            else
            {
                _CloseClient(client);
            }
        }

        private TClient _CreateClient()
        {
            TClient client = this.CreateInnerClient(_MakeServiceUrl());

            // access channel factory:
            // this strange trick is a legal way to disable channel factory caching
            ChannelFactory<TChannel> fact = client.ChannelFactory;

            // set credentials if necessary
            if (_connection.RequiresHttpAuthentication)
            {
                ServiceHelper.SetClientCredentials(client,
                    _connection.Credentials);
            }

            return client;
        }

        private void _CloseClient()
        {
            if (_client != null)
            {
                _CloseClient(_client);
                _client = null;
            }
        }

        private void _CloseClient(TClient client)
        {
            int refCount = 0;

            IAsyncState asyncState = client as IAsyncState;
            if (asyncState != null)
                refCount = asyncState.RefCount;

            if (refCount == 0)
            {
                try
                {
                    this.OnCloseInnerClient(client);
                }
                catch { } // ignore errors

                ServiceHelper.CloseCommObject(client);
            }
        }

        private string _MakeServiceUrl()
        {
            Debug.Assert(_baseUrl != null);
            Debug.Assert(_connection != null);

            string url = _baseUrl;
            if (_connection.RequiresTokens)
                url = AgsHelper.FormatTokenUrl(url, _connection.LastToken);

            return url;
        }

        private static void _IncrementAsyncRefCount(TClient client)
        {
            IAsyncState asyncState = _GetAsyncState(client);
            asyncState.RefCount++;
        }

        private static void _DecrementAsyncRefCount(TClient client)
        {
            IAsyncState asyncState = _GetAsyncState(client);
            if (asyncState.RefCount > 0)
                asyncState.RefCount--;
        }

        private static IAsyncState _GetAsyncState(TClient client)
        {
            IAsyncState asyncState = client as IAsyncState;
            if (asyncState == null)
                throw new InvalidOperationException();

            return asyncState;
        }

        private static bool _IsAsyncFailed(AsyncCompletedEventArgs args)
        {
            return (!args.Cancelled && args.Error != null);
        }

        private static bool _IsTokenError(Exception ex)
        {
            WebException webEx = null;
            if (ex is WebException)
                webEx = ex as WebException;
            else if (ex.InnerException != null && ex.InnerException is WebException)
                webEx = ex.InnerException as WebException;

            bool res = false;
            if (webEx != null)
            {
                if (webEx.Response != null &&
                    (int)((HttpWebResponse)webEx.Response).StatusCode == AgsConst.ARCGIS_EXPIRED_TOKEN)
                {
                    res = true;
                }
            }

            return res;
        }

        private Exception _ConvertCommException(Exception ex)
        {
            if (ServiceHelper.IsCommunicationError(ex))
            {
                return ServiceHelper.CreateCommException(_connection.Title, ex);
            }

            return ex;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private TClient _client;
        private AgsServerConnection _connection;
        private string _baseUrl;

        private Dictionary<Guid, AsyncContext> _asyncContextMap = new Dictionary<
            Guid, AsyncContext>();

        #endregion private fields
    }
}
