using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// BarrierPolygonSymbol class.
    /// </summary>
    internal class BarrierPolygonSymbol : FillSymbol
    {
        public BarrierPolygonSymbol()
        {
            ControlTemplate = CommonHelpers.LoadTemplateFromResource(
                "ESRI.ArcLogistics.App.Symbols.BarrierPolygonSymbol.xaml");
        }
    }
}
