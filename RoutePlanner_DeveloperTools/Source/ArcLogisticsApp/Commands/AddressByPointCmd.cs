using System;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Reverse Geocoding command.
    /// </summary>
    class AddressByPointCmd : CommandBase
    {
        #region properties

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        /// <summary>
        /// Title of the command that can be shown in UI.
        /// </summary>
        public override string Title 
        {
            get { return string.Empty; } 
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public override string TooltipText 
        {
            get { return string.Empty; }
            protected set { }
        }

        /// <summary>
        /// Index of returned argument which sygnalize about responce received.
        /// </summary>
        public int IsResponceReceivedIndex
        {
            get
            {
                return IS_RESPONSE_RECEIVED_INDEX;
            }
        }

        #endregion

        #region CommandBase methods

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="args">Command args.</param>
        protected override void _Execute(params object[] args)
        {
            System.Windows.Point point = (System.Windows.Point)args[0];
            ESRI.ArcLogistics.Geometry.Point location =
                new ESRI.ArcLogistics.Geometry.Point(point.X, point.Y);

            IGeocodable geocodable = (IGeocodable)args[1];

            // Save new GeoLocation anyway.
            geocodable.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(point.X, point.Y);

            // If one or more address fields is not empty - do not fill them with new values
            // otherwise make reversegeocoding request and fill address fields.
            if (CommonHelpers.IsAllAddressFieldsEmpty(geocodable.Address))
            {
                args[IS_RESPONSE_RECEIVED_INDEX] = _ProcessReverseGeocode(geocodable, location);
            }
            else
            {
                args[IS_RESPONSE_RECEIVED_INDEX] = true;
                // In case of manually filled address fields after setting position.
                // set locator to manually edited xy
                string manuallyEditedXY = (string)System.Windows.Application.Current.FindResource(MANUALLY_EDITED_XY_RESOURCE_NAME);
                if (geocodable.Address.MatchMethod == null ||
                    !geocodable.Address.MatchMethod.Equals(manuallyEditedXY, StringComparison.OrdinalIgnoreCase))
                    geocodable.Address.MatchMethod = manuallyEditedXY;
            }

            // Workaround - see method comment.
            CommonHelpers.FillAddressWithSameValues(geocodable.Address);
        }

        /// <summary>
        /// Make reverse geocode request and fill geocodable object with new address fields values.
        /// </summary>
        /// <param name="geocodable">Geocodable to fill.</param>
        /// <param name="location">Geolocation point for request.</param>
        /// <returns>Is responce was received.</returns>
        private bool _ProcessReverseGeocode(IGeocodable geocodable, ESRI.ArcLogistics.Geometry.Point location)
        {
            bool result = false;

            try
            {
                Address geocodedAddress = App.Current.Geocoder.ReverseGeocode(location);

                // In case of not empty response - fill address.
                if (geocodedAddress != null)
                {
                    geocodedAddress.CopyTo(geocodable.Address);
                }
                else
                {
                    geocodable.Address.MatchMethod = (string)App.Current.FindResource(MANUALLY_EDITED_XY_FAR_FROM_NEAREST_ROAD_RESOURCE_NAME);
                }

                result = true;
            }
            catch (Exception ex)
            {
                if (ex is AuthenticationException || ex is CommunicationException)
                {
                    string service = (string)App.Current.FindResource(GEOCODING_SERVICE_NAME_RESOURCE_NAME);
                    CommonHelpers.AddServiceMessage(service, ex);
                }
                else
                    throw;
            }

            return result;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Match method manually edited items resource name.
        /// </summary>
        private const string MANUALLY_EDITED_XY_RESOURCE_NAME = "ManuallyEditedXY";

        /// <summary>
        /// Match method for not geocoded items resource name.
        /// </summary>
        private const string MANUALLY_EDITED_XY_FAR_FROM_NEAREST_ROAD_RESOURCE_NAME = "ManuallyEditedXYFarFromNearestRoad";

        /// <summary>
        /// Command name.
        /// </summary>
        public const string COMMAND_NAME = "ArcLogistics.Commands.AddressByPoint";

        /// <summary>
        /// Resource name of geocoding service name.
        /// </summary>
        private const string GEOCODING_SERVICE_NAME_RESOURCE_NAME = "ServiceNameGeocoding";

        /// <summary>
        /// Index of returned argument which sygnalize about responce received.
        /// </summary>
        private const int IS_RESPONSE_RECEIVED_INDEX = 2;

        #endregion
    }
}
