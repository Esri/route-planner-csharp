using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Represents a geocoder.
    /// </summary>
    public interface IGeocoder
    {
        #region events

        /// <summary>
        /// Raises when async reverse geocoding operation completes.
        /// </summary>
        event AsyncReverseGeocodedEventHandler AsyncReverseGeocodeCompleted;

        #endregion events

        #region properties
        /// <summary>
        /// Gets a value indicating the minimum score value for an address candidate
        /// retrieved via geocoding to be treated as matched. 
        /// </summary>
        int MinimumMatchScore
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the collection of locators information for composite locators or
        /// an empty collection for non-composite ones.
        /// </summary>
        ReadOnlyCollection<LocatorInfo> Locators
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating if the geocoder represents composite locator.
        /// </summary>
        bool IsCompositeLocator
        {
            get;
        }

        /// <summary>
        /// Gets geocoder's address fields.
        /// </summary>
        AddressField[] AddressFields
        {
            get;
        }

        /// <summary>
        /// Get a value indicating which address format is used.
        /// </summary>
        AddressFormat AddressFormat
        {
            get;
        }

        #endregion

        // APIREV: need to specify what happens if geocoding fails

        /// <summary>
        /// Geocodes an address and returns the best candidate.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <returns>Returns address candidate with max score.</returns>
        AddressCandidate Geocode(Address address);

        /// <summary>
        /// Geocodes array of addresses.
        /// </summary>
        /// <param name="addresses">Array of addresses.</param>
        /// <returns>Returns array of best candidates for each input address.</returns>
        AddressCandidate[] BatchGeocode(Address[] addresses);

        /// <summary>
        /// Geocodes address and returns array of found candidates.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Returns array of found address candidates.</returns>
        AddressCandidate[] GeocodeCandidates(Address address, bool includeDisabledLocators);

        /// <summary>
        /// Finds address by geographical location.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <returns>Returns found address.</returns>
        Address ReverseGeocode(Point location);

        /// <summary>
        /// Finds address by geographical location. Asynchronous method.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <param name="userToken">Geocoding operation token.</param>
        void ReverseGeocodeAsync(Point location, object userToken);

        /// <summary>
        /// Cancels asynchronous reverse geocoding operation.
        /// </summary>
        /// <param name="userToken">Token of the geocoding operation that must be cancelled.</param>
        /// <returns>Returns <c>true</c> if operation successfully cancelled, or <c>false</c> if operation with the token was not found.
        /// </returns>
        bool ReverseGeocodeAsyncCancel(object userToken);
    }

    /// <summary>
    /// Represents the method that handles <c>AsyncReverseGeocodeCompleted</c> event. 
    /// </summary>
    public delegate void AsyncReverseGeocodedEventHandler(
        Object sender,
        AsyncReverseGeocodedEventArgs e
    );

    /// <summary>
    /// Provides data for <c>AsyncReverseGeocodeCompleted</c> event.
    /// </summary>
    public class AsyncReverseGeocodedEventArgs : AsyncCompletedEventArgs
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>AsyncReverseGeocodedEventArgs</c> class.
        /// </summary>
        /// <param name="address">Found address.</param>
        /// <param name="location">Address location.</param>
        public AsyncReverseGeocodedEventArgs(Address address, 
            ESRI.ArcGIS.Client.Geometry.MapPoint location, object userState)
            : base(null, false, userState)
        {
            _address = (Address)address.Clone();
            _location = new ESRI.ArcGIS.Client.Geometry.MapPoint(location.X, location.Y);
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Found address.
        /// </summary>
        public Address Address
        {
            get { return _address; }
        }

        /// <summary>
        /// Address location.
        /// </summary>
        public ESRI.ArcGIS.Client.Geometry.MapPoint Location
        {
            get { return _location; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Address _address;
        private ESRI.ArcGIS.Client.Geometry.MapPoint _location;

        #endregion private fields
    }
}
