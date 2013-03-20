using System.Windows.Controls;
using ESRI.ArcLogistics.Geocoding;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Tools;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for CandidatesFoundSubPage.xaml
    /// </summary>
    internal partial class CandidatesFoundSubPage : Grid, IGeocodeSubPage
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        public CandidatesFoundSubPage(string typeName)
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(_CandidatesFoundSubPageLoaded);

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
            string result = string.Format(resultFmt, item.ToString());

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on subpage loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidatesFoundSubPageLoaded(object sender, RoutedEventArgs e)
        {
            CandidatesFoundSubPageText1.Text = string.Format((string)App.Current.FindResource(CANDIDATES_FOUND_SUBPAGE_TEXT1), _typeName);
            CandidatesFoundSubPageText2.Text = string.Format((string)App.Current.FindResource(CANDIDATES_FOUND_SUBPAGE_TEXT2), _typeName);
            CandidatesFoundSubPageText3.Text = string.Format((string)App.Current.FindResource(CANDIDATES_FOUND_SUBPAGE_TEXT3), _typeName);
        }
        
        #endregion

        #region Private constants

        /// <summary>
        /// Resource name for format string of geocode result.
        /// </summary>
        private const string GEOCODE_RESULT_RESOURCE_NAME = "FleetSetupWizardCandidatesFoundFmt";

        /// <summary>
        /// Text 1 resource name.
        /// </summary>
        private const string CANDIDATES_FOUND_SUBPAGE_TEXT1 = "FleetSetupWizardCandidatesFoundSubPageText1";

        /// <summary>
        /// Text 2 resource name.
        /// </summary>
        private const string CANDIDATES_FOUND_SUBPAGE_TEXT2 = "FleetSetupWizardCandidatesFoundSubPageText2";

        /// <summary>
        /// Text 3 resource name.
        /// </summary>
        private const string CANDIDATES_FOUND_SUBPAGE_TEXT3 = "FleetSetupWizardCandidatesFoundSubPageText3";

        #endregion

        #region Private fields

        /// <summary>
        /// Type name.
        /// </summary>
        private string _typeName;

        #endregion
    }
}
