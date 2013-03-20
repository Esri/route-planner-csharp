using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class, which contains result's warning messages.
    /// </summary>
    internal class GrfExportResult
    {
        #region Constructor
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public GrfExportResult()
        {
            _warnings = new List<string>();
        }

        #endregion

        #region Public property

        /// <summary>
        /// List of warning messages.
        /// </summary>
        public List<string> Warnings
        {
            get { return _warnings; }
        }
        
        #endregion

        #region Private field

        /// <summary>
        /// List of warning messages.
        /// </summary>
        private List<string> _warnings; 
        
        #endregion

    }
}
