using System.Collections.Generic;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Validates geo-locations for address candidates.
    /// </summary>
    public interface ILocationValidator
    {
        /// <summary>
        /// Finds address candidates with "incorrect" locations, i.e. ones which can't be reverse
        /// geocoded using streets geocoder to the address set for candidate.
        /// </summary>
        /// <param name="candidates">The reference to the collection of address candidates to
        /// search for ones with incorrect locations at.</param>
        /// <returns>A collection of indices of candidates with incorrect locations.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="candidates"/> argument
        /// is a null reference.</exception>
        IEnumerable<int> FindIncorrectLocations(IEnumerable<AddressCandidate> candidates);
    }
}
