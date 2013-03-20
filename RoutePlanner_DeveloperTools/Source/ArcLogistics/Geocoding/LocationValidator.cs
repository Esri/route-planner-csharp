using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.Geocoding.ILocationValidator"/> using supplied
    /// geocoder object for reverse geocoding.
    /// </summary>
    public sealed class LocationValidator : ILocationValidator
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LocationValidator class.
        /// </summary>
        /// <param name="streetsGeocoder">The reference to the geocoder objects to be used for
        /// street addresses reverse geocoding.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="streetsGeocoder"/>
        /// argument is a null reference.</exception>
        public LocationValidator(IGeocoder streetsGeocoder)
        {
            if (streetsGeocoder == null)
            {
                throw new ArgumentNullException("streetsGeocoder");
            }

            _streetsGeocoder = streetsGeocoder;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Finds address candidates with "incorrect" locations, i.e. ones which can't be reverse
        /// geocoded using streets geocoder to the address set for candidate.
        /// </summary>
        /// <param name="candidates">The reference to the collection of address candidates to
        /// search for ones with incorrect locations at.</param>
        /// <returns>A collection of indices of candidates with incorrect locations.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="candidates"/> argument
        /// is a null reference.</exception>
        public IEnumerable<int> FindIncorrectLocations(IEnumerable<AddressCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException("candidates");
            }

            var result = _FindIncorrectLocations(candidates);

            return result;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Implements incorrect locations finding.
        /// </summary>
        /// <param name="candidates">The reference to the collection of address candidates to
        /// search for ones with incorrect locations at.</param>
        /// <returns>A collection of indices of candidates with incorrect locations.</returns>
        /// <remarks>We need a separate method with iterator block to provide eager validation
        /// of arguments for the public interface method.</remarks>
        private IEnumerable<int> _FindIncorrectLocations(IEnumerable<AddressCandidate> candidates)
        {
            Debug.Assert(candidates != null);

            foreach (var item in candidates.ToIndexed())
            {
                var candidate = item.Value;

                if (candidate == null ||
                    candidate.Locator == null ||
                    candidate.Locator.Type != SublocatorType.AddressPoint)
                {
                    continue;
                }

                var address = _streetsGeocoder.ReverseGeocode(candidate.GeoLocation);
                if (address == null)
                {
                    yield return item.Index;

                    continue;
                }

                var fullAddress = candidate.Address.FullAddress.ToUpper();
                string addressLine = address.AddressLine ?? string.Empty;

                // If unit field do not contain House number, find and remove it from AddressLine.
                if (string.IsNullOrEmpty(address.Unit))
                    addressLine = _GetStreetLineFromAddressLine(addressLine);
                else
                {
                    // AddressLine doesn't contain House numbers.
                }

                // Check AddressLine.
                if (fullAddress.Contains(addressLine.ToUpper()))
                {
                    continue;
                }

                yield return item.Index;
            }
        }

        /// <summary>
        /// Method gets only street line without House number from address line.
        /// </summary>
        /// <param name="addressLine">Address line.</param>
        /// <returns>Street line without House number.</returns>
        private string _GetStreetLineFromAddressLine(string addressLine)
        {
            string result = addressLine;
            string[] addressParts = addressLine.Split();

            // House + Address should be at least 2 parts.
            if (addressParts.Length > 1)
            {
                string firstPart = addressParts[0];
                string lastPart = addressParts[addressParts.Length - 1];

                // Check for House number at the First (before street name) and
                // at the Last (after street name) address part.
                if (_IsInteger(firstPart))
                    result = _CreateAddressWithoutHouseNumber(firstPart, true, addressLine);
                else if (_IsInteger(lastPart))
                    result = _CreateAddressWithoutHouseNumber(lastPart, false, addressLine);
                else
                {
                    // Do nothing: return adress line, since it doesn't contain house number.
                }
            }

            return result;
        }

        /// <summary>
        /// Method determines if string value is Integer Number.
        /// </summary>
        /// <param name="valueToDetermine"></param>
        /// <returns>True - if it is integer, otherwise false.</returns>
        private bool _IsInteger(string valueToDetermine)
        {
            Double result;
            return Double.TryParse(valueToDetermine, NumberStyles.Integer,
                CultureInfo.CurrentCulture, out result);
        }

        /// <summary>
        /// Method creates address line from Address Parts without house number.
        /// </summary>
        /// <param name="houseNumber">House number.</param>
        /// <param name="isHouseNumberBeforeStreet">Is house number before or after street.</param>
        /// <param name="addressLine">Address line.</param>
        /// <returns>Address line without house number.</returns>
        private string _CreateAddressWithoutHouseNumber(string houseNumber,
            bool isHouseNumberBeforeStreet, string addressLine)
        {
            string result = string.Empty;

            int addressLength = addressLine.Length - houseNumber.Length;

            // Remove redundant space before street.
            if (isHouseNumberBeforeStreet)
            {
                // Remove house number.
                result = addressLine.Substring(houseNumber.Length, addressLength);
                result = result.TrimStart(SPACE);
            }
            // Remove redundant space after street.
            else
            {
                // Remove house number.
                result = addressLine.Substring(0, addressLength);
                result = result.TrimEnd(SPACE);
            }

            return result;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Space.
        /// </summary>
        private const char SPACE = ' ';

        #endregion

        #region private fields
        /// <summary>
        /// The reference to the geocoder objects for street addresses geocoding.
        /// </summary>
        private readonly IGeocoder _streetsGeocoder;
        #endregion
    }
}
