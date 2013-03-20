using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.Geocoding;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for MatchFoundSubPage.xaml
    /// </summary>
    internal partial class MatchFoundSubPage : Grid, IGeocodeSubPage
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        public MatchFoundSubPage(string typeName)
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(_MatchFoundSubPageLoaded);

            _typeName = typeName;
        }

        #endregion

        #region IGeocodeSubPage members

        /// <summary>
        /// Get geocode result for subpage.
        /// </summary>
        /// <param name="item">Geocoded item.</param>
        /// <param name="context">Ignored.</param>
        /// <returns>Current geocode process result.</returns>
        public string GetGeocodeResultString(IGeocodable item, object context)
        {
            if (item == null)
                return string.Empty;

            string resultFmt = (string)App.Current.FindResource(GEOCODE_RESULT_RESOURCE_NAME);

            string cityStateString = string.Empty;

            // Use both fields if both not empty. Use not empty otherwise.
            if (!string.IsNullOrEmpty(item.Address.Locality3) && !string.IsNullOrEmpty(item.Address.StateProvince))
            {
                cityStateString = string.Format(CITY_STATE_FMT, item.Address.Locality3, item.Address.StateProvince);
            }
            else if (!string.IsNullOrEmpty(item.Address.Locality3))
            {
                cityStateString = item.Address.Locality3;
            }
            else if (!string.IsNullOrEmpty(item.Address.StateProvince))
            {
                cityStateString = item.Address.StateProvince;
            }

            string inString = string.Empty;

            // If city or state present - make "in" string.
            if (!string.IsNullOrEmpty(cityStateString))
            {
                inString = string.Format((string)App.Current.FindResource(IN_ADDRESS_RESOURCE_NAME), cityStateString);
            }

            string result = string.Format(resultFmt, item.ToString(), inString);

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on subpage loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MatchFoundSubPageLoaded(object sender, RoutedEventArgs e)
        {
            MatchFoundSubPageText1.Text = string.Format((string)App.Current.FindResource(MATCH_FOUND_SUBPAGE_TEXT1), _typeName);
            MatchFoundSubPageText2.Text = (string)App.Current.FindResource(MATCH_FOUND_SUBPAGE_TEXT2);
            MatchFoundSubPageText3.Text = (string)App.Current.FindResource(MATCH_FOUND_SUBPAGE_TEXT3);

            // Last sentence is visible only for locations.
            string orderString = (string)App.Current.FindResource(ORDER_RESOURCE_NAME);
            string locationString = (string)App.Current.FindResource(LOCATION_RESOURCE_NAME);
            if (_typeName.Equals(orderString, System.StringComparison.OrdinalIgnoreCase))
            {
                MatchFoundSubPageText3.Visibility = Visibility.Collapsed;
            }
            else if (_typeName.Equals(locationString, System.StringComparison.OrdinalIgnoreCase))
            {
                MatchFoundSubPageText3.Visibility = Visibility.Visible;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        #endregion
        
        #region Private constants

        /// <summary>
        /// Resource name for format string of geocode result.
        /// </summary>
        private const string GEOCODE_RESULT_RESOURCE_NAME = "FleetSetupWizardMatchFoundFmt";

        /// <summary>
        /// Resource name for format string for "in" string.
        /// </summary>
        private const string IN_ADDRESS_RESOURCE_NAME = "FleetSetupWizardInAddressFmt";

        /// <summary>
        /// Format string in case of both city and state present.
        /// </summary>
        private const string CITY_STATE_FMT = "{0} {1}";

        /// <summary>
        /// Text 1 resource name.
        /// </summary>
        private const string MATCH_FOUND_SUBPAGE_TEXT1 = "FleetSetupWizardMatchFoundSubPageText1";

        /// <summary>
        /// Text 2 resource name.
        /// </summary>
        private const string MATCH_FOUND_SUBPAGE_TEXT2 = "FleetSetupWizardMatchFoundSubPageText2";

        /// <summary>
        /// Text 3 resource name.
        /// </summary>
        private const string MATCH_FOUND_SUBPAGE_TEXT3 = "FleetSetupWizardMatchFoundSubPageText3";

        /// <summary>
        /// Order resource name.
        /// </summary>
        private const string ORDER_RESOURCE_NAME = "Order";

        /// <summary>
        /// Location resource name.
        /// </summary>
        private const string LOCATION_RESOURCE_NAME = "Location";

        #endregion

        #region Private fields

        /// <summary>
        /// Type name.
        /// </summary>
        private string _typeName;

        #endregion
    }
}
