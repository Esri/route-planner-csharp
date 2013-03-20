using System;
using System.Collections.Generic;

using DataDynamics.ActiveReports;

namespace ESRI.ArcLogistics.App.Reports
{
    /// <summary>
    /// Class - keeper of created report state.
    /// </summary>
    internal sealed class ReportStateDescription
    {
        /// <summary>
        /// Report name.
        /// </summary>
        public string ReportName { get; set; }

        /// <summary>
        /// Source file path (*.mdb).
        /// </summary>
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Report info.
        /// </summary>
        public ReportInfo ReportInfo { get; set; }

        /// <summary>
        /// Report object.
        /// </summary>
        public ActiveReport3 Report { get; set; }

        /// <summary>
        /// Is loked state.
        /// </summary>
        public bool IsLocked { get; set; }
    }

    /// <summary>
    /// Provides data for the <see cref="ReportsGenerator.CreateReportsCompleted"/> event.
    /// </summary>
    internal sealed class CreateReportsCompletedEventArgs : EventArgs
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>CreateReportsCompletedEventArgs</c> class.
        /// </summary>
        /// <param name="error">Detected error or null.</param>
        /// <param name="cancelled">Is operation canceled flag.</param>
        /// <param name="reports">Created reports.</param>
        public CreateReportsCompletedEventArgs(Exception error,
                                               bool cancelled,
                                               ICollection<ReportStateDescription> reports)
        {
            Error = error;
            Cancelled = cancelled;
            Reports = reports;
        }

        #endregion Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Detected error.
        /// </summary>
        public readonly Exception Error;

        /// <summary>
        /// Is operation canceled flag.
        /// </summary>
        public readonly bool Cancelled;

        /// <summary>
        /// Created reports.
        /// </summary>
        public readonly ICollection<ReportStateDescription> Reports;

        #endregion // Public properties
    }

    /// <summary>
    /// Provides handler for the <see cref="ReportsGenerator.CreateReportsCompleted"/> event.
    /// </summary>
    internal delegate void CreateReportsCompletedEventHandler(Object sender,
                                                              CreateReportsCompletedEventArgs e);
}
