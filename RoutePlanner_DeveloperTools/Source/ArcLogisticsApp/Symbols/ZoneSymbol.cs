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
    /// ZoneSymbol class
    /// </summary>
    class ZoneSymbol: TemplateMarkerSymbol
    {
        public ZoneSymbol()
        {
            ControlTemplate = _template;
        }

        static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.ZoneSymbol.xaml");
    }
}
