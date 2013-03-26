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
