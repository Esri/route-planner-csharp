using System;
using System.Text;
using System.Diagnostics;
using System.Drawing.Printing;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Printer settings.
    /// </summary>
    internal class PrinterSettingsStore
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public PrinterSettingsStore()
        {
            _Init();
        }

        #endregion // Constructor

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Seleted printer name
        /// </summary>
        /// <remarks>Can be empty, if in system not present printers, or default not selected.</remarks>
        public string PrinterName
        {
            get { return _printerName; }
        }

        /// <summary>
        /// Page settings
        /// </summary>
        /// <remarks>Can be null. Valide only if PrinterName valid.</remarks>
        public PageSettings PageSettings
        {
            get { return _pageSettings; }
        }

        #endregion // Public members

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void StoreSetting(string printerName)
        {
            Debug.Assert(!string.IsNullOrEmpty(printerName));

            StoreSetting(printerName, null);
        }

        public void StoreSetting(string printerName, PageSettings settings)
        {
            Debug.Assert(!string.IsNullOrEmpty(printerName));
            _printerName = printerName;

            if (null == settings)
                _InitPageSettings(_printerName);
            else
                _pageSettings = (PageSettings)settings.Clone();
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _InitPageSettings(string printerName)
        {
            Debug.Assert(!string.IsNullOrEmpty(printerName));

            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printerName;
            _pageSettings = (PageSettings)pd.PrinterSettings.DefaultPageSettings.Clone();
        }

        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "GetDefaultPrinter")]
        public static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int pcchBuffer);

        private void _Init()
        {
            // NOTE: only once
            Debug.Assert(string.IsNullOrEmpty(_printerName));
            Debug.Assert(null == _pageSettings);

            try
            {
                int length = 256;
                System.Text.StringBuilder name = new System.Text.StringBuilder(length);
                GetDefaultPrinter(name, ref length);
                _printerName = name.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (!string.IsNullOrEmpty(_printerName))
               _InitPageSettings(_printerName);
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _printerName = null;
        private PageSettings _pageSettings = null;

        #endregion // Private members
    }
}
