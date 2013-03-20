using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Geocode
{
    /// <summary>
    /// Class for geocode, using local geocoder.
    /// </summary>
    class LocalGeocoder
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="geocoder">Application server geocoder.</param>
        /// <param name="nameAddressStorage">Local storage for name/address geocoding.</param>
        public LocalGeocoder(IGeocoder geocoder, NameAddressStorage nameAddressStorage)
        {
            Debug.Assert(geocoder != null);
            Debug.Assert(nameAddressStorage != null);

            _geocoder = geocoder;
            _nameAddressStorage = nameAddressStorage;
        }

        #endregion

        #region Public constants

        /// <summary>
        /// Get address candidate from local storage.
        /// </summary>
        /// <param name="nameAddress">Name\Address pair to geocode.</param>
        /// <returns>Address candidate if exists in local storage. Null otherwise.</returns>
        public AddressCandidate Geocode(NameAddress nameAddress)
        {
            Debug.Assert(nameAddress != null);
            Debug.Assert(_nameAddressStorage != null);

            // Search record in storage.
            NameAddressRecord nameAddressRecord = _nameAddressStorage.Search(
                nameAddress, App.Current.Geocoder.AddressFormat);

            AddressCandidate candidateFromLocalStorage = null;
            
            // Extract candidate from record, if record exists.
            if (nameAddressRecord != null)
            {
                candidateFromLocalStorage = _ConvertToCandidate(nameAddressRecord);
            }

            return candidateFromLocalStorage;
        }

        /// <summary>
        /// Get address candidates from local storage and from server.
        /// </summary>
        /// <param name="nameAddress">Name\Address pair to geocode.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Candidates list.</returns>
        public AddressCandidate[] GeocodeCandidates(NameAddress nameAddress, bool includeDisabledLocators)
        {
            Debug.Assert(nameAddress != null);

            List<AddressCandidate> candidates = _GeocodeCandidates(nameAddress.Address, includeDisabledLocators);

            AddressCandidate candidateFromLocalStorage = Geocode(nameAddress);

            if (candidateFromLocalStorage != null)
            {
                // If candidate from local storage exists - insert it to first position.
                candidates.Insert(0, candidateFromLocalStorage);
            }

            return candidates.ToArray();
        }

        /// <summary>
        /// Geocode batch of of items.
        /// </summary>
        /// <param name="nameAddresses">Name\Address pairs to geocode.</param>
        /// <returns></returns>
        public AddressCandidate[] BatchGeocode(NameAddress[] nameAddresses)
        {
            Debug.Assert(nameAddresses != null);
            Debug.Assert(_geocoder != null);

            // Create candidates array.
            AddressCandidate[] candidates = new AddressCandidate[nameAddresses.Length];

            // Try to find candidates in local storage and put in appropriate position in candidates array.
            // Leave null if candidate not found. Return not addresses, which not exists in local storage.
            List<Address> addressesToGeocode = _ProcessLocalBatchGeocoding(nameAddresses, candidates);

            if (addressesToGeocode.Count > 0)
            {
                // Make batch geocode at server.
                AddressCandidate[] candidatesFromServer = _geocoder.BatchGeocode(addressesToGeocode.ToArray());

                // Fill results of batch geocoding in candidates array.
                _ProcessServerBatchGeocoding(candidatesFromServer, candidates);
            }

            return candidates;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Convert local storage record to address candidate.
        /// </summary>
        /// <param name="nameAddressRecord">Local storage record.</param>
        /// <returns>Address candidate.</returns>
        private AddressCandidate _ConvertToCandidate(NameAddressRecord nameAddressRecord)
        {
            Debug.Assert(nameAddressRecord != null);

            AddressCandidate candidateFromLocalStorage = new AddressCandidate();

            // Candidate from local storage have maximum score.
            candidateFromLocalStorage.Score = MAXIMUM_SCORE;

            // Set candidate geolocation.
            candidateFromLocalStorage.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(
                nameAddressRecord.GeoLocation.X, nameAddressRecord.GeoLocation.Y);

            // Set candidate address.
            Address candidateAddress = new Address();
            candidateFromLocalStorage.Address = candidateAddress;

            Address matchedAddress = nameAddressRecord.MatchedAddress;
            Address address = nameAddressRecord.NameAddress.Address;

            Address addressToCopy;
            if (CommonHelpers.IsAllAddressFieldsEmpty(matchedAddress) && string.IsNullOrEmpty(matchedAddress.MatchMethod))
            {
                addressToCopy = address;
            }
            else
            {
                addressToCopy = matchedAddress;
            }

            addressToCopy.CopyTo(candidateAddress);

            GeocodeHelpers.SetFullAddress(candidateAddress);

            // Set locator.
            foreach (LocatorInfo locator in App.Current.Geocoder.Locators)
            {
                if (locator.Title.Equals(candidateFromLocalStorage.Address.MatchMethod,
                    System.StringComparison.OrdinalIgnoreCase) ||
                    locator.Name.Equals(candidateFromLocalStorage.Address.MatchMethod,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    candidateFromLocalStorage.Locator = locator;
                    break;
                }
            }

            candidateFromLocalStorage.AddressType = ArcGiscomGeocoder.LocalStorageAddressType;

            return candidateFromLocalStorage;
        }

        /// <summary>
        /// Execute geocoding candidates.
        /// </summary>
        /// <param name="address">Address for geocoding.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Address candidates list.</returns>
        private List<AddressCandidate> _GeocodeCandidates(Address address, bool includeDisabledLocators)
        {
            Debug.Assert(address != null);
            Debug.Assert(_geocoder != null);

            List<AddressCandidate> candidatesList = null;

            // Clean old geocoding info.
            address.MatchMethod = string.Empty;

            if (App.Current.Geocoder.AddressFormat == AddressFormat.MultipleFields)
                address.FullAddress = string.Empty;
            else
            {
                // Do nothing: do not clear full address since
                // it is used for geocoding in Single Address Field Format.
            }

            // Find all candidates.
            AddressCandidate[] candidatesArr = _geocoder.GeocodeCandidates(address, includeDisabledLocators);

            candidatesList = new List<AddressCandidate>();
            if (candidatesArr != null)
            {
                candidatesList.AddRange(candidatesArr);
            }

            return candidatesList;
        }

        /// <summary>
        /// Try to find candidates in local storage and put in appropriate position in candidates array.
        /// Leave null if candidate not found. Return addresses, which not exists in local storage.
        /// </summary>
        /// <param name="nameAddresses">Name\Address pairs to geocode.</param>
        /// <param name="candidates">Candidates list.</param>
        /// <returns>Addresses, which not exists in local storage.</returns>
        private List<Address> _ProcessLocalBatchGeocoding(NameAddress[] nameAddresses,
                                                          AddressCandidate[] candidates)
        {
            Debug.Assert(nameAddresses != null);
            Debug.Assert(candidates != null);

            List<Address> addressesToGeocode = new List<Address>();

            for (int index = 0; index < nameAddresses.Length; index++)
            {
                AddressCandidate candidate = Geocode(nameAddresses[index]);

                // If candidate exists in local storage - put it to candidates list.
                // Otherwise add address to geocode on server.
                if (candidate != null)
                {
                    candidates[index] = candidate;
                }
                else
                {
                    addressesToGeocode.Add(nameAddresses[index].Address);
                }
            }

            return addressesToGeocode;
        }

        /// <summary>
        /// Fill candidates array with geocoding results from server.
        /// </summary>
        /// <param name="candidatesFromServer">Candidates, geocoded by server batch geocoding.
        /// </param>
        /// <param name="candidates">Candidates, geocoded by local storage.</param>
        private void _ProcessServerBatchGeocoding(AddressCandidate[] candidatesFromServer,
            AddressCandidate[] candidates)
        {
            Debug.Assert(candidatesFromServer != null);
            Debug.Assert(candidates != null);

            int curIndex = 0;
            for (int candidateFromServerIndex = 0;
                candidateFromServerIndex < candidatesFromServer.Length;
                candidateFromServerIndex++)
            {
                while (curIndex < candidates.Length && candidates[curIndex] != null)
                {
                    curIndex++;
                }

                Debug.Assert(curIndex < candidates.Length);

                // If candidate geocoded - save it. Otherwise skip this position in candidates list.
                if (candidatesFromServer[candidateFromServerIndex] != null)
                {
                    candidates[curIndex] = candidatesFromServer[candidateFromServerIndex];
                }
                else
                {
                    curIndex++;
                }
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Maximum address candidate score.
        /// </summary>
        private const int MAXIMUM_SCORE = 100;

        #endregion

        #region Private fields

        /// <summary>
        /// Application server geocoder.
        /// </summary>
        private IGeocoder _geocoder;

        /// <summary>
        /// Local storage for name/address geocoding.
        /// </summary>
        private NameAddressStorage _nameAddressStorage;

        #endregion
    }
}
