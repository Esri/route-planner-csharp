using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// BarrierPolylineSymbol class
    /// </summary>
    internal class BarrierPolylineSymbol : LineSymbol
    {
        public BarrierPolylineSymbol()
        {
            ControlTemplate = CommonHelpers.LoadTemplateFromResource(
                "ESRI.ArcLogistics.App.Symbols.BarrierPolylineSymbol.xaml");
        }
    }
}
