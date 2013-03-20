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

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// Candidate symbol class
    /// </summary>
    class CandidateSymbol : TemplateMarkerSymbol
    {
        public CandidateSymbol()
        {
            ControlTemplate = _template;
        }

        static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.CandidateSymbol.xaml");
    }
}
