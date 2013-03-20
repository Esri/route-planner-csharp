using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands.SendRoutesCommandHelpers
{
    /// <summary>
    /// Class that implements send routes functionality
    /// </summary>
    class SendRoutesHelper
    {
        #region public methods

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="resourceDictionary">Resource dictionary.</param>
        public void Initialize(IDictionary resourceDictionary)
        {
            _resourceDictionary = resourceDictionary;
            _app = App.Current;
        }

        /// <summary>
        /// Execute send
        /// </summary>
        /// <param name="routesConfigs">Sent routes config collection</param>
        /// <param name="deploymentDate">Date/time to deply routes for.</param>
        public void Execute(
            IList<SentRouteConfig> routesConfigs,
            DateTime deploymentDate)
        {
            // If all selected routes deploy type is tracking server - deploy routes to tracking server.
            if (_IsDeployToTrackingServer(routesConfigs))
            {
                var routes = routesConfigs.Select(config => config.Route);
                _DeployToRouteServer(routes, deploymentDate);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Check deploying type.
        /// </summary>
        /// <param name="routesConfigs">Config of routes to deploy.</param>
        /// <returns>True if need to deploy to tracking server. False otherwise.</returns>
        private bool _IsDeployToTrackingServer(IList<SentRouteConfig> routesConfigs)
        {
            // Get first selected route and check its type.
            foreach (SentRouteConfig sendedRouteConfig in routesConfigs)
            {
                if (sendedRouteConfig.IsChecked)
                {
                    MobileDevice mobileDevice = sendedRouteConfig.Route.Driver.MobileDevice;

                    if (mobileDevice == null)
                    {
                        mobileDevice = sendedRouteConfig.Route.Vehicle.MobileDevice;
                    }

                    if (mobileDevice.SyncType != SyncType.WMServer)
                    {
                        return false;
                    }
                }
            }

            // Show error in case of not all used mobile devices has unique tracking ID.
            if (!_IsTrackingIDUnique(routesConfigs))
            {
                string sendedMessage = (string)_resourceDictionary["FailedToSendRoutes"];

                List<MessageDetail> details = new List<MessageDetail>();
                string messageDetailText = (string)_resourceDictionary["MobileDevicesShouldHaveUniqueIDs"];
                MessageDetail detail = new MessageDetail(MessageType.Error, messageDetailText);
                details.Add(detail);
                _app.Messenger.AddError(sendedMessage, details);

                return false;
            }
            else
            {
                // Check that all routes has mobile device and its types is "Tracking server".
                if (!_IsMobileDevicesPresentWithTrackingType(routesConfigs))
                {
                    string messageDetailText = (string)_resourceDictionary["AllMobileDevicesShouldBePresentWithTrackingType"];
                    _app.Messenger.AddWarning(messageDetailText);
                }
            }

            return true;
        }

        /// <summary>
        /// Do deploying to route server.
        /// </summary>
        /// <param name="routes">Routes to deploy.</param>
        /// <param name="deploymentDate">Date/time to deploy routes for.</param>
        private void _DeployToRouteServer(
            IEnumerable<Route> routes,
            DateTime deploymentDate)
        {
            bool savedSuccessfully = false;
            var hasRoutesBeenSent = false;
            List<MessageDetail> details = new List<MessageDetail>();

            try
            {
                // Try to deploy.
                hasRoutesBeenSent = _app.Tracker.Deploy(routes, deploymentDate); // Exception

                // Save schedule changes to db.
                _app.Project.Save();

                savedSuccessfully = true;
            }
            catch (Exception ex)
            {
                // if some error occurs during.
                MessageDetail detail = new MessageDetail(MessageType.Error, ex.Message);
                details.Add(detail);

                Logger.Error(ex);
            }

            // Show deploy message in UI.
            // If routes was deployed.
            if (savedSuccessfully && hasRoutesBeenSent)
                _app.Messenger.AddInfo((string)_resourceDictionary["AllRoutesSent"]);
            // If there was nothing to deploy.
            else if (savedSuccessfully && !hasRoutesBeenSent)
                _app.Messenger.AddWarning((string)_resourceDictionary["NothingWasSent"]);
            // If there was error.
            else
                _app.Messenger.AddError((string)_resourceDictionary["FailedToSendRoutes"], details);
        }

        /// <summary>
        /// Is tracking IDs unique for checked routes.
        /// </summary>
        /// <param name="routesConfigs">Configuration of routes to deploy.</param>
        /// <returns>Is tracking IDs unique.</returns>
        private bool _IsTrackingIDUnique(IList<SentRouteConfig> routesConfigs)
        {
            bool isTrackingIDUnique = true;

            // Create tracking ID collection.
            StringCollection IDCollection = new StringCollection();
            foreach (SentRouteConfig sendedRouteConfig in routesConfigs)
            {
                if (sendedRouteConfig.IsChecked)
                {
                    // Check mobile device present.
                    MobileDevice mobileDevice = sendedRouteConfig.Route.Driver.MobileDevice;
                    if (mobileDevice == null)
                    {
                        mobileDevice = sendedRouteConfig.Route.Vehicle.MobileDevice;
                    }

                    if (mobileDevice != null && mobileDevice.SyncType == SyncType.WMServer)
                    {
                        // Check that tracking ID is unique.
                        if (IDCollection.Contains(mobileDevice.TrackingId))
                        {
                            isTrackingIDUnique = false;
                            break;
                        }

                        // Add tracking ID of current mobile device to ID collection.
                        IDCollection.Add(mobileDevice.TrackingId);
                    }
                }
            }

            return isTrackingIDUnique;
        }

        /// <summary>
        /// Is mobile device present for all deploying routes and tracking type is server.
        /// </summary>
        /// <param name="routesConfigs">Configuration of routes to deploy.</param>
        /// <returns>Is tracking IDs unique.</returns>
        private bool _IsMobileDevicesPresentWithTrackingType(IList<SentRouteConfig> routesConfigs)
        {
            bool isMobileDevicesPresentWithTrackingType = true;

            // Go through all selected routes and check mobile device present and its type is tracking server.
            foreach (SentRouteConfig sendedRouteConfig in routesConfigs)
            {
                if (sendedRouteConfig.IsChecked)
                {
                    MobileDevice mobileDevice = sendedRouteConfig.Route.Driver.MobileDevice;
                    if (mobileDevice == null)
                    {
                        mobileDevice = sendedRouteConfig.Route.Vehicle.MobileDevice;
                    }

                    if (mobileDevice == null || mobileDevice.SyncType != SyncType.WMServer)
                    {
                        isMobileDevicesPresentWithTrackingType = false;
                        break;
                    }
                }
            }

            return isMobileDevicesPresentWithTrackingType;
        }
        #endregion

        #region private members

        private App _app;
        private IDictionary _resourceDictionary;

        #endregion
    }
}
