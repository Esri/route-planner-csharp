using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Markup;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows;

namespace ESRI.ArcLogistics.App.Symbols
{
    internal class TemplateMarkerSymbol : MarkerSymbol
    {
        /// <summary>
        /// Load template from embedded resource by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Resource</returns>
        protected static ControlTemplate _LoadTemplateFromResource(string key)
        {
            ControlTemplate controlTemplate = null;
            try
            {
                Stream stream = Application.Current.GetType().Assembly.GetManifestResourceStream(key);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                controlTemplate = XamlReader.Load(xmlReader) as ControlTemplate;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            return controlTemplate;
        }
    }
}
