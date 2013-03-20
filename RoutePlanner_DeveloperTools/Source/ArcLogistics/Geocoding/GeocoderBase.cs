using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Abstract class that represents a base geocoder.
    /// </summary>
    public abstract class GeocoderBase : IGeocoder
    {
        #region Public events

        /// <summary>
        /// Raises when async reverse geocoding operation completes.
        /// </summary>
        public abstract event AsyncReverseGeocodedEventHandler AsyncReverseGeocodeCompleted;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets a value indicating the minimum score value for an address candidate
        /// retrieved via geocoding to be treated as matched. 
        /// </summary>
        public abstract int MinimumMatchScore
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the collection of locators information for composite locators or
        /// an empty collection for non-composite ones.
        /// </summary>
        public abstract System.Collections.ObjectModel.ReadOnlyCollection<LocatorInfo> Locators
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating if the geocoder represents composite locator.
        /// </summary>
        public abstract bool IsCompositeLocator
        {
            get;
        }

        /// <summary>
        /// Gets geocoder's address fields.
        /// </summary>
        public abstract AddressField[] AddressFields
        {
            get;
        }

        /// <summary>
        /// Get a value indicating which address format is used.
        /// </summary>
        public abstract AddressFormat AddressFormat
        {
            get;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Geocodes an address and returns the best candidate.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <returns>Returns address candidate with max score.</returns>
        public abstract AddressCandidate Geocode(DomainObjects.Address address);

        /// <summary>
        /// Geocodes array of addresses.
        /// </summary>
        /// <param name="addresses">Array of addresses.</param>
        /// <returns>Returns array of best candidates for each input address.</returns>
        public abstract AddressCandidate[] BatchGeocode(DomainObjects.Address[] addresses);

        /// <summary>
        /// Geocodes address and returns array of found candidates.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Returns array of found address candidates.</returns>
        public abstract AddressCandidate[] GeocodeCandidates(
            DomainObjects.Address address, bool includeDisabledLocators);

        /// <summary>
        /// Finds address by geographical location.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <returns>Returns found address.</returns>
        public abstract DomainObjects.Address ReverseGeocode(
            Geometry.Point location);

        /// <summary>
        /// Finds address by geographical location. Asynchronous method.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <param name="userToken">Geocoding operation token.</param>
        public abstract void ReverseGeocodeAsync(
            Geometry.Point location, object userToken);

        /// <summary>
        /// Cancels asynchronous reverse geocoding operation.
        /// </summary>
        /// <param name="userToken">Token of the geocoding operation that must be cancelled.</param>
        /// <returns>Returns <c>true</c> if operation successfully cancelled,
        /// or <c>false</c> if operation with the token was not found.
        /// </returns>
        public abstract bool ReverseGeocodeAsyncCancel(object userToken);

        #endregion

        #region Internal methods

        /// <summary>
        /// Determines is geocoder initialized.
        /// </summary>
        /// <returns>True if geocoder is initialized, otherwise false.</returns>
        internal abstract bool IsInitialized();

        #endregion
    }
}