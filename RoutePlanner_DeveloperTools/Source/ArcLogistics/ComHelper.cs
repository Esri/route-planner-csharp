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

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stadard HRESULT codes.
    /// </summary>
    internal sealed class HResult
    {
        public const int S_OK = 0;
        public const int E_FAIL = -2147467259;
    }

    /// <summary>
    /// ComHelper class.
    /// </summary>
    internal class ComHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static bool IsHRSucceeded(int hr)
        {
            return !IsHRFailed(hr);
        }

        public static bool IsHRFailed(int hr)
        {
            return (hr < 0);
        }

        #endregion public methods
    }
}
