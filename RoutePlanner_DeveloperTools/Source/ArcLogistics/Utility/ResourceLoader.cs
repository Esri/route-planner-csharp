using System.IO;
using System.Reflection;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Helper for loading resources.
    /// </summary>
    internal static class ResourceLoader
    {
        #region internal static methods

        /// <summary>
        /// Load resource file as a string.
        /// </summary>
        /// <param name="filename">File name with extension.</param>
        /// <returns>File content as string.</returns>
        internal static string ReadFileAsString(string filename)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Stream resource = currentAssembly.GetManifestResourceStream(RESOURCES_PATH + filename);
            return new StreamReader(resource).ReadToEnd();
        }

        #endregion

        #region private constants

        /// <summary>
        /// Path to the folder with resource files.
        /// </summary>
        private const string RESOURCES_PATH = @"ESRI.ArcLogistics.Resources.";

        #endregion
    }
}
