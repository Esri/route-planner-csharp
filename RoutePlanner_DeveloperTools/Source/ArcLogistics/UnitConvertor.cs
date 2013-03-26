/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Unit Convertor
    /// </summary>
    public static class UnitConvertor
    {
        #region public functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static double Convert(double value, Unit fromUnit, Unit toUnit)
        {
            double convertedValue = -1;
            if (fromUnit == toUnit)
                convertedValue = value;

            // Item
            else if ((Unit.Unknown == fromUnit) || (Unit.Unknown == toUnit))
                convertedValue = value;

            // Mass
            else if ((Unit.Pound == fromUnit) || (Unit.Ounce == fromUnit) ||
                     (Unit.Kilogram == fromUnit) || (Unit.Gram == fromUnit))
                convertedValue = _ConvertMass(value, fromUnit, toUnit);

            // Volume
            else if ((Unit.CubicFoot == fromUnit) || (Unit.CubicInch == fromUnit) ||
                     (Unit.CubicYard == fromUnit) || (Unit.CubicMeter == fromUnit) ||
                     (Unit.Quart == fromUnit) || (Unit.Gallon == fromUnit) ||
                     (Unit.Liter == fromUnit))
                convertedValue = _ConvertVolumeFull(value, fromUnit, toUnit);

            // Length
            else if ((Unit.Inch == fromUnit) || (Unit.Foot == fromUnit) ||
                     (Unit.Yard == fromUnit) || (Unit.Mile == fromUnit) ||
                     (Unit.Milimeter == fromUnit) || (Unit.Centimeter == fromUnit) ||
                     (Unit.Decimeter == fromUnit) || (Unit.Meter == fromUnit) ||
                     (Unit.Kilometer == fromUnit))
                convertedValue = _ConvertLength(value, fromUnit, toUnit);

            // Time
            else if ((Unit.Second == fromUnit) || (Unit.Minute == fromUnit) ||
                     (Unit.Hour == fromUnit) || (Unit.Day == fromUnit))
                convertedValue = TimeConvertor.Convert(value, _UnitTypeToTimeUnit(fromUnit), _UnitTypeToTimeUnit(toUnit));

            // Area
            else if ((Unit.SquareFoot == fromUnit) || (Unit.SquareMeter == fromUnit))
            {
                Debug.Assert((Unit.SquareFoot == toUnit) || (Unit.SquareMeter == toUnit));
                convertedValue = (Unit.SquareFoot == fromUnit)? SquareConvertor.SquareFootToSquareMeter(value) :
                                                                 SquareConvertor.SquareMeterToSquareFoot(value);
            }

            // Fuel Economy
            else if ((Unit.MilesPerGallon == fromUnit) || (Unit.LitersPer100Kilometers == fromUnit))
            {
                Debug.Assert((Unit.MilesPerGallon == toUnit) || (Unit.LitersPer100Kilometers == toUnit));
                convertedValue = (Unit.MilesPerGallon == fromUnit)? FuelEconomyConvertor.MPGToL100KM(value) :
                                                                     FuelEconomyConvertor.L100KMToMPG(value);
            }

            else if (Unit.Currency == fromUnit)
            {
                Debug.Assert(Unit.Currency == toUnit);
                convertedValue = value;
            }

            // Fuel Cost
            else if ((Unit.CurrencyPerGallon == fromUnit) || (Unit.CurrencyPerLiter == fromUnit))
            {
                Debug.Assert((Unit.CurrencyPerGallon == toUnit) || (Unit.CurrencyPerLiter == toUnit));
                convertedValue = (Unit.CurrencyPerGallon == fromUnit)? FuelCostConvertor.CurrencyPerGallonToPerLiter(value) :
                                                                        FuelCostConvertor.CurrencyPerLiterToPerGallon(value);
            }

            // Co2 Emission
            else if ((Unit.PoundPerGallon == fromUnit) || (Unit.KilogramPerLiter == fromUnit))
            {
                Debug.Assert((Unit.PoundPerGallon == toUnit) || (Unit.KilogramPerLiter == toUnit));
                convertedValue = (Unit.PoundPerGallon == fromUnit)? Co2EmissionConvertor.PoundPerGallonToKilogramPerLiter(value) :
                                                                     Co2EmissionConvertor.KilogramPerLiterToPoundPerGallon(value);
            }

            else
            {
                Debug.Assert(false); // NOTE: not supported
                throw new ArgumentException(Properties.Resources.InvalidConversion);
            }

            return convertedValue;
        }

        #endregion // public functions

        #region helper functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static bool _IsSIUnit(Unit type)
        {
            return ((Unit.Kilogram == type) ||
                    (Unit.Gram == type) ||
                    (Unit.CubicMeter == type) ||
                    (Unit.Liter == type) ||
                    (Unit.Milimeter == type) ||
                    (Unit.Centimeter == type) ||
                    (Unit.Decimeter == type) ||
                    (Unit.Meter == type) ||
                    (Unit.Kilometer == type) ||
                    (Unit.SquareMeter == type) ||
                    (Unit.LitersPer100Kilometers == type) ||
                    (Unit.CurrencyPerLiter == type) ||
                    (Unit.KilogramPerLiter == type));
        }

        private static bool _IsUSUnit(Unit type)
        {
            return ((Unit.Pound == type) ||
                    (Unit.Ounce == type) ||
                    (Unit.CubicFoot == type) ||
                    (Unit.CubicInch == type) ||
                    (Unit.CubicYard == type) ||
                    (Unit.Quart == type) ||
                    (Unit.Gallon == type) ||
                    (Unit.Inch == type) ||
                    (Unit.Foot == type) ||
                    (Unit.Yard == type) ||
                    (Unit.Mile == type) ||
                    (Unit.SquareFoot == type) ||
                    (Unit.MilesPerGallon == type) ||
                    (Unit.CurrencyPerGallon == type) ||
                    (Unit.PoundPerGallon == type));
        }

        private static double _MassOuncesToSI(double value, Unit toUnit)
        {
            Debug.Assert(_IsSIUnit(toUnit));

            double convertedValue = MassConvertor.OuncesToGrams(value); // [g]
            if (Unit.Kilogram == toUnit)
                convertedValue = MassConvertor.GramsToKilograms(convertedValue); // [kg]
            return convertedValue;
        }

        private static double _MassGramsToUS(double value, Unit toUnit)
        {
            Debug.Assert(_IsUSUnit(toUnit));

            double convertedValue = MassConvertor.GramsToOunces(value); // [oz]
            if (Unit.Pound == toUnit)
                convertedValue = MassConvertor.OuncesToPounds(convertedValue); // [lb]
            return convertedValue;
        }

        private static double _VolumeFoots3ToOther(double value, Unit toUnit)
        {
            Debug.Assert((Unit.CubicInch == toUnit) || (Unit.CubicMeter == toUnit));
            double convertedValue = VolumeConvertor.Foots3ToInches3(value); // [in3]
            if (Unit.CubicMeter == toUnit)
                convertedValue = VolumeConvertor.Inches3ToMeters3(convertedValue); // [m3]
            return convertedValue;
        }

        private static double _VolumeInches3ToOtherUS(double value, Unit toUnit)
        {
            Debug.Assert((Unit.CubicFoot == toUnit) || (Unit.CubicYard == toUnit));
            double convertedValue = VolumeConvertor.Inches3ToFoots3(value); // [ft3]
            if (Unit.CubicYard == toUnit)
                convertedValue = VolumeConvertor.Foots3ToYards3(convertedValue); // [yd3]
            return convertedValue;
        }

        private static double _ConvertMass(double value, Unit fromUnit, Unit toUnit)
        {
            Debug.Assert((Unit.Pound == toUnit) || (Unit.Ounce == toUnit) ||
                         (Unit.Kilogram == toUnit) || (Unit.Gram == toUnit));
            double convertedValue = value;
            if (Unit.Ounce == fromUnit) // [oz]
            {
                convertedValue = (Unit.Pound == toUnit)? MassConvertor.OuncesToPounds(value) : // [lb]
                                                          _MassOuncesToSI(value, toUnit); // [g], [kg]
            }

            else if (Unit.Pound == fromUnit) // [lb]
            {
                convertedValue = MassConvertor.PoundsToOunces(value); // [oz]
                if (Unit.Ounce != toUnit)
                    convertedValue = _MassOuncesToSI(convertedValue, toUnit); // [g], [kg]
            }

            else if (Unit.Kilogram == fromUnit) // [kg]
            {
                convertedValue = MassConvertor.KilogramsToGrams(value); // [g]
                if (Unit.Gram != toUnit)
                    convertedValue = _MassGramsToUS(convertedValue, toUnit); // [oz], [lb]
            }

            else if (Unit.Gram == fromUnit) // [g]
            {
                convertedValue = (Unit.Kilogram == toUnit)? MassConvertor.GramsToKilograms(value) : // [kg]
                                                             _MassGramsToUS(value, toUnit); // [oz], [lb]
            }

            return convertedValue;
        }

        private static double _ConvertVolume(double value, Unit fromUnit, Unit toUnit)
        {
            if (fromUnit == toUnit)
                return value;

            Debug.Assert((Unit.CubicFoot == toUnit) || (Unit.CubicInch == toUnit) ||
                         (Unit.CubicYard == toUnit) || (Unit.CubicMeter == toUnit));

            double convertedValue = value;
            if (Unit.CubicInch == fromUnit) // [in3]
            {
                convertedValue = (Unit.CubicMeter == toUnit)? VolumeConvertor.Inches3ToMeters3(value) : // [m3]
                                                               _VolumeInches3ToOtherUS(value, toUnit); // [ft3], [yd3]
            }

            else if (Unit.CubicFoot == fromUnit) // [ft3]
            {
                convertedValue = (Unit.CubicYard == toUnit)? VolumeConvertor.Foots3ToYards3(value) : // [yd3]
                                                               _VolumeFoots3ToOther(value, toUnit); // [in3], [m3]
            }

            else if (Unit.CubicYard == fromUnit) // [yd3]
            {
                convertedValue = VolumeConvertor.Yards3ToFoots3(value); // [ft3]
                if (Unit.CubicFoot != toUnit)
                    convertedValue = _VolumeFoots3ToOther(convertedValue, toUnit); // [in3], [m3]
            }

            else if (Unit.CubicMeter == fromUnit) // [m3]
            {
                convertedValue = VolumeConvertor.Meters3ToInches3(value); // [in3]
                if (Unit.CubicInch != toUnit)
                    convertedValue = _VolumeInches3ToOtherUS(convertedValue, toUnit); // [ft3], [yd3]
            }

            return convertedValue;
        }

        private static double _ConvertFluidVolume(double value, Unit fromUnit, Unit toUnit)
        {
            if (fromUnit == toUnit)
                return value;

            Debug.Assert((Unit.Quart == toUnit) || (Unit.Gallon == toUnit) ||
                         (Unit.Liter == toUnit));

            double convertedValue = value;
            if (Unit.Quart == fromUnit) // [qt]
            {
                convertedValue = (Unit.Liter == toUnit)? FluidVolumeConvertor.QuartsToLiters(value) : // [L]
                                                         FluidVolumeConvertor.QuartsToGalons(value); // [gal]
            }

            else if (Unit.Gallon == fromUnit) // [gal]
            {
                convertedValue = FluidVolumeConvertor.GalonsToQuarts(value); // [qt]
                if (Unit.Liter == toUnit)
                    convertedValue = FluidVolumeConvertor.QuartsToLiters(convertedValue); //[L]
            }

            else if (Unit.Liter == fromUnit) // [L]
            {
                convertedValue = FluidVolumeConvertor.LitersToQuarts(value); // [qt]
                if (Unit.Gallon == toUnit)
                    convertedValue = FluidVolumeConvertor.QuartsToGalons(convertedValue); //[gal]
            }

            return convertedValue;
        }

        private static double _ConvertVolumeFull(double value, Unit fromUnit, Unit toUnit)
        {
            Debug.Assert((Unit.CubicFoot == toUnit) || (Unit.CubicInch == toUnit) ||
                         (Unit.CubicYard == toUnit) || (Unit.CubicMeter == toUnit) ||
                         (Unit.Quart == toUnit) || (Unit.Gallon == toUnit) ||
                         (Unit.Liter == toUnit));

            double convertedValue = value;
            // convert volume
            if (((Unit.CubicFoot == toUnit) || (Unit.CubicInch == toUnit) ||
                 (Unit.CubicYard == toUnit) || (Unit.CubicMeter == toUnit)) &&
                ((Unit.CubicFoot == fromUnit) || (Unit.CubicInch == fromUnit) ||
                 (Unit.CubicYard == fromUnit) || (Unit.CubicMeter == fromUnit)))
                convertedValue = _ConvertVolume(value, fromUnit, toUnit);
            // convert fluid volume
            else if (((Unit.Quart == toUnit) || (Unit.Gallon == toUnit) ||
                      (Unit.Liter == toUnit)) && ((Unit.Quart == fromUnit) ||
                      (Unit.Gallon == fromUnit) || (Unit.Liter == fromUnit)))
                convertedValue = _ConvertFluidVolume(value, fromUnit, toUnit);
            else
            {   // convert volume to fluid volume
                if ((Unit.CubicFoot == fromUnit) || (Unit.CubicInch == fromUnit) ||
                    (Unit.CubicYard == fromUnit) || (Unit.CubicMeter == fromUnit))
                {
                    convertedValue = _ConvertVolume(value, fromUnit, Unit.CubicInch); // to inch3
                    convertedValue = VolumeConvertorEx.Inches3ToQuart(convertedValue); // to quart
                    convertedValue = _ConvertFluidVolume(convertedValue, Unit.Quart, toUnit); // to needed
                }
                else
                {   // convert fluid volume to volume
                    Debug.Assert((Unit.Quart == fromUnit) || (Unit.Gallon == fromUnit) ||
                                 (Unit.Liter == fromUnit));
                    convertedValue = _ConvertFluidVolume(convertedValue, fromUnit, Unit.Quart); // to quart
                    convertedValue = VolumeConvertorEx.QuartToInches3(convertedValue); // to inch3
                    convertedValue = _ConvertVolume(convertedValue, Unit.CubicInch, toUnit); // to needed
                }
            }

            return convertedValue;
        }

        private static LengthConvertor.UnitSI _UnitTypeToUnitSI(Unit unitType)
        {
            Debug.Assert(_IsSIUnit(unitType));

            LengthConvertor.UnitSI lengthType = LengthConvertor.UnitSI.Milimeter;
            switch (unitType)
            {
                case Unit.Milimeter:
                    lengthType = LengthConvertor.UnitSI.Milimeter;
                    break;
                case Unit.Centimeter:
                    lengthType = LengthConvertor.UnitSI.Centimeter;
                    break;
                case Unit.Decimeter:
                    lengthType = LengthConvertor.UnitSI.Decimeter;
                    break;
                case Unit.Meter:
                    lengthType = LengthConvertor.UnitSI.Meter;
                    break;
                case Unit.Kilometer:
                    lengthType = LengthConvertor.UnitSI.Kilometer;
                    break;
                default:
                    {
                        Debug.Assert(false); // NOTE: not sussported
                        break;
                    }
            }

            return lengthType;
        }

        private static LengthConvertor.UnitUS _UnitTypeToUnitUS(Unit unitType)
        {
            Debug.Assert(_IsUSUnit(unitType));

            LengthConvertor.UnitUS lengthType = LengthConvertor.UnitUS.Inch;
            switch (unitType)
            {
                case Unit.Inch:
                    lengthType = LengthConvertor.UnitUS.Inch;
                    break;
                case Unit.Foot:
                    lengthType = LengthConvertor.UnitUS.Foot;
                    break;
                case Unit.Yard:
                    lengthType = LengthConvertor.UnitUS.Yard;
                    break;
                case Unit.Mile:
                    lengthType = LengthConvertor.UnitUS.Mile;
                    break;
                default:
                    {
                        Debug.Assert(false); // NOTE: not sussported
                        break;
                    }
            }

            return lengthType;
        }

        private static double _ConvertLength(double value, Unit fromUnit, Unit toUnit)
        {
            Debug.Assert((Unit.Inch == toUnit) || (Unit.Foot == toUnit) ||
                         (Unit.Yard == toUnit) || (Unit.Mile == toUnit) ||
                         (Unit.Milimeter == toUnit) || (Unit.Centimeter == toUnit) ||
                         (Unit.Decimeter == toUnit) || (Unit.Meter == toUnit) ||
                         (Unit.Kilometer == toUnit));
            double convertedValue = value;
            if (_IsSIUnit(fromUnit) && _IsSIUnit(toUnit))
                convertedValue = LengthConvertor.ConvertSI(value, _UnitTypeToUnitSI(fromUnit), _UnitTypeToUnitSI(toUnit));

            else if (_IsUSUnit(fromUnit) && _IsUSUnit(toUnit))
                convertedValue = LengthConvertor.ConvertUS(value, _UnitTypeToUnitUS(fromUnit), _UnitTypeToUnitUS(toUnit));

            else if (_IsSIUnit(fromUnit) && _IsUSUnit(toUnit))
            {
                double valueMM = (Unit.Milimeter == fromUnit) ? value :
                                     LengthConvertor.ConvertSI(value, _UnitTypeToUnitSI(fromUnit), LengthConvertor.UnitSI.Milimeter);
                double valueIN = LengthConvertor.MilimetersToInches(valueMM);
                convertedValue = (Unit.Inch == toUnit) ? valueIN :
                                     LengthConvertor.ConvertUS(valueIN, LengthConvertor.UnitUS.Inch, _UnitTypeToUnitUS(toUnit));
            }
            else // _IsUSUnit(fromUnit) && _IsSIUnit(toUnit)
            {
                double valueIN = (Unit.Inch == fromUnit) ? value :
                                     LengthConvertor.ConvertUS(value, _UnitTypeToUnitUS(fromUnit), LengthConvertor.UnitUS.Inch);
                double valueMM = LengthConvertor.InchesToMilimeters(valueIN);
                convertedValue = (Unit.Milimeter == toUnit) ? valueMM :
                                     LengthConvertor.ConvertSI(valueMM, LengthConvertor.UnitSI.Milimeter, _UnitTypeToUnitSI(toUnit));
            }

            return convertedValue;
        }

        private static TimeConvertor.TimeUnit _UnitTypeToTimeUnit(Unit unitType)
        {
            TimeConvertor.TimeUnit timeType = TimeConvertor.TimeUnit.Second;
            switch (unitType)
            {
                case Unit.Second:
                    timeType = TimeConvertor.TimeUnit.Second;
                    break;
                case Unit.Minute:
                    timeType = TimeConvertor.TimeUnit.Minute;
                    break;
                case Unit.Hour:
                    timeType = TimeConvertor.TimeUnit.Hour;
                    break;
                case Unit.Day:
                    timeType = TimeConvertor.TimeUnit.Day;
                    break;
                default:
                    {
                        Debug.Assert(false); // NOTE: not sussported
                        break;
                    }
            }

            return timeType;
        }

        #endregion // helper functions
    }
}
