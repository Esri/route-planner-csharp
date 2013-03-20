namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Specifies the type of the sub-locator.
    /// </summary>
    public enum SublocatorType
    {
        /// <summary>
        /// Denotes address point sub-locator.
        /// </summary>
        AddressPoint,

        /// <summary>
        /// A sub-locator placing geocoded coordinates on streets.
        /// </summary>
        Streets,

        /// <summary>
        /// Denotes sub-locator using ZIP code address parts for geocoding.
        /// </summary>
        Zip,

        /// <summary>
        /// Denotes sub-locator using City, State and/or Province address parts for geocoding.
        /// </summary>
        CityState,
    }
}
