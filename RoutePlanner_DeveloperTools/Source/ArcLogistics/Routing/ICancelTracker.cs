/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
*/

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// ICancelTracker interface.
    /// </summary>
    internal interface ICancelTracker
    {
        #region properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a boolean value indicating whether an operation was cancelled.
        /// </summary>
        bool IsCancelled
        {
            get;
        }

        #endregion properties
    }
}
