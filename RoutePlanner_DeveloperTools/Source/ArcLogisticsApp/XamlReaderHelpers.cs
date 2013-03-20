using System.Diagnostics;
using System.IO;
using System.Windows.Markup;
using System.Xml;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Provides utility helpers for the <see cref="T:System.Windows.Markup.XamlReader"/>
    /// class.
    /// </summary>
    internal static class XamlReaderHelpers
    {
        /// <summary>
        /// Method converts the specified xaml markup to an object corresponding to
        /// the root of the markup.
        /// </summary>
        /// <param name="xaml">The string containing xaml markup to be converted
        /// to an object.</param>
        /// <returns>A new object corresponding to the root of the specified
        /// xaml markup.</returns>
        public static object Load(string xaml)
        {
            Debug.Assert(!string.IsNullOrEmpty(xaml));

            object result = null;
            using (var reader = new XmlTextReader(new StringReader(xaml)))
            {
                result = XamlReader.Load(reader);
            }

            return result;
        }
    }
}
