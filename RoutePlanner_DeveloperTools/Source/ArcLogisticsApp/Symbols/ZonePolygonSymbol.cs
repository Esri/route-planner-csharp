using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// ZonePolygonSymbol class.
    /// </summary>
    class ZonePolygonSymbol : FillSymbol
    {
        public ZonePolygonSymbol()
        {
            ControlTemplate = CommonHelpers.LoadTemplateFromResource(
                "ESRI.ArcLogistics.App.Symbols.ZonePolygonSymbol.xaml");
        }
    }
}
