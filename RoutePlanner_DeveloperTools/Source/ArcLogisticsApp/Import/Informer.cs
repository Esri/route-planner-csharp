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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class for generation import process statistic.
    /// </summary>
    internal sealed class Informer
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>Informer</c> class.
        /// </summary>
        public Informer()
        {}

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Showes process results with details in message window.
        /// </summary>
        /// <param name="importer">Importer object.</param>
        /// <param name="geocoder">Geocoder object (can be NULL).</param>
        /// <param name="storage">Storage object</param>
        /// <param name="status">Status text.</param>
        public void Inform(Importer importer, Geocoder geocoder, Storage storage, string status)
        {
            Debug.Assert(!string.IsNullOrEmpty(status)); // not empty
            Debug.Assert(null != importer); // created
            Debug.Assert(null != storage); // created

            var details = new List<MessageDetail>();

            // add statistic
            string statistic = _CreateStatistic(importer, geocoder, storage);
            Debug.Assert(!string.IsNullOrEmpty(statistic));
            var statisticDetail = new MessageDetail(MessageType.Information, statistic);
            details.Add(statisticDetail);

            // add geocoder exception
            if ((null != geocoder) &&
                (null != geocoder.Exception))
            {
                details.Add(_GetServiceMessage(geocoder.Exception));
            }

            // add steps deatils
            details.AddRange(importer.Details);
            if (null != geocoder)
                details.AddRange(geocoder.Details);
            details.AddRange(storage.Details);

            // show status with details
            App.Current.Messenger.AddMessage(status, details);
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets service message as detail.
        /// </summary>
        /// <param name="ex">Service exception to conversion.</param>
        /// <returns>Created message detail.</returns>
        private MessageDetail _GetServiceMessage(Exception ex)
        {
            Debug.Assert(null != ex); // created

            App currentApp = App.Current;
            string service = currentApp.FindString("ServiceNameGeocoding");

            MessageDetail detail = null;

            var exCommunication = ex as CommunicationException;
            if (null == exCommunication)
            {   // other exception
                var exAuthentication = ex as AuthenticationException;
                if (null == exAuthentication)
                {   // all other exception
                    Debug.Assert(false); // not supported
                    Logger.Error(ex);

                    detail = new MessageDetail(MessageType.Warning, ex.Message);
                }
                else
                {   // authentication exception
                    string message = currentApp.GetString("ServiceAuthError",
                                                          service,
                                                          exAuthentication.ServiceName);
                    // create link to license page
                    var link = new Link(currentApp.FindString("LicencePanelText"),
                                        Pages.PagePaths.LicensePagePath,
                                        LinkType.Page);
                    detail = new MessageDetail(MessageType.Warning, message, link);
                }
            }
            else
            {   // communication exception
                string message =
                    CommonHelpers.FormatServiceCommunicationError(service, exCommunication);
                detail = new MessageDetail(MessageType.Warning, message);
            }

            return detail;
        }

        /// <summary>
        /// Gets imported objects count.
        /// </summary>
        /// <param name="importer">Importer.</param>
        /// <returns>Imported objects count.</returns>
        private int _GetImportedObjsCount(Importer importer)
        {
            Debug.Assert(null != importer); // created
            Debug.Assert(null != importer.ImportedObjects); // created

            int importedCount = importer.ImportedObjects.Count;
            return importedCount;
        }

        /// <summary>
        /// Gets statistic string.
        /// </summary>
        /// <param name="processRscName">Process resource name.</param>
        /// <param name="count">Count elements for statictic.</param>
        /// <returns>Statistic string in predefined format.</returns>
        private string _GetStatisticText(string processRscName, int count)
        {
            Debug.Assert(!string.IsNullOrEmpty(processRscName)); // not empty

            App currentApp = App.Current;

            // get statistic format
            string format = currentApp.FindString("ImportProcessStatisticFormat");

            // statistic text
            string text = string.Format(format, count, currentApp.FindString(processRscName));
            return text;
        }

        /// <summary>
        /// Gets statistic string as statistic element.
        /// </summary>
        /// <param name="processRscName">Process resource name.</param>
        /// <param name="count">Count elements for statictic.</param>
        /// <returns>Statistic string in selected format with element separator.</returns>
        private string _GetStatisticElement(string processRscName, int count)
        {
            // statistic text
            string text = _GetStatisticText(processRscName, count);
            // decorate as element of statistic
            string result = string.Format(STATISTIC_ELEMENT_FORMAT, text);
            return result;
        }

        /// <summary>
        /// Gets geocode objects statistic element text.
        /// </summary>
        /// <param name="geocoder">Reference to geocoder object.</param>
        /// <param name="importedCount">Count of imported objects.</param>
        /// <returns>Geocoded objects statistic element text.</returns>
        private string _GetGeocodeStatisticElementText(Geocoder geocoder, int importedCount)
        {
            Debug.Assert(null != geocoder); // inited

            int geocodedCount = geocoder.GeocodedCount;

            string text = _GetStatisticElement("ImportProcessStatusGeoded", geocodedCount);
            var sb = new StringBuilder(text);

            if (geocodedCount != importedCount)
            {   // append ungeocoded objects text
                sb.Append(_GetStatisticElement("ImportProcessStatusUngeoded",
                                               importedCount - geocodedCount));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets import objects statistic etended element text.
        /// </summary>
        /// <param name="importer">Importer.</param>
        /// <returns>Import objects statistic etended element text or empty string.</returns>
        private string _GetImportStatisticExtElementText(Importer importer)
        {
            Debug.Assert(null != importer); // created

            var sb = new StringBuilder();

            if (0 < importer.FailedCount)
            {   // append failed objects text
                string text =
                    _GetStatisticElement("ImportProcessStatisticFailed", importer.FailedCount);
                sb.Append(text);
            }

            if (0 < importer.SkippedCount)
            {   // append skipped objects text
                string text = 
                    _GetStatisticElement("ImportProcessStatisticSkipped", importer.SkippedCount);
                sb.Append(text);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates statictic message.
        /// </summary>
        /// <param name="importer">Importer object.</param>
        /// <param name="geocoder">Geocoder object (can be NULL).</param>
        /// <param name="storage">Storage object</param>
        /// <returns>Process statictic description.</returns>
        private string _CreateStatistic(Importer importer, Geocoder geocoder, Storage storage)
        {
            Debug.Assert(null != importer); // created
            Debug.Assert(null != storage); // created

            // append totals text
            var sb = new StringBuilder(App.Current.FindString("ImportProcessStatisticTotals"));

            // append readed record from source text
            sb.Append(_GetStatisticText("ImportProcessStatisticRead", importer.RecordCount));

            // append imported objects text
            Debug.Assert(null != importer); // inited
            int importedCount = _GetImportedObjsCount(importer);
            sb.Append(_GetStatisticElement("ImportProcessStatisticImported", importedCount));

            if (0 < importedCount)
            {   // append imported objects text
                Debug.Assert(null != storage); // created

                // append import extended text
                string text = _GetImportStatisticExtElementText(importer);
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                }

                // append valid objects text
                text = _GetStatisticElement("ImportProcessStatisticValid", storage.ValidCount);
                sb.Append(text);

                // append geocoding objects text
                if (null != geocoder)
                {   // append geocoded objects text
                    sb.Append(_GetGeocodeStatisticElementText(geocoder, importedCount));
                }

                // append added objects text
                int createdCount = storage.CreatedCount;
                sb.Append(_GetStatisticElement("ImportProcessStatisticAdded", createdCount));
                // append updated objects text
                int updatedCount = importedCount - createdCount;
                sb.Append(_GetStatisticElement("ImportProcessStatisticUpdated", updatedCount));
            }

            return sb.ToString();
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Statictic element format (with separator).
        /// </summary>
        private const string STATISTIC_ELEMENT_FORMAT = ", {0}";

        #endregion // Private constants
    }
}
