using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// RouteLineSymbol class
    /// </summary>
    internal class RouteLineSymbol : LineSymbol
    {
        public RouteLineSymbol()
        {
            ControlTemplate = _template;
        }

        /// <summary>
        /// Load template from embedded resource by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Resource</returns>
        private static ControlTemplate _LoadTemplateFromResource(string key)
        {
            ControlTemplate ctemplate = new ControlTemplate();
            try
            {
                Stream stream = Application.Current.GetType().Assembly.GetManifestResourceStream(key);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                ctemplate = XamlReader.Load(xmlReader) as ControlTemplate;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }

            return ctemplate;
        }

        private static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.RouteLineSymbol.xaml");
    }
}
