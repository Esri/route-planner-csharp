using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// Label Sequence symbol class
    /// </summary>
    class LabelSequenceSymbol : TemplateMarkerSymbol
    {
        public LabelSequenceSymbol()
        {
            ControlTemplate = _template;
        }

        static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.LabelSequenceSymbol.xaml");
    }
}
