using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// CommandPlugInAttribute class contains information about which pages a command must appear on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPlugInAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>CommandPlugInAttribute</c> class.
        /// </summary>
        /// <param name="categoryName">Name(s) of the page's parent category.</param>
        public CommandPlugInAttribute(params string[] categoryNames)
        {
            foreach (string categoryName in categoryNames)
                _categories.Add(categoryName);
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a collection of category names where this command will appear.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public ICollection<string> Categories
        {
            get { return _categories.AsReadOnly(); }
        }

        #endregion // Public properties

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Category names list.
        /// </summary>
        private readonly List<string> _categories = new List<string> ();

        #endregion // Private properties
    }
}
