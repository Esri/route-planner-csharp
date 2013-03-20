using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Xml;
using System.Windows;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// Custom order symbol class
    /// </summary>
    internal class ClusterSymbol : TemplateMarkerSymbol
    {
        public ClusterSymbol()
        {
            ControlTemplate = _template;
        }

        static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.ClusterSymbol.xaml");

        
    }
}
