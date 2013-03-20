using System.Diagnostics;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Convert factors.
    /// </summary>
    internal sealed class ConvertFactors
    {
        public const double KILOMETERS_PER_MILE = 1.609344;

        public const double GRAMMES_PER_KILOGRAM = 1000;
        public const double OUNCES_PER_POUND = 16;
        public const double GRAMMES_PER_OUNCE = 28.34952;
        public const double POUND_PER_KILOGRAM = 2.204623;

        public const double METER2_PER_FOOT2 = 0.09290341;

        public const double MILIMETERS_PER_INCH = 25.4;
        public const double MILIMETERS_PER_CENTIMETER = 10;
        public const double CENTIMETERS_PER_DECIMETER = 10;
        public const double DECIMETERS_PER_METER = 10;
        public const double METERS_PER_KILOMETER = 1000;
        public const double INCHES_PER_FOOT = 12;
        public const double FOOTS_PER_YARD = 3;
        public const double YARDS_PER_MILE = 1760;

        public const double SECONDS_PER_MINUTE = 60;
        public const double MINUTES_PER_HOUR = 60;
        public const double HOURS_PER_DAY = 24;

        public const double INCHES3_PER_FOOT3 = 0.000578703764;
        public const double FOOTS3_PER_YARD3 = 0.037037037;

        public const double INCHES3_PER_METER3 = 1.6387064E-5;

        public const double INCHES3_PER_QUART = 1 / 57.75;

        public const double METER3_PER_LITER = 1000;

        public const double LITERS_PER_GALLON_US = 3.785411;
        public const double QUARTS_PER_GALLON = 4;
        public const double LITERS_PER_QUART_US = QUARTS_PER_GALLON / LITERS_PER_GALLON_US;

        public const double GPM_PER_L100KM_US = LITERS_PER_GALLON_US / KILOMETERS_PER_MILE;
        public const double L100KM_PER_GPM_US = GPM_PER_L100KM_US * 100;
    };

    /// <summary>
    /// Length Converter
    /// </summary>
    internal class LengthConvertor
    {
        #region US conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public enum UnitUS
        {
            Inch = 0,
            Foot = 1,
            Yard = 2,
            Mile = 3
        }

        static public double ConvertUS(double value, UnitUS fromUnit, UnitUS toUnit)
        {
            Debug.Assert(fromUnit != toUnit);
            return _Convert((int)fromUnit, (int)toUnit, FACTORS_US, value);
        }

        #endregion // US conversion

        #region SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal enum UnitSI
        {
            Milimeter = 0,
            Centimeter = 1,
            Decimeter = 2,
            Meter = 3,
            Kilometer = 4
        }

        internal static double ConvertSI(double value, UnitSI fromUnit, UnitSI toUnit)
        {
            Debug.Assert(fromUnit != toUnit);
            return _Convert((int)fromUnit, (int)toUnit, FACTORS_SI, value);
        }

        #endregion // SI conversion

        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double InchesToMilimeters(double value)
        {
            return (value * ConvertFactors.MILIMETERS_PER_INCH);
        }

        static public double MilimetersToInches(double value)
        {
            return (value / ConvertFactors.MILIMETERS_PER_INCH);
        }

        #endregion // US\SI conversion

        #region Helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static private double _Convert(int from, int to, double[] factors, double value)
        {
            double convertedValue = value;
            if (to < from)
            {
                for (int index = from; index > to; --index)
                    convertedValue *= factors[index];
            }
            else
            {
                for (int index = from + 1; index <= to; ++index)
                    convertedValue /= factors[index];
            }

            return convertedValue;
        }

        #endregion // Helpers

        #region Factors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static readonly double[] FACTORS_SI = new double[] { 1,
                                                                     ConvertFactors.MILIMETERS_PER_CENTIMETER,
                                                                     ConvertFactors.CENTIMETERS_PER_DECIMETER,
                                                                     ConvertFactors.DECIMETERS_PER_METER,
                                                                     ConvertFactors.METERS_PER_KILOMETER };
        private static readonly double[] FACTORS_US = new double[] { 1,
                                                                     ConvertFactors.INCHES_PER_FOOT,
                                                                     ConvertFactors.FOOTS_PER_YARD,
                                                                     ConvertFactors.YARDS_PER_MILE };

        #endregion // Factors
    }

    /// <summary>
    /// FluidVolume Converter
    /// </summary>
    internal class FluidVolumeConvertor
    {
        #region US conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double QuartsToGalons(double value)
        {
            return (value / ConvertFactors.QUARTS_PER_GALLON);
        }

        static public double GalonsToQuarts(double value)
        {
            return (value * ConvertFactors.QUARTS_PER_GALLON);
        }

        #endregion // US conversion

        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double LitersToQuarts(double value)
        {
            return (value * ConvertFactors.LITERS_PER_QUART_US);
        }

        static public double QuartsToLiters(double value)
        {
            return (value / ConvertFactors.LITERS_PER_QUART_US);
        }

        #endregion // US\SI conversion
    }

    /// <summary>
    /// Mass Converter
    /// </summary>
    internal class MassConvertor
    {
        #region SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double GramsToKilograms(double value)
        {
            return (value / ConvertFactors.GRAMMES_PER_KILOGRAM);
        }

        static public double KilogramsToGrams(double value)
        {
            return (value * ConvertFactors.GRAMMES_PER_KILOGRAM);
        }

        #endregion // SI conversion

        #region US conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double OuncesToPounds(double value)
        {
            return (value / ConvertFactors.OUNCES_PER_POUND);
        }

        static public double PoundsToOunces(double value)
        {
            return (value * ConvertFactors.OUNCES_PER_POUND);
        }

        #endregion // US conversion

        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double GramsToOunces(double value)
        {
            return (value / ConvertFactors.GRAMMES_PER_OUNCE);
        }

        static public double OuncesToGrams(double value)
        {
            return (value * ConvertFactors.GRAMMES_PER_OUNCE);
        }

        #endregion // US\SI conversion
    }

    /// <summary>
    /// Time Converter
    /// </summary>
    internal class TimeConvertor
    {
        public enum TimeUnit
        {
            Second = 0,
            Minute = 1,
            Hour = 2,
            Day = 3
        }

        #region Conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double Convert(double value, TimeUnit fromUnit, TimeUnit toUnit)
        {
            Debug.Assert(fromUnit != toUnit);

            int from = (int)fromUnit;
            int to = (int)toUnit;
            double convertedValue = value;
            if (to < from)
            {
                for (int index = from; index > to; --index)
                    convertedValue *= FACTORS[index];
            }
            else
            {
                for (int index = from + 1; index <= to; ++index)
                    convertedValue /= FACTORS[index];
            }

            return convertedValue;
        }

        #endregion // Conversion

        #region Factors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static readonly double[] FACTORS = new double[] { 1,
                                                                  ConvertFactors.SECONDS_PER_MINUTE,
                                                                  ConvertFactors.MINUTES_PER_HOUR,
                                                                  ConvertFactors.HOURS_PER_DAY };

        #endregion // Factors
    }

    /// <summary>
    /// Volume Converter
    /// </summary>
    internal class VolumeConvertor
    {
        #region US conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double Inches3ToFoots3(double value)
        {
            return (value * ConvertFactors.INCHES3_PER_FOOT3);
        }

        static public double Foots3ToInches3(double value)
        {
            return (value / ConvertFactors.INCHES3_PER_FOOT3);
        }

        static public double Foots3ToYards3(double value)
        {
            return (value * ConvertFactors.FOOTS3_PER_YARD3);
        }

        static public double Yards3ToFoots3(double value)
        {
            return (value / ConvertFactors.FOOTS3_PER_YARD3);
        }

        #endregion // US conversion

        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double Inches3ToMeters3(double value)
        {
            return (value * ConvertFactors.INCHES3_PER_METER3);
        }

        static public double Meters3ToInches3(double value)
        {
            return (value / ConvertFactors.INCHES3_PER_METER3);
        }

        #endregion // US\SI conversion
    }

    /// <summary>
    /// Volume Converter Extensional (FluidVolume convert Volume)
    /// </summary>
    internal class VolumeConvertorEx
    {
        #region US conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double Inches3ToQuart(double value)
        {
            return (value * ConvertFactors.INCHES3_PER_QUART);
        }

        static public double QuartToInches3(double value)
        {
            return (value / ConvertFactors.INCHES3_PER_QUART);
        }

        #endregion // US conversion

        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double Meters3ToLiter(double value)
        {
            return (value * ConvertFactors.METER3_PER_LITER);
        }

        static public double LiterToMeters3(double value)
        {
            return (value / ConvertFactors.METER3_PER_LITER);
        }

        #endregion // US\SI conversion
    }

    /// <summary>
    /// Square Convertor
    /// </summary>
    internal class SquareConvertor
    {
        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double SquareFootToSquareMeter(double value)
        {
            return (value * ConvertFactors.METER2_PER_FOOT2);
        }

        static public double SquareMeterToSquareFoot(double value)
        {
            return (value / ConvertFactors.METER2_PER_FOOT2);
        }

        #endregion // US\SI conversion
    }

    /// <summary>
    /// Fuel Cost Convertor
    /// </summary>
    internal class FuelCostConvertor
    {
        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double CurrencyPerGallonToPerLiter(double value)
        {
            return (value / ConvertFactors.LITERS_PER_GALLON_US);
        }

        static public double CurrencyPerLiterToPerGallon(double value)
        {
            return (value * ConvertFactors.LITERS_PER_GALLON_US);
        }

        #endregion // US\SI conversion
    }

    /// <summary>
    /// Co2 Emission Convertor
    /// </summary>
    internal class Co2EmissionConvertor
    {
        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double PoundPerGallonToKilogramPerLiter(double value)
        {
            return (value * PPG_PER_KPL);
        }

        static public double KilogramPerLiterToPoundPerGallon(double value)
        {
            return (value / PPG_PER_KPL);
        }

        #endregion // US\SI conversion

        #region Factors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const double PPG_PER_KPL = ConvertFactors.LITERS_PER_GALLON_US / ConvertFactors.POUND_PER_KILOGRAM;

        #endregion // Factors
    }

    /// <summary>
    /// Fuel Economy Convertor
    /// </summary>
    internal class FuelEconomyConvertor
    {
        #region US\SI conversion
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static public double MPGToL100KM(double value)
        {
            return (0 == value)? 0 : (ConvertFactors.L100KM_PER_GPM_US / value);
        }

        static public double L100KMToMPG(double value)
        {
            return (0 == value)? 0 : (ConvertFactors.GPM_PER_L100KM_US * 100 / value);
        }

        #endregion // US\SI conversion
    }
}
