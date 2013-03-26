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
using System.Collections;
using System.Collections.Generic;

using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// ReportTemplatesDetailDescription class
    /// </summary>
    internal class ReportTemplatesDetailDescription : DataGridDetailDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ReportTemplatesDetailDescription()
        {
            RelationName = "ReportTemplateDetails";
        }

        #endregion // Constructors

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SelectReportWrapper MasterReportWrapper
        {
            get { return _report; }
        }

        #endregion // Public members

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override IEnumerable GetDetailsForParentItem(DataGridCollectionViewBase parentCollectionView, object parentItem)
        {
            Debug.Assert(parentItem is SelectReportWrapper);

            IEnumerable subReports = null;
            if (null == parentItem)
                subReports = new List<SelectReportWrapper>();
            else
            {
                _report = parentItem as SelectReportWrapper;
                subReports = _report.SubReportWrappers;
            }

            return subReports;
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SelectReportWrapper _report = null;

        #endregion // Private members
    }
}
