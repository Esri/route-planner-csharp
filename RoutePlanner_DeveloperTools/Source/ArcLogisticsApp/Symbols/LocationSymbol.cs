using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// LocationSymbol class
    /// </summary>
    internal class LocationSymbol : TemplateMarkerSymbol
    {
        public LocationSymbol()
        {
            ControlTemplate = _template;
        }

        static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.LocationSymbol.xaml");
    }
}
