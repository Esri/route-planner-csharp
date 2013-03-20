using System;
using System.Diagnostics;
using System.Globalization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class provide helper functions for format units show text.
    /// </summary>
    public static class UnitFormatter
    {
        #region public functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Formats value show text.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="unit">Unit type.</param>
        /// <returns>Formated text.</returns>
        public static string Format(double value, Unit unit)
        {
            string formattedValue = null;
            if (Unit.Currency == unit)
                formattedValue = value.ToString("c");
            else
            {
                string unitSymbol = _GetUnitSymbol(unit);

                formattedValue = _Format(value);
                if (!string.IsNullOrEmpty(unitSymbol))
                    formattedValue = string.Format(Properties.Resources.UnitFormat, formattedValue, unitSymbol);
            }

            return formattedValue;
        }

        /// <summary>
        /// Gets unit type by symbol (with check).
        /// </summary>
        /// <param name="symbol">Unit symbol.</param>
        /// <param name="expected">Expected type (for checking units scope).</param>
        /// <returns>Unit type or Unit.Unknown if not valid symbol</returns>
        public static Unit GetUnitBySymbol(string symbol, Unit expected)
        {
            Unit unit = _GetUnitBySymbol(symbol, _AllSupportedUnits);

            // check type scope
            Unit result = Unit.Unknown;
            if (_IsUnitsFromOneScope(unit, expected, _MassScope) ||
                _IsUnitsFromOneScope(unit, expected, _VolumeScope) ||
                _IsUnitsFromOneScope(unit, expected, _LengthScope) ||
                _IsUnitsFromOneScope(unit, expected, _TimeScope) ||
                _IsUnitsFromOneScope(unit, expected, _AreaScope) ||
                _IsUnitsFromOneScope(unit, expected, _CostScope) ||
                _IsUnitsFromOneScope(unit, expected, _FuelEconomyScope) ||
                _IsUnitsFromOneScope(unit, expected, _FuelCostScope) ||
                _IsUnitsFromOneScope(unit, expected, _Co2EmissionScope))
                result = unit;
            else
            {   // NOTE: for example - minute and meter have symbol 'm',
                //       need update first founded unit if it not belong to expected scope
                Unit[] scope = _GetScopeByUnit(expected);
                result = _GetUnitBySymbol(symbol, scope);
            }

            if (result == Unit.Unknown)
                throw new NotSupportedException();

            return result;
        }

        #endregion // public functions

        #region internal functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets unit localized title.
        /// </summary>
        /// <param name="unit"><see cref="P:ESRI.ArcLogistics.Unit" />.</param>
        /// <returns>Localized unit title.</returns>
        internal static string GetUnitTitle(Unit unit)
        {
            string[] supportedSymbols = _GetSupportedSymbols(unit);

            string result = null;
            if ((null != supportedSymbols) && (1 < supportedSymbols.Length))
                result = supportedSymbols[FULL_TITLE_INDEX];

            return result;
        }

        #endregion // internal functions

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// All supported units.
        /// </summary>
        private static Unit[] _AllSupportedUnits
        {
            get { return (Unit[])Enum.GetValues(typeof(Unit)); }
        }

        /// <summary>
        /// Mass scope.
        /// </summary>
        private static Unit[] _MassScope
        {
            get { return new Unit[] { Unit.Pound, Unit.Ounce, Unit.Kilogram, Unit.Gram }; }
        }

        /// <summary>
        /// Volume scope.
        /// </summary>
        private static Unit[] _VolumeScope
        {
            get { return new Unit[] { Unit.CubicInch, Unit.CubicFoot, Unit.CubicYard, Unit.CubicMeter, Unit.Quart, Unit.Gallon, Unit.Liter }; }
        }

        /// <summary>
        /// Length scope.
        /// </summary>
        private static Unit[] _LengthScope
        {
            get { return new Unit[] { Unit.Inch, Unit.Foot, Unit.Yard, Unit.Mile, Unit.Milimeter, Unit.Centimeter, Unit.Decimeter, Unit.Meter, Unit.Kilometer }; }
        }

        /// <summary>
        /// Time scope.
        /// </summary>
        private static Unit[] _TimeScope
        {
            get { return new Unit[] { Unit.Day, Unit.Hour, Unit.Minute, Unit.Second }; }
        }

        /// <summary>
        /// Area scope.
        /// </summary>
        private static Unit[] _AreaScope
        {
            get { return new Unit[] { Unit.SquareFoot, Unit.SquareMeter }; }
        }

        /// <summary>
        /// Cost scope.
        /// </summary>
        private static Unit[] _CostScope
        {
            get { return new Unit[] { Unit.Currency }; }
        }

        /// <summary>
        /// Fuel economy scope.
        /// </summary>
        private static Unit[] _FuelEconomyScope
        {
            get { return new Unit[] { Unit.MilesPerGallon, Unit.LitersPer100Kilometers }; }
        }

        /// <summary>
        /// Fuel cost scope.
        /// </summary>
        private static Unit[] _FuelCostScope
        {
            get { return new Unit[] { Unit.CurrencyPerGallon, Unit.CurrencyPerLiter }; }
        }

        /// <summary>
        /// CO2 emission scope.
        /// </summary>
        private static Unit[] _Co2EmissionScope
        {
            get { return new Unit[] { Unit.PoundPerGallon, Unit.KilogramPerLiter }; }
        }

        #endregion // private properties

        #region private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Formats double value.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <returns>Show numeric string.</returns>
        private static string _Format(double value)
        {
            string result = value.ToString("N", CultureInfo.CurrentCulture.NumberFormat);
            return _ClearNulls(result);
        }

        /// <summary>
        /// Removes all final nulls.
        /// </summary>
        /// <param name="numericText">Input numeric string.</param>
        /// <returns>Numeric text without final nulls.</returns>
        private static string _ClearNulls(string numericText)
        {
            string clearString = numericText;
            int decimalIndex = numericText.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (decimalIndex > 0)
            {
                for (int i = clearString.Length - 1; i > decimalIndex; i--)
                {
                    if (clearString[i] == '0')
                        clearString = clearString.Remove(i, 1);
                    else
                        break;
                }
            }

            if (clearString.Length - 1 == decimalIndex)
                clearString = clearString.Remove(decimalIndex, 1);

            return clearString;
        }

        /// <summary>
        /// Remove '.' in symbol text.
        /// </summary>
        /// <param name="symbol">Symbol text.</param>
        /// <returns>Symbol text without dots.</returns>
        private static string _RemoveDots(string symbol)
        {
            Debug.Assert(!string.IsNullOrEmpty(symbol.Trim()));
            return symbol.Replace(DOT, ""); // remove spaces;
        }

        /// <summary>
        /// Gets unit symbol.
        /// </summary>
        /// <param name="unit">Unit type.</param>
        /// <returns>Unit symbol.</returns>
        static private string _GetUnitSymbol(Unit unit)
        {
            string unitSymbol = null;
            switch (unit)
            {
                case Unit.Unknown:
                    break;
                // Mass
                case Unit.Pound:
                    unitSymbol = Properties.Resources.UnitSymbolPound;
                    break;
                case Unit.Ounce:
                    unitSymbol = Properties.Resources.UnitSymbolOunce;
                    break;
                case Unit.Kilogram:
                    unitSymbol = Properties.Resources.UnitSymbolKilogram;
                    break;
                case Unit.Gram:
                    unitSymbol = Properties.Resources.UnitSymbolGramme;
                    break;
                // Volume
                case Unit.CubicFoot:
                    unitSymbol = Properties.Resources.UnitSymbolCubicFoot;
                    break;
                case Unit.CubicInch:
                    unitSymbol = Properties.Resources.UnitSymbolCubicInch;
                    break;
                case Unit.CubicYard:
                    unitSymbol = Properties.Resources.UnitSymbolCubicYard;
                    break;
                case Unit.CubicMeter:
                    unitSymbol = Properties.Resources.UnitSymbolCubicMeter;
                    break;
                // Fluid volume
                case Unit.Quart:
                    unitSymbol = Properties.Resources.UnitSymbolQuart;
                    break;
                case Unit.Gallon:
                    unitSymbol = Properties.Resources.UnitSymbolGallon;
                    break;
                case Unit.Liter:
                    unitSymbol = Properties.Resources.UnitSymbolLiter;
                    break;
                // Length
                case Unit.Inch:
                    unitSymbol = Properties.Resources.UnitSymbolInch;
                    break;
                case Unit.Foot:
                    unitSymbol = Properties.Resources.UnitSymbolFoot;
                    break;
                case Unit.Yard:
                    unitSymbol = Properties.Resources.UnitSymbolYard;
                    break;
                case Unit.Mile:
                    unitSymbol = Properties.Resources.UnitSymbolMile;
                    break;
                case Unit.Milimeter:
                    unitSymbol = Properties.Resources.UnitSymbolMilimeter;
                    break;
                case Unit.Centimeter:
                    unitSymbol = Properties.Resources.UnitSymbolCentimeter;
                    break;
                case Unit.Decimeter:
                    unitSymbol = Properties.Resources.UnitSymbolDecimeter;
                    break;
                case Unit.Meter:
                    unitSymbol = Properties.Resources.UnitSymbolMeter;
                    break;
                case Unit.Kilometer:
                    unitSymbol = Properties.Resources.UnitSymbolKilometer;
                    break;
                // Time
                case Unit.Second:
                    unitSymbol = Properties.Resources.UnitSymbolSecond;
                    break;
                case Unit.Minute:
                    unitSymbol = Properties.Resources.UnitSymbolMinute;
                    break;
                case Unit.Hour:
                    unitSymbol = Properties.Resources.UnitSymbolHour;
                    break;
                case Unit.Day:
                    unitSymbol = Properties.Resources.UnitSymbolDay;
                    break;
                // Area
                case Unit.SquareFoot:
                    unitSymbol = Properties.Resources.UnitSymbolSquareFoot;
                    break;
                case Unit.SquareMeter:
                    unitSymbol = Properties.Resources.UnitSymbolSquareMeter;
                    break;
                // Cost
                case Unit.Currency:
                    Debug.Assert(false); // NOTE: special case
                    break;
                // Fuel Economy
                case Unit.MilesPerGallon:
                    unitSymbol = Properties.Resources.UnitSymbolMilesPerGallon;
                    break;
                case Unit.LitersPer100Kilometers:
                    unitSymbol = Properties.Resources.UnitSymbolLitersPer100Kilometers;
                    break;
                // Fuel Cost
                case Unit.CurrencyPerGallon:
                    unitSymbol = string.Format(Properties.Resources.UnitSymbolPerFormat,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, Properties.Resources.UnitSymbolGallon);
                    break;
                case Unit.CurrencyPerLiter:
                    unitSymbol = string.Format(Properties.Resources.UnitSymbolPerFormat,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, Properties.Resources.UnitSymbolLiter);
                    break;
                // Co2 Emission
                case Unit.PoundPerGallon:
                    unitSymbol = string.Format(Properties.Resources.UnitSymbolPerFormat,
                                               Properties.Resources.UnitSymbolPound, Properties.Resources.UnitSymbolGallon);
                    break;
                case Unit.KilogramPerLiter:
                    unitSymbol = string.Format(Properties.Resources.UnitSymbolPerFormat,
                                               Properties.Resources.UnitSymbolKilogram, Properties.Resources.UnitSymbolLiter);
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported;
                    break;
            }

            return unitSymbol;
        }

        /// <summary>
        /// Gets unit by symbol.
        /// </summary>
        /// <param name="symbol">Input symbol text.</param>
        /// <param name="units">Units to check.</param>
        /// <returns>Parsed unit type.</returns>
        public static Unit _GetUnitBySymbol(string symbol, Unit[] units)
        {
            if (string.IsNullOrEmpty(symbol.Trim()))
                throw new System.ArgumentException(); // exception

            string normSymbol = CommonHelpers.NormalizeText(symbol);
            string symbolToCheck = _RemoveDots(normSymbol);

            Unit result = Unit.Unknown;
            for (int index = 0; index < units.Length; ++index)
            {
                Unit unit = units[index];
                string[] supportedSymbols = _GetSupportedSymbols(unit);
                if (CommonHelpers.IsValuePresentInList(symbolToCheck, supportedSymbols))
                {
                    result = unit;
                    break; // NOTE: result founded.
                }
            }

            return result;
        }

        /// <summary>
        /// Checks is unit is units scope.
        /// </summary>
        /// <param name="unit">Unit to checking.</param>
        /// <param name="scope">Units scope.</param>
        /// <returns>TRUE if unit present in units scope.</returns>
        private static bool _IsUnitInScope(Unit unit, Unit[] scope)
        {
            bool result = false;
            for (int index = 0; index < scope.Length; ++index)
            {
                if (unit == scope[index])
                {
                    result = true;
                    break; // NOTE result founded
                }
            }

            return result;
        }

        /// <summary>
        /// Checks units belong to one scope.
        /// </summary>
        /// <param name="readed">Input unit type to check.</param>
        /// <param name="expected">Expected unit type.</param>
        /// <param name="scope">Units scope.</param>
        /// <returns>TRUE if both unit type in scope.</returns>
        private static bool _IsUnitsFromOneScope(Unit readed, Unit expected, Unit[] scope)
        {
            return (_IsUnitInScope(readed, scope) && _IsUnitInScope(expected, scope));
        }

        /// <summary>
        /// Gets supported symbols.
        /// </summary>
        /// <param name="unit">Unit type.</param>
        /// <returns>Unit's supported symbol list.</returns>
        private static string[] _GetSupportedSymbols(Unit unit)
        {
            string supportedSymbols = null;
            switch (unit)
            {
                case Unit.Unknown:
                    break; // empty
                case Unit.Pound:
                    supportedSymbols = Properties.Resources.UnitSymbolPoundSupported;
                    break;
                case Unit.Ounce:
                    supportedSymbols = Properties.Resources.UnitSymbolOunceSupported;
                    break;
                case Unit.Kilogram:
                    supportedSymbols = Properties.Resources.UnitSymbolKilogramSupported;
                    break;
                case Unit.Gram:
                    supportedSymbols = Properties.Resources.UnitSymbolGrammeSupported;
                    break;
                case Unit.CubicInch:
                    supportedSymbols = Properties.Resources.UnitSymbolCubicInchSupported;
                    break;
                case Unit.CubicFoot:
                    supportedSymbols = Properties.Resources.UnitSymbolCubicFootSupported;
                    break;
                case Unit.CubicYard:
                    supportedSymbols = Properties.Resources.UnitSymbolCubicYardSupported;
                    break;
                case Unit.CubicMeter:
                    supportedSymbols = Properties.Resources.UnitSymbolCubicMeterSupported;
                    break;
                case Unit.Quart:
                    supportedSymbols = Properties.Resources.UnitSymbolQuartSupported;
                    break;
                case Unit.Gallon:
                    supportedSymbols = Properties.Resources.UnitSymbolGallonSupported;
                    break;
                case Unit.Liter:
                    supportedSymbols = Properties.Resources.UnitSymbolLiterSupported;
                    break;
                case Unit.Inch:
                    supportedSymbols = Properties.Resources.UnitSymbolInchSupported;
                    break;
                case Unit.Foot:
                    supportedSymbols = Properties.Resources.UnitSymbolFootSupported;
                    break;
                case Unit.Yard:
                    supportedSymbols = Properties.Resources.UnitSymbolYardSupported;
                    break;
                case Unit.Mile:
                    supportedSymbols = Properties.Resources.UnitSymbolMileSupported;
                    break;
                case Unit.Milimeter:
                    supportedSymbols = Properties.Resources.UnitSymbolMilimeterSupported;
                    break;
                case Unit.Centimeter:
                    supportedSymbols = Properties.Resources.UnitSymbolCentimeterSupported;
                    break;
                case Unit.Decimeter:
                    supportedSymbols = Properties.Resources.UnitSymbolDecimeterSupported;
                    break;
                case Unit.Meter:
                    supportedSymbols = Properties.Resources.UnitSymbolMeterSupported;
                    break;
                case Unit.Kilometer:
                    supportedSymbols = Properties.Resources.UnitSymbolKilometerSupported;
                    break;
                case Unit.Second:
                    supportedSymbols = Properties.Resources.UnitSymbolSecondSupported;
                    break;
                case Unit.Minute:
                    supportedSymbols = Properties.Resources.UnitSymbolMinuteSupported;
                    break;
                case Unit.Hour:
                    supportedSymbols = Properties.Resources.UnitSymbolHourSupported;
                    break;
                case Unit.Day:
                    supportedSymbols = Properties.Resources.UnitSymbolDaySupported;
                    break;
                case Unit.SquareFoot:
                    supportedSymbols = Properties.Resources.UnitSymbolSquareFootSupported;
                    break;
                case Unit.SquareMeter:
                    supportedSymbols = Properties.Resources.UnitSymbolSquareMeterSupported;
                    break;
                case Unit.Currency:
                    supportedSymbols = Properties.Resources.UnitSymbolCurrencySupported;
                    break;
                case Unit.MilesPerGallon:
                    supportedSymbols = Properties.Resources.UnitSymbolMilesPerGallonSupported;
                    break;
                case Unit.LitersPer100Kilometers:
                    supportedSymbols = Properties.Resources.UnitSymbolLitersPer100KilometersSupported;
                    break;
                case Unit.CurrencyPerGallon:
                    supportedSymbols = Properties.Resources.UnitSymbolCurrencyPerGallonSupported;
                    break;
                case Unit.CurrencyPerLiter:
                    supportedSymbols = Properties.Resources.UnitSymbolCurrencyPerLiterSupported;
                    break;
                case Unit.PoundPerGallon:
                    supportedSymbols = Properties.Resources.UnitSymbolPoundPerGallonSupported;
                    break;
                case Unit.KilogramPerLiter:
                    supportedSymbols = Properties.Resources.UnitSymbolKilogramPerLiterSupported;
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return (null == supportedSymbols)? null :
                                               supportedSymbols.Split(new char[] { CommonHelpers.SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets units scope by unit form scope.
        /// </summary>
        /// <param name="unit">Input unit.</param>
        /// <returns>Units scope.</returns>
        private static Unit[] _GetScopeByUnit(Unit unit)
        {
            Debug.Assert(unit != Unit.Unknown);

            Unit[] scope = null;
            switch (unit)
            {
                case Unit.Pound:
                case Unit.Ounce:
                case Unit.Kilogram:
                case Unit.Gram:
                    scope = _MassScope;
                    break;

                case Unit.CubicInch:
                case Unit.CubicFoot:
                case Unit.CubicYard:
                case Unit.CubicMeter:
                case Unit.Quart:
                case Unit.Gallon:
                case Unit.Liter:
                    scope = _VolumeScope;
                    break;

                case Unit.Inch:
                case Unit.Foot:
                case Unit.Yard:
                case Unit.Mile:
                case Unit.Milimeter:
                case Unit.Centimeter:
                case Unit.Decimeter:
                case Unit.Meter:
                case Unit.Kilometer:
                    scope = _LengthScope;
                    break;

                case Unit.Day:
                case Unit.Hour:
                case Unit.Minute:
                case Unit.Second:
                    scope = _TimeScope;
                    break;

                case Unit.SquareFoot:
                case Unit.SquareMeter:
                    scope = _AreaScope;
                    break;

                case Unit.Currency:
                    scope = _CostScope;
                    break;

                case Unit.MilesPerGallon:
                case Unit.LitersPer100Kilometers:
                    scope = _FuelEconomyScope;
                    break;

                case Unit.CurrencyPerGallon:
                case Unit.CurrencyPerLiter:
                    scope = _FuelCostScope;
                    break;

                case Unit.PoundPerGallon:
                case Unit.KilogramPerLiter:
                    scope = _Co2EmissionScope;
                    break;

                default:
                    Debug.Assert(false); // NOTE: not suported
                    break;
            }

            return scope;
        }

        #endregion // private helpers

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Symbol DOT.
        /// </summary>
        private const string DOT = ".";

        /// <summary>
        /// Unit symbol title index.
        /// </summary>
        private const int FULL_TITLE_INDEX = 0;

        #endregion // private constants
    }
}
