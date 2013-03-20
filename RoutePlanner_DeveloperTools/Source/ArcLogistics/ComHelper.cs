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
