using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using DataDynamics.ActiveReports.Export;
using DataDynamics.ActiveReports.Export.Pdf;
using DataDynamics.ActiveReports.Export.Rtf;
using DataDynamics.ActiveReports.Export.Xls;
using DataDynamics.ActiveReports.Export.Html;
using DataDynamics.ActiveReports.Export.Tiff;
using DataDynamics.ActiveReports.Export.Text;

namespace ESRI.ArcLogistics.App.Reports
{
    /// <summary>
    /// Class contains helper method for reports.
    /// </summary>
    internal static class ReportsHelpers
    {
        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default selected type index.
        /// </summary>
        public static int DefaultSelectedTypeIndex
        {
            get { return DEFAULT_SELECTED_INDEX; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Assemblyes file dialog filter.
        /// </summary>
        /// <returns>Created file dialog filter.</returns>
        public static string AssemblyDialogFilter()
        {
            var filter = new StringBuilder();
            for (int index = 0; index < EXPORT_TYPES.Length; ++index)
            {
                if (0 < filter.Length)
                    filter.Append('|');

                ExportType type = EXPORT_TYPES[index];
                filter.AppendFormat("{0}|*{1}", type.Name, type.FileExtension);
            }

            return filter.ToString();
        }

        /// <summary>
        /// Gets export type names.
        /// </summary>
        /// <returns>Supported export type names.</returns>
        public static ICollection<string> GetExportTypeNames()
        {
            var names = new List<string> (EXPORT_TYPES.Length);
            for (int index = 0; index < EXPORT_TYPES.Length; ++index)
            {
                ExportType type = EXPORT_TYPES[index];
                names.Add(type.Name);
            }

            return names;
        }

        /// <summary>
        /// Gets export file extension by index
        /// </summary>
        /// <param name="index">Selecetd index.</param>
        /// <returns></returns>
        public static string GetFileExtension(int index)
        {
            Debug.Assert((0 <= index) && (index < EXPORT_TYPES.Length));

            ExportType type = EXPORT_TYPES[index];
            return type.FileExtension;
        }

        /// <summary>
        /// Gets export file extension by name.
        /// </summary>
        /// <param name="name">Export type name.</param>
        /// <returns>File extension for export type.</returns>
        public static string GetFileExtensionByName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            string fileExtension = null;
            for (int index = 0; index < EXPORT_TYPES.Length; ++index)
            {
                ExportType type = EXPORT_TYPES[index];
                if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = type.FileExtension;
                    break; // result founded
                }
            }
            Debug.Assert(!string.IsNullOrEmpty(fileExtension));

            return fileExtension;
        }

        /// <summary>
        /// Gets exporter object by name.
        /// </summary>
        /// <param name="name">Export type name.</param>
        /// <returns>Unifed document exporter.</returns>
        public static IDocumentExport GetExporterByName(string name)
        {
            return _GetExporter(name);
        }

        /// <summary>
        /// Gets exporter object by file extension.
        /// </summary>
        /// <param name="fileExtension">Export file extension.</param>
        /// <returns>Unifed document exporter.</returns>
        public static IDocumentExport GetExporterByFileExtension(string fileExtension)
        {
            return _GetExporter(fileExtension);
        }

        #endregion // Public methods

        #region Private types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Structure keep reports export type information.
        /// </summary>
        private struct ExportType
        {
            /// <summary>
            /// Creates a new instance of the <c>ExportType</c>.
            /// </summary>
            /// <param name="name">Export type name.</param>
            /// <param name="extension">Export file extension.</param>
            public ExportType(string name, string extension)
            {
                Debug.Assert(!string.IsNullOrEmpty(name));
                Debug.Assert(!string.IsNullOrEmpty(extension));

                Name = name;
                FileExtension = extension;
            }

            /// <summary>
            /// Export type name.
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// Export file extension.
            /// </summary>
            public readonly string FileExtension;
        }

        #endregion // Private types

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets exporter object by identificator.
        /// </summary>
        /// <param name="identificator">Export type identificator (name or file extension).</param>
        /// <returns>Unifed document exporter.</returns>
        private static IDocumentExport _GetExporter(string identificator)
        {
            Debug.Assert(!string.IsNullOrEmpty(identificator));

            IDocumentExport export = null;
            // select export format type
            switch (identificator)
            {
                case EXPORT_EXTENSION_HTM:
                case EXPORT_TYPE_NAME_HTM:
                    export = new HtmlExport();
                    break;

                case EXPORT_EXTENSION_PDF:
                case EXPORT_TYPE_NAME_PDF:
                    export = new PdfExport();
                    break;

                case EXPORT_EXTENSION_RTF:
                case EXPORT_TYPE_NAME_RTF:
                    export = new RtfExport();
                    break;

                case EXPORT_EXTENSION_TIF:
                case EXPORT_TYPE_NAME_TIF:
                    export = new TiffExport();
                    break;

                case EXPORT_EXTENSION_TXT:
                case EXPORT_TYPE_NAME_TXT:
                    export = new TextExport();
                    break;

                case EXPORT_EXTENSION_XLS:
                case EXPORT_TYPE_NAME_XLS:
                    export = new XlsExport();
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return export;
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // export type names
        private const string EXPORT_TYPE_NAME_PDF = "Portable Document Format (PDF)";
        private const string EXPORT_TYPE_NAME_HTM = "Html Format (HTM)";
        private const string EXPORT_TYPE_NAME_RTF = "Rich Text Format (RTF)";
        private const string EXPORT_TYPE_NAME_TIF = "TIFF Format (TIF)";
        private const string EXPORT_TYPE_NAME_TXT = "Text Format (TXT)";
        private const string EXPORT_TYPE_NAME_XLS = "Microsoft Excel (XLS)";

        // export file extension
        private const string EXPORT_EXTENSION_PDF = ".pdf";
        private const string EXPORT_EXTENSION_HTM = ".htm";
        private const string EXPORT_EXTENSION_RTF = ".rtf";
        private const string EXPORT_EXTENSION_TIF = ".tif";
        private const string EXPORT_EXTENSION_TXT = ".txt";
        private const string EXPORT_EXTENSION_XLS = ".xls";

        /// <summary>
        /// Default selected type index.
        /// </summary>
        private const int DEFAULT_SELECTED_INDEX = 0;

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Supported reports export types.
        /// </summary>
        private static ExportType[] EXPORT_TYPES = new ExportType[]
        {
            new ExportType(EXPORT_TYPE_NAME_PDF, EXPORT_EXTENSION_PDF),
            new ExportType(EXPORT_TYPE_NAME_HTM, EXPORT_EXTENSION_HTM),
            new ExportType(EXPORT_TYPE_NAME_RTF, EXPORT_EXTENSION_RTF),
            new ExportType(EXPORT_TYPE_NAME_TIF, EXPORT_EXTENSION_TIF),
            new ExportType(EXPORT_TYPE_NAME_TXT, EXPORT_EXTENSION_TXT),
            new ExportType(EXPORT_TYPE_NAME_XLS, EXPORT_EXTENSION_XLS)
        };

        #endregion // Private fields
    }
}
