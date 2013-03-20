using System;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interface contains function for cancel editing item in Xceed grid control.
    /// </summary>
    internal interface ICancelDataObjectEditing
    {
        /// <summary>
        /// Cancels editing item in Xceed data grid.
        /// </summary>
        void CancelObjectEditing();
    }
}
