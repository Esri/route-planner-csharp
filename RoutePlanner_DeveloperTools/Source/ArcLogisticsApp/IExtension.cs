using System;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implement this interface to create extension for ArcLogistics
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// Initialize extension with the instance of application.
        /// </summary>
        /// <param name="app">Application instance.</param>
        void Initialize(App app);

        /// <summary>
        /// Returns extension name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns extension description.
        /// </summary>
        string Description { get; }
    }
}
