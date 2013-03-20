using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// WidgetPlugInAttribute class contains information about which pages a widget must appear on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WidgetPlugInAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>WidgetPlugInAttribute</c> class.
        /// </summary>
        public WidgetPlugInAttribute(params string[] pagePaths)
        {
            foreach (string path in pagePaths)
                _pagePaths.Add(path);
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns list of page paths where this widget should appear.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public ICollection<string> PagePaths
        {
            get { return _pagePaths.AsReadOnly(); }
        }

        #endregion // Public properties

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page paths list
        /// </summary>
        private readonly List<string> _pagePaths = new List<string> ();

        #endregion // Private properties
    }
}
