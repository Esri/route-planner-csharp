using System;
using System.Diagnostics;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class represented helper functions for ArcLogistic enums parsering.
    /// </summary>
    internal static class CustomEnumParser
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses input text to enum type element.
        /// </summary>
        /// <param name="type">Application enum type.</param>
        /// <param name="text">Input text to parsing.</param>
        /// <returns>Enum type element</returns>
        static public object Parse(Type type, string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(); // exception
            if (!type.IsEnum)
                throw new ArgumentException(); // exception
                    // NOTE: conversion supported only for Enums

            string normText = CommonHelpers.NormalizeText(text);

            object result = null;

            // check in predefined strings from resource
            Array elements = Enum.GetValues(type);
            foreach (object element in elements)
            {
                string[] supportedSymbols = _GetSupportedValues(element);
                if (CommonHelpers.IsValuePresentInList(normText, supportedSymbols))
                {
                    result = element;
                    break; // NOTE: result founded.
                }
            }

            // try convert as index
            if (null == result)
            {
                int index = int.Parse(normText);
                if (elements.Length <= index)
                    throw new InvalidOperationException(); // exception

                int curIndex = 0;
                foreach (object element in elements)
                {
                    if (index == curIndex)
                    {
                        result = element;
                        break; // operation done - stop
                    }
                    ++curIndex;
                }
            }

            // must be inited
            if (null == result)
                throw new InvalidOperationException(); // exception

            return result;
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets supported values by element.
        /// </summary>
        /// <param name="element">CurbApproach element.</param>
        /// <returns>Array of supported values.</returns>
        static private string _GetSupportedValues(CurbApproach element)
        {
            string supportedValues = null;
            switch (element)
            {
                case CurbApproach.Both:
                    supportedValues = Properties.Resources.CurbApproachBothSupportedValues;
                    break;
                case CurbApproach.Left:
                    supportedValues = Properties.Resources.CurbApproachLeftSupportedValues;
                    break;
                case CurbApproach.Right:
                    supportedValues = Properties.Resources.CurbApproachRightSupportedValues;
                    break;
                case CurbApproach.NoUTurns:
                    supportedValues = Properties.Resources.CurbApproachNoUTurnsSupportedValues;
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return supportedValues;
        }

        /// <summary>
        /// Gets supported values by element.
        /// </summary>
        /// <param name="element">SyncType element.</param>
        /// <returns>Array of supported values.</returns>
        static private string _GetSupportedValues(SyncType element)
        {
            string supportedValues = null;
            switch (element)
            {
                case SyncType.ActiveSync:
                    supportedValues = Properties.Resources.SyncTypeActiveSyncSupportedValues;
                    break;
                case SyncType.EMail:
                    supportedValues = Properties.Resources.SyncTypeEMailSupportedValues;
                    break;
                case SyncType.Folder:
                    supportedValues = Properties.Resources.SyncTypeFolderSupportedValues;
                    break;
                case SyncType.None:
                    supportedValues = Properties.Resources.SyncTypeNoneSupportedValues;
                    break;
                case SyncType.WMServer:
                    supportedValues = Properties.Resources.SyncTypeWMServerSupportedValues;
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return supportedValues;
        }

        /// <summary>
        /// Gets supported values by element.
        /// </summary>
        /// <param name="element">OrderPriority element.</param>
        /// <returns>Array of supported values.</returns>
        static private string _GetSupportedValues(OrderPriority element)
        {
            string supportedValues = null;
            switch (element)
            {
                case OrderPriority.High:
                    supportedValues = Properties.Resources.OrderPriorityHighSupportedValues;
                    break;
                case OrderPriority.Normal:
                    supportedValues = Properties.Resources.OrderPriorityNormalSupportedValues;
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return supportedValues;
        }

        /// <summary>
        /// Gets supported values by element.
        /// </summary>
        /// <param name="element">OrderType element.</param>
        /// <returns>Array of supported values.</returns>
        static private string _GetSupportedValues(OrderType element)
        {
            string supportedValues = null;
            switch (element)
            {
                case OrderType.Delivery:
                    supportedValues = Properties.Resources.OrderTypeDeliverySupportedValues;
                    break;
                case OrderType.Pickup:
                    supportedValues = Properties.Resources.OrderTypePickupSupportedValues;
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return supportedValues;
        }

        /// <summary>
        /// Gets supported values by type.
        /// </summary>
        /// <param name="element">Element for search.</param>
        /// <returns>Array of supported values.</returns>
        static private string[] _GetSupportedValues(object element)
        {
            string supportedValues = null;
            if (element is CurbApproach)
                supportedValues = _GetSupportedValues((CurbApproach)element);
            else if (element is SyncType)
                supportedValues = _GetSupportedValues((SyncType)element);
            else if (element is OrderType)
                supportedValues = _GetSupportedValues((OrderType)element);
            else if (element is OrderPriority)
                supportedValues = _GetSupportedValues((OrderPriority)element);
            else
            {
                Debug.Assert(false); // NOTE: not suported
            }

            return (null == supportedValues) ?
                        null : supportedValues.Split(TEXT_SEPARATORS,
                                                     StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion // Public methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Text separators.
        /// </summary>
        private static readonly char[] TEXT_SEPARATORS = new char[] { CommonHelpers.SEPARATOR };

        #endregion // Private constants
    }
}
