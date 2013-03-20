using System;
using System.Linq;
using System.Collections.Generic;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents a new ArcGis geocoder.
    /// </summary>
    public class ArcGiscomGeocoder : Geocoder
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="geocodingServiceInfo">Geocoding service info.</param>
        /// <param name="geocodeServer">Geocode server.</param>
        /// <param name="exceptionHandler">Exception handler.</param>
        /// <exception cref="System.ArgumentException">Is thrown in case if geocodingServiceInfo
        /// or geocodeServer parameters is null.</exception>
        internal ArcGiscomGeocoder(GeocodingServiceInfo geocodingServiceInfo,
            AgsServer geocodeServer, IServiceExceptionHandler exceptionHandler)
            : base(geocodingServiceInfo, geocodeServer, exceptionHandler)
        {
            if (geocodingServiceInfo == null)
                throw new ArgumentException("geocodingServiceInfo");
            if(geocodeServer == null)
                throw new ArgumentException("geocodeServer");

            var list = new List<string>();

            // Fill collection with names of exact locators types.
            foreach (var locator in geocodingServiceInfo.ExactLocators.Locators)
                list.Add(locator.Type);

            // Local storage is exact locator.
            list.Add(LOCAL_STORAGE_ADDRESS_TYPE);

            ExactLocatorsTypesNames = list;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Name of address types for points from local storage.
        /// </summary>
        public static string LocalStorageAddressType
        {
            get
            {
                return LOCAL_STORAGE_ADDRESS_TYPE;
            }
        }

        /// <summary>
        /// Collection with names of exact locators.
        /// </summary>
        public IEnumerable<string> ExactLocatorsTypesNames { get; private set; }

        #endregion

        #region Constants

        /// <summary>
        /// Name of address types for points from local storage.
        /// </summary>
        private const string LOCAL_STORAGE_ADDRESS_TYPE = "User selected";

        #endregion

    }
}
