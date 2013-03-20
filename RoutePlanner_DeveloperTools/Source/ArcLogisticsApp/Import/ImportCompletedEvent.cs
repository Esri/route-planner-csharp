using System;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Event arguments for import completed event.
    /// </summary>
    internal sealed class ImportCompletedEventArgs : EventArgs
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ImportCompletedEventArgs</c> class.
        /// </summary>
        /// <param name="importedObjects">Imported imported objects (Can be empty).</param>
        /// <param name="cancelled">Operation has been canceled flag.</param>
        public ImportCompletedEventArgs(IList<AppData.DataObject> importedObjects, bool cancelled)
        {
            Debug.Assert(null != importedObjects); // created

            ImportedObjects = importedObjects;
            Cancelled = cancelled;
        }

        #endregion // Constructors

        #region Public members

        /// <summary>
        /// Imported objects.
        /// </summary>
        public readonly IList<AppData.DataObject> ImportedObjects;

        /// <summary>
        /// Operation has been canceled flag.
        /// </summary>
        public readonly bool Cancelled;

        #endregion // Public members
    }

    /// <summary>
    /// Represents the method that handles <c>ImportCompleted</c> event.
    /// </summary>
    internal delegate void ImportCompletedEventHandler(object sender, ImportCompletedEventArgs e);
}
