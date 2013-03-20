using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;
using ESRI.ArcLogistics.Tracking.TrackingService;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Provides access to the <see cref="Tracker"/> class instances.
    /// </summary>
    internal sealed class TrackerProvider
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerProvider"/> class.
        /// </summary>
        /// <param name="settings">Tracking settings to be used for created <see cref="Tracker"/>
        /// class instances.</param>
        /// <param name="servers">Collection of ags servers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> or 
        /// <paramref name="servers"/> is a null reference.</exception>
        /// <exception cref="ArgumentException"><paramref name="settings"/> contains invalid
        /// tracking settings.</exception>
        public TrackerProvider(TrackingInfo settings, ICollection<Services.AgsServer> servers)
       {
            _CheckSettings(settings);
            if (servers == null)
                throw new ArgumentNullException("servers");

            _settings = settings;
            Server = _GetServerByName(servers, settings.TrackingServiceInfo.ServerName);
        }

        #endregion

        #region public members

        /// <summary>
        /// Server with feature services.
        /// </summary>
        public Services.AgsServer Server
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a reference to the <see cref="Tracker"/> class object.
        /// </summary>
        /// <param name="solver">The solver to be used by the returned tracker.</param>
        /// <param name="geocoder">The geocoder to be used by the returned tracker.</param>
        /// <param name="messageReporter">The messageReporter to be used by the returned
        /// tracker.</param>
        /// <returns>A new <see cref="Tracker"/> class instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="solver"/>,
        /// <paramref name="geocoder"/> or <paramref name="messageReporter"/> is a null
        /// reference.</exception>
        public Tracker GetTracker(
            IVrpSolver solver,
            IGeocoder geocoder,
            IMessageReporter messageReporter)
        {
            CodeContract.RequiresNotNull("solver", solver);
            CodeContract.RequiresNotNull("geocoder", geocoder);
            CodeContract.RequiresNotNull("messageReporter", messageReporter);

            _CheckSettings(_settings);

            var settings = new TrackingSettings
            {
                BreakTolerance = _settings.TrackingSettings.BreakTolerance ?? 0,
            };

            var uri = new Uri(_settings.TrackingServiceInfo.RestUrl);
            var service = FeatureService.Create(uri, Server);
            var trackingServiceClient = new TrackingServiceClient(service);

            var trackingService = new TrackingServiceClient(service);
            var synchronizationService = new SynchronizationService(trackingServiceClient);

            return new Tracker(
                settings,
                trackingService,
                synchronizationService,
                solver,
                geocoder,
                messageReporter);
        }

        #endregion

         #region private static methods

        /// <summary>
        /// Check that tracking settings are valid.
        /// </summary>
        /// <param name="settings">Tracking settings to be used for creating <see cref="Tracker"/>
        /// class instances.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException"><paramref name="settings"/> contains invalid
        /// tracking settings.</exception>
        private static void _CheckSettings(TrackingInfo settings)
        {
            if (settings == null)
                throw new ArgumentNullException("TrackingInfo", 
                    Properties.Messages.Warning_NoTrackingConfig);

            if (settings.TrackingServiceInfo == null ||
                string.IsNullOrEmpty(settings.TrackingServiceInfo.RestUrl) ||
                string.IsNullOrEmpty(settings.TrackingServiceInfo.ServerName) || 
                settings.TrackingSettings == null)
            {
                throw new ArgumentException(
                    Properties.Messages.Error_InvalidTrackingConfig,
                    "settings");
            }
        }

        /// <summary>
        /// Converts the specified number of seconds to milliseconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to be converted.</param>
        /// <returns>The number of milliseconds corresponding to the specified number of
        /// seconds.</returns>
        private static int _ConvertSecondsToMilliseconds(int seconds)
        {
            var milliseconds = TimeSpan.FromSeconds(seconds).TotalMilliseconds;

            return (int)milliseconds;
        }
         #endregion

        #region private methods

        /// <summary>
        /// Gets server with the specified name from the specified collection and
        /// throws an exception if the server was not found.
        /// </summary>
        /// <param name="servers">The reference to the servers collection to
        /// get server from.</param>
        /// <param name="serverName">The name of the server to get.</param>
        /// <returns>The reference to the <see cref="T:ESRI.ArcLogistics.Services.AgsServer"/>
        /// object with the specified name. If the server was not found a
        /// <see cref="T:System.ApplicationException"/> is thrown.</returns>
        /// <exception cref="T:System.ApplicationException">The server was not found.</exception>
        private AgsServer _GetServerByName(ICollection<AgsServer> servers, string serverName)
        {
            var server = ServiceHelper.FindServerByName(serverName, servers);
            if (server == null)
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidTrackingConfig);
            }

            return server;
        }

        #endregion

        #region private fields
        /// <summary>
        /// The tracking settings object to be used for all created <see cref="Tracker"/> objects.
        /// </summary>
        private TrackingInfo _settings;

        #endregion
    }
}
