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
using System.ComponentModel;

using AppPages = ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class contains set status parameters.
    /// </summary>
    internal sealed class SetStatusParams
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>SetStatusParams</c> class.
        /// </summary>
        /// <param name="statusNameRsc">Name of resource for status text.</param>
        public SetStatusParams(string statusNameRsc)
        {
            _statusNameRsc = statusNameRsc;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of resource for status text.
        /// </summary>
        public string StatusNameRsc
        {
            get { return _statusNameRsc; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of resource for status text.
        /// </summary>
        private string _statusNameRsc;

        #endregion // Private members
    }

    /// <summary>
    /// Class that implements progress inforation tracker for import.
    /// </summary>
    internal sealed class ProgressInfoTracker : IProgressInformer
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates and initializes a new instance of the <c>ProgressInfoTracker</c> class.
        /// </summary>
        /// <param name="worker">Background worker.</param>
        /// <param name="parentPage">Parent page for status panel.</param>
        /// <param name="objectName">Object name to progress message generation.</param>
        /// <param name="objectsName">Objects name to status message generation.</param>
        public ProgressInfoTracker(BackgroundWorker worker,
                                   AppPages.Page parentPage,
                                   string objectName,
                                   string objectsName)
        {
            Debug.Assert(!string.IsNullOrEmpty(objectsName)); // not empty
            Debug.Assert(!string.IsNullOrEmpty(objectName)); // not empty
            Debug.Assert(null != parentPage); // created
            Debug.Assert(null != worker); // created

            _parentPage = parentPage;
            _objectName = objectName;
            _objectsName = objectsName;
            _worker = worker;
        }

        #endregion // Constructors

        #region IProgressInformer interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Import object face name.
        /// </summary>
        public string ObjectName
        {
            get { return _objectName; }
        }

        /// <summary>
        /// Import objects face name.
        /// </summary>
        public string ObjectsName
        {
            get { return _objectsName; }
        }

        /// <summary>
        /// Parent page for status panel.
        /// </summary>
        public AppPages.Page ParentPage
        {
            get { return _parentPage; }
        }

        /// <summary>
        /// Sets status message. Hide progress bar and button.
        /// </summary>
        /// <param name="statusNameRsc">Name of resource for status text.</param>
        public void SetStatus(string statusNameRsc)
        {
            var param = new SetStatusParams(statusNameRsc);
            _ReportProgress(param);
        }

        #endregion // IProgressInformer interface members

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reports progress.
        /// </summary>
        /// <param name="param">Report parameters.</param>
        private void _ReportProgress(object param)
        {
            Debug.Assert(null != param); // created

            _worker.ReportProgress(0, param);
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parent page for status panel.
        /// </summary>
        private AppPages.Page _parentPage;
        /// <summary>
        /// Import objects name to status message generation.
        /// </summary>
        private string _objectsName;
        /// <summary>
        /// Import object name to progress message generation.
        /// </summary>
        private string _objectName;
        /// <summary>
        /// Background worker to report progress
        /// </summary>
        private BackgroundWorker _worker;

        #endregion // Private members
    }
}
