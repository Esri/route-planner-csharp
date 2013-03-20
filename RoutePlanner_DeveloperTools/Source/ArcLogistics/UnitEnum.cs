namespace ESRI.ArcLogistics
{
    public enum Unit
    {
        // Item
        Unknown,

        // Mass
        // US
        Pound,
        Ounce,
        // SI
        Kilogram,
        Gram,

        // Volume
        // US
        CubicInch,
        CubicFoot,
        CubicYard,
        // SI
        CubicMeter,

        // FluidVolume
        // US
        Quart,
        Gallon,
        // SI
        Liter,

        // Length
        // US
        Inch,
        Foot,
        Yard,
        Mile,
        // SI
        Milimeter,
        Centimeter,
        Decimeter,
        Meter,
        Kilometer,

        // Time
        Second,
        Minute,
        Hour,
        Day,

        // Area
        // US
        SquareFoot,
        // SI
        SquareMeter,

        // Cost
        Currency,

        // Fuel Economy
        // US
        MilesPerGallon,
        // SI
        LitersPer100Kilometers,

        // Fuel Cost
        // US
        CurrencyPerGallon,
        // SI
        CurrencyPerLiter,

        // Co2 Emission
        // US
        PoundPerGallon,
        // SI
        KilogramPerLiter
    }
}
