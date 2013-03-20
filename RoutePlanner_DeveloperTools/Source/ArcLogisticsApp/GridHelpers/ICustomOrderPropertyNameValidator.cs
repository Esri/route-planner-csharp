using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Interface provides method for Custom order property name validation.
    /// </summary>
    internal interface ICustomOrderPropertyNameValidator
    {
        /// <summary>
        /// Performs validation of custom order property name.
        /// </summary>
        /// <param name="propertyName">Custom order property name.</param>
        /// <param name="errorMessage">Output error message.</param>
        /// <returns>True - if property name is valid, otherwise - false.</returns>
        bool Validate(string propertyName, out string errorMessage);
    }
}
