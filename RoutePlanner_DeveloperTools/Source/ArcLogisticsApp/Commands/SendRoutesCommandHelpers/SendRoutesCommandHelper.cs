using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Tracking;

namespace ESRI.ArcLogistics.App.Commands.SendRoutesCommandHelpers
{
    internal sealed class SendRoutesCommandHelper : IRoutesSender
    {
        #region constructors
        public SendRoutesCommandHelper(IDictionary resourceDictionary)
        {
            _resourceDictionary = resourceDictionary;
        }
        #endregion

        #region IRoutesSender Members
        /// <summary>
        /// Sends specified routes to the workflow management server.
        /// </summary>
        /// <param name="routes">Routes to be send.</param>
        /// <param name="deploymentDate">Date/time to deploy routes for.</param>
        public void Send(IEnumerable<Route> routes, DateTime deploymentDate)
        {
            var routesConfigs = new ObservableCollection<SentRouteConfig>();
            foreach (Route route in routes)
            {
                if (route.Driver == null)
                    continue;

                SentRouteConfig sentRouteConfig = _CreateSendedRouteConfig(route);
                routesConfigs.Add(sentRouteConfig);
            }

            SendRoutesHelper sendRoutesHelper = new SendRoutesHelper();
            sendRoutesHelper.Initialize(_resourceDictionary);
            sendRoutesHelper.Execute(routesConfigs, deploymentDate);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Create sent route config from route.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>Sent route config.</returns>
        private SentRouteConfig _CreateSendedRouteConfig(Route route)
        {
            SentRouteConfig sendedRouteConfig = new SentRouteConfig(route);

            MobileDevice mobileDevice = route.Driver.MobileDevice;
            if (mobileDevice == null)
                mobileDevice = route.Vehicle.MobileDevice;

            sendedRouteConfig.RouteName = route.Name;
            if (mobileDevice != null && mobileDevice.SyncType != SyncType.None)
            {
                sendedRouteConfig.IsChecked = true;
            }
            else
            {
                sendedRouteConfig.IsChecked = false;
            }
            return sendedRouteConfig;
        }
        #endregion

        #region private members

        private IDictionary _resourceDictionary;

        #endregion
    }
}
