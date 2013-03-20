using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Manages a pool of client-side connections to the WCF service.
    /// </summary>
    /// <typeparam name="TService">The type of the service interface to connect to.</typeparam>
    internal sealed class WcfClientConnectionPool<TService>
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientConnectionPool&lt;TService&gt;"/>
        /// class.
        /// </summary>
        /// <param name="channelFactory">The reference to the channel factory instance
        /// to be used for creating communication channels for the REST service.</param>
        /// <exception cref="ArgumentNullException"><paramref name="channelFactory"/> is a null
        /// reference.</exception>
        public WcfClientConnectionPool(ChannelFactory<TService> channelFactory)
        {
            CodeContract.RequiresNotNull("channelFactory", channelFactory);

            _channelFactory = channelFactory;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Acquires new connection from the pool.
        /// </summary>
        /// <returns>A new instance of the <see cref="IWcfClientConnection&lt;TService&gt;"/> for
        /// interacting with the service.</returns>
        public IWcfClientConnection<TService> AcquireConnection()
        {
            var client = _AcquireClient();
            var connection = new WcfClientConnection(this, client);

            return connection;
        }
        #endregion

        #region private classes
        /// <summary>
        /// Implementation of the pooled WCF service connection.
        /// </summary>
        private sealed class WcfClientConnection : IWcfClientConnection<TService>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WcfClientConnection"/> class.
            /// </summary>
            /// <param name="owner">The pool object owning this connection.</param>
            /// <param name="client">The service client object for this connection.</param>
            public WcfClientConnection(
                WcfClientConnectionPool<TService> owner,
                TService client)
            {
                Debug.Assert(owner != null);
                Debug.Assert(client != null);

                _owner = owner;
                _client = client;
            }

            /// <summary>
            /// Gets reference to the client-side WCF service interface.
            /// </summary>
            public TService Client
            {
                get
                {
                    _EnsureNotDisposed();

                    return _client;
                }
            }

            /// <summary>
            /// Disposes this connection returning it to the pool.
            /// </summary>
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _owner._ReleaseClient(_client);
                _disposed = true;
            }

            #region private methods
            /// <summary>
            /// Checks if this connection is disposed already.
            /// </summary>
            /// <exception cref="ObjectDisposedException">The connection object is already
            /// disposed.</exception>
            private void _EnsureNotDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
            }
            #endregion

            #region private fields
            /// <summary>
            /// The owner of this connection.
            /// </summary>
            private WcfClientConnectionPool<TService> _owner;

            /// <summary>
            /// The service client object for this connection.
            /// </summary>
            private TService _client;

            /// <summary>
            /// Indicates if connection was disposed.
            /// </summary>
            private bool _disposed;
            #endregion
        }
        #endregion

        #region private methods
        /// <summary>
        /// Acquires service connection using already opened client or creating
        /// a new one if there is no client available.
        /// </summary>
        /// <returns>Client instance connected to the service.</returns>
        private TService _AcquireClient()
        {
            var channel = default(IChannel);

            // Attempt to get existing connection from pool.
            lock (_channelsGuard)
            {
                while (_channels.Count > 0)
                {
                    channel = _channels.First();
                    _channels.Remove(channel);

                    // Channel is removed from the pool, so no need to listen for events.
                    channel.Faulted -= _ChannelFaulted;
                    channel.Closed -= _ChannelClosed;

                    if (channel.State != CommunicationState.Faulted)
                    {
                        return (TService)channel;
                    }

                    // The channel is faulted, so just close it and proceed to the next one.
                    _CloseChannel(channel);
                }
            }

            // There were no usable channels in the pool, so create a new one.
            channel = (IChannel)_channelFactory.CreateChannel();

            return (TService)channel;
        }

        /// <summary>
        /// Releases the specified service client returning it to the pool if possible.
        /// </summary>
        /// <param name="client">The service client to be released.</param>
        private void _ReleaseClient(TService client)
        {
            Debug.Assert(client != null);

            var channel = (IChannel)client;

            // Check if it makes sense to return channel in the pool.
            if (channel.State == CommunicationState.Faulted)
            {
                _CloseChannel(channel);
            }

            if (channel.State == CommunicationState.Closed ||
                channel.State == CommunicationState.Closing)
            {
                return;
            }

            // The channel can be reused, so store it in the pool.
            lock (_channelsGuard)
            {
                _channels.Add(channel);

                // Listen for changes in the channel state in order to remove unusable channels
                // from the pool.
                channel.Faulted += _ChannelFaulted;
                channel.Closed += _ChannelClosed;
            }
        }

        /// <summary>
        /// Handles channel faulting event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data object.</param>
        private void _ChannelFaulted(object sender, EventArgs e)
        {
            var channel = (IChannel)sender;

            _CloseChannel(channel);
        }

        /// <summary>
        /// Handles channel closing event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data object.</param>
        private void _ChannelClosed(object sender, EventArgs e)
        {
            var channel = (IChannel)sender;

            lock (_channelsGuard)
            {
                _channels.Remove(channel);
            }
        }

        /// <summary>
        /// Closes the specified service client.
        /// </summary>
        /// <param name="client">The service client to be closed.</param>
        private void _CloseChannel(IChannel client)
        {
            if (client == null)
            {
                return;
            }

            ServiceHelper.CloseCommObject(client);
        }
        #endregion

        #region private fields
        /// <summary>
        /// The channel factory instance to be used for creating communication channels for
        /// the WCF service.
        /// </summary>
        private ChannelFactory<TService> _channelFactory;

        /// <summary>
        /// Collection of currently opened communication channel instances to be used for
        /// interaction with the WCF service.
        /// </summary>
        private HashSet<IChannel> _channels = new HashSet<IChannel>();

        /// <summary>
        /// The object to be used for serializing access to the _channels field.
        /// </summary>
        private object _channelsGuard = new object();
        #endregion
    }
}
