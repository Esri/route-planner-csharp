/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for CandidatesNotFoundSubPage.xaml
    /// </summary>
    internal partial class CandidatesNotFoundSubPage : Grid, IGeocodeSubPage
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        public CandidatesNotFoundSubPage(string typeName)
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(_CandidatesNotFoundSubPageLoaded);

            _typeName = typeName;
        }

        #endregion

        #region IGeocodeSubPage members

        /// <summary>
        /// Get geocode result for subpage.
        /// </summary>
        /// <param name="item">Geocoded item.</param>
        /// <param name="context">Candidates to zoom.</param>
        /// <returns>Current geocode process result.</returns>
        public string GetGeocodeResultString(IGeocodable item, object context)
        {
            if (item == null)
                return string.Empty;

            AddressCandidate[] candidatesToZoom = context as AddressCandidate[];

            string result;
            if (candidatesToZoom == null || candidatesToZoom.Length == 0)
            {
                result = _MakeNotFoundString(item);
            }
            else
            {
                result = _MakeNotFoundButZoomedString(item, candidatesToZoom[0]);
            }

            return result;
        }

        /// <summary>
        /// Make string like 'Bevmo could not be found in Redlands,CA.'
        /// </summary>
        /// <param name="item">Geocodable item.</param>
        /// <returns>Geocode result string.</returns>
        private string _MakeNotFoundString(IGeocodable item)
        {
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

        /// <summary>
        /// Make string like 'Bevmo couldn’t be located, we’ve zoomed to Pat’s Ranch Rd.  Use the pushpin tool to locate exactly.'
        /// If street candidate - use Address Line from geocodable item, otherwise(citystate or zip) use full address.
        /// </summary>
        /// <param name="item">Geocodable item.</param>
        /// <param name="candidateToZoom">First candidate to zoom.</param>
        /// <returns>Geocode result string.</returns>
        private string _MakeNotFoundButZoomedString(IGeocodable item, AddressCandidate candidateToZoom)
        {
            string messageFmt = (string)App.Current.FindResource(GEOCODE_RESULT_ZOOMED_RESOURCE_NAME);

            StringBuilder stringBuilder = new StringBuilder();

            // Add "Not located, but zoomed to..." string.
            stringBuilder.AppendFormat(messageFmt, item.ToString());

            // Add zoomed address.
            string zoomedAddress = GeocodeHelpers.GetZoomedAddress(item, candidateToZoom);
            stringBuilder.Append(zoomedAddress);

            // Add dot at the end of sentence.
            stringBuilder.Append(MESSAGE_STRING_PARTS_DELIMETER);

            // Add "Use pushpin" string.
            stringBuilder.Append((string)App.Current.FindResource(USE_PUSHPIN_RESOURCE_NAME));

            string result = stringBuilder.ToString();

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on subpage loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidatesNotFoundSubPageLoaded(object sender, RoutedEventArgs e)
        {
            CandidatesNotFoundSubPageText1.Text = string.Format((string)App.Current.FindResource(CANDIDATES_NOT_FOUND_SUBPAGE_TEXT1), _typeName);
            CandidatesNotFoundSubPageText2.Text = (string)App.Current.FindResource(CANDIDATES_NOT_FOUND_SUBPAGE_TEXT2);
            CandidatesNotFoundSubPageText3.Text = (string)App.Current.FindResource(CANDIDATES_NOT_FOUND_SUBPAGE_TEXT3);
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Resource name for format string of geocode result.
        /// </summary>
        private const string GEOCODE_RESULT_RESOURCE_NAME = "FleetSetupWizardCandidatesNotFoundFmt";

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
        private const string CANDIDATES_NOT_FOUND_SUBPAGE_TEXT1 = "FleetSetupWizardCandidatesNotFoundSubPageText1";

        /// <summary>
        /// Text 2 resource name.
        /// </summary>
        private const string CANDIDATES_NOT_FOUND_SUBPAGE_TEXT2 = "FleetSetupWizardCandidatesNotFoundSubPageText2";

        /// <summary>
        /// Text 3 resource name.
        /// </summary>
        private const string CANDIDATES_NOT_FOUND_SUBPAGE_TEXT3 = "FleetSetupWizardCandidatesNotFoundSubPageText3";

        /// <summary>
        /// Resource name for format string of zoomed geocode result.
        /// </summary>
        private const string GEOCODE_RESULT_ZOOMED_RESOURCE_NAME = "FleetSetupWizardCandidatesNotFoundButZoomedFmt";

        /// <summary>
        /// Resource name for "Use pushpin..." string.
        /// </summary>
        private const string USE_PUSHPIN_RESOURCE_NAME = "FleetSetupWizardUsePushpinText";

        /// <summary>
        /// Delimeter string to concatenate message.
        /// </summary>
        private const string MESSAGE_STRING_PARTS_DELIMETER = ". ";

        #endregion

        #region Private fields

        /// <summary>
        /// Type name.
        /// </summary>
        private string _typeName;

        #endregion
    }
}
