using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interface for geocode subpage.
    /// </summary>
    interface IGeocodeSubPage
    {
        /// <summary>
        /// Get geocode result for subpage.
        /// </summary>
        /// <param name="item">Geocoded item.</param>
        /// <param name="context">Ignored.</param>
        /// <returns>Current geocode process result.</returns>
        string GetGeocodeResultString(IGeocodable item, object context);
    }
}
