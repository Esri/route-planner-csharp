using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Help
{
    /// <summary>
    /// Help topic class.
    /// </summary>
    public class HelpTopic
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of <c>HelpTopic</c> class.
        /// </summary>
        /// <remarks>Either path OR quickHelpString can be empty</remarks>
        public HelpTopic(string path, string quickHelpString) :
            this(path, null, quickHelpString)
        {
        }

        /// <summary>
        /// Creates a new instance of <c>HelpTopic</c> class.
        /// </summary>
        /// <remarks>path OR key OR quickHelpString can be empty</remarks>
        internal HelpTopic(string path, string key, string quickHelpString)
        {
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(key) && string.IsNullOrEmpty(quickHelpString))
                throw new ArgumentException();

            _path = path;
            _key = key;
            _quickHelp = quickHelpString;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Help path.
        /// </summary>
        /// <remarks>Can be null</remarks>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Help topic key.
        /// </summary>
        /// <remarks>Can be null</remarks>
        internal string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Quick help string.
        /// </summary>
        /// <remarks>Can be null</remarks>
        public string QuickHelpText
        {
            get { return _quickHelp; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _path = null;
        private string _key = null;
        private string _quickHelp = null;

        #endregion // Private members
    }
}
