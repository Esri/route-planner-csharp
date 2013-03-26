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
using System.Net;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Services
{
    /// <summary>
    /// AgsHelper class.
    /// </summary>
    internal class AgsHelper
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const int ARCGIS_UNAUTHORIZED_ACCESS = 400;
        private const int ARCGIS_EXPIRED_TOKEN = 498;
        private const int ARCGIS_INVALID_TOKEN = 499;

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static bool IsAuthServiceError(Exception ex)
        {
            WebException webEx = _GetWebException(ex);

            bool isAuthError = false;
            if (webEx != null)
            {
                if (webEx.Response != null)
                {
                    HttpStatusCode code = ((HttpWebResponse)webEx.Response).StatusCode;
                    if (code == HttpStatusCode.Unauthorized ||
                        (int)code == ARCGIS_UNAUTHORIZED_ACCESS ||
                        (int)code == ARCGIS_INVALID_TOKEN ||
                        (int)code == ARCGIS_EXPIRED_TOKEN)
                    {
                        isAuthError = true;
                    }
                }
            }

            return isAuthError;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static WebException _GetWebException(Exception ex)
        {
            WebException webEx = null;
            if (ex is WebException)
                webEx = ex as WebException;
            else
            {
                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    if (inner is WebException)
                    {
                        webEx = inner as WebException;
                        break;
                    }

                    inner = inner.InnerException;
                }
            }

            return webEx;
        }

        #endregion private methods
    }
}
