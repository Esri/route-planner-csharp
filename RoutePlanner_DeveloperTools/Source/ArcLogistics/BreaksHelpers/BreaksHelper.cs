using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.BreaksHelpers
{
    /// <summary>
    /// Class provides comparison for brakes.
    /// </summary>
    internal static class BreaksHelper
    {
        #region Public Static methods

        /// <summary>
        /// Comparison for two Brakes.
        /// </summary>
        /// <param name="break1">First <c>Brake</c>.</param>
        /// <param name="break2">Second <c>Brake</c>.</param>
        /// <returns>-1 if first break less then second, 0 if they are equal 
        /// and 1 if first break more then second. </returns>
        public static int Compare(Break break1, Break break2)
        {
            // If first break == null.
            if (break1 == null)
            {
                if (break2 == null)
                    return 0;
                else
                    return -1;
            }
            else if (break2 == null)
                return 1;

            // If both not null.
            if (break1.GetType() != break2.GetType())
                return 0; // Breaks are not of the same type, cant compare.

            else if (break1.GetType() == typeof(TimeWindowBreak))
            {
                TimeWindowBreak br1 = break1 as TimeWindowBreak;
                TimeWindowBreak br2 = break2 as TimeWindowBreak;

                return _TimeWindowBreakComparer(br1, br2);
            }
            else if (break1.GetType() == typeof(WorkTimeBreak))
            {
                WorkTimeBreak br1 = break1 as WorkTimeBreak;
                WorkTimeBreak br2 = break2 as WorkTimeBreak;

                return _WorkTimeBreakComparer(br1, br2);
            }
            else if (break1.GetType() == typeof(DriveTimeBreak))
            {
                DriveTimeBreak br1 = break1 as DriveTimeBreak;
                DriveTimeBreak br2 = break2 as DriveTimeBreak;

                return _DriveTimeBreakComparer(br1, br2);
            }
            else
            {
                // Breaks are of unknown type, cant compare them.
                Debug.Assert(false);
                return 0;
            }
        }

        /// <summary>
        /// Return the name of the ordinal number.
        /// </summary>
        /// <param name="number">Number.</param>
        /// <returns>Name.</returns>
        public static string GetOrdinalNumberName(int number)
        {
            switch (number)
            {
                case 0:
                    return Properties.Resources.NameOfFirst;
                case 1:
                    return Properties.Resources.NameOfSecond;
                case 2:
                    return Properties.Resources.NameOfThird;
                case 3:
                    return Properties.Resources.NameOfFourth;
                case 4:
                    return Properties.Resources.NameOfFifth;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Uppercasing first letter in string.
        /// </summary>
        /// <param name="str">String.</param>
        /// <returns>String with uppercased first letter.</returns>
        public static string UppercaseFirstLetter(string str)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Returns sorted List of <c>Break</c>.
        /// </summary>
        /// <param name="breaks">Breaks.</param>
        /// <returns> Sorted List of <c>Break</c>.</returns>
        public static List<Break> GetSortedList(Breaks breaks)
        {
            List<Break> tempBreaks = new List<Break>();
            for (int i = 0; i < breaks.Count; i++)
            {
                tempBreaks.Add(breaks[i]);
            }
            tempBreaks.Sort(BreaksHelper.Compare);
            return tempBreaks;
        }

        /// <summary>
        /// Returns index of Break in breaks list.
        /// </summary>
        /// <param name="breaks">List of <c>Break</c>.</param>
        /// <param name="br">Break.</param>
        /// <returns>Index of break if break is in Breaks and -1 if not.</returns>
        public static int IndexOf(List<Break> breaks, Break br)
        {
            for (int i = 0; i < breaks.Count; i++)
                if (breaks[i] as Break == br)
                    return i;
            // If no such break - return -1.
            return -1;
        }

        /// <summary>
        /// Returns index of Break in breaks collection.
        /// </summary>
        /// <param name="breaks"><c>Breaks</c>.</param>
        /// <param name="br"><c>Break</c>.</param>
        /// <returns>Index of break if break is in Breaks and -1 if not.</returns>
        public static int IndexOf(Breaks breaks, Break br)
        {
            for (int i = 0; i < breaks.Count; i++)
                if (breaks[i] as Break == br)
                    return i;

            // If no such break - return -1.
            return -1;
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Comparer for two <c>WorkTimeBrakes</c>.
        /// </summary>
        /// <param name="break1">First <c>Brake</c>.</param>
        /// <param name="break2">Second <c>Brake</c>.</param>
        /// <returns>Result of comapring. Breaks are comapred by timeinterval.</returns>
        private static int _WorkTimeBreakComparer(WorkTimeBreak break1, WorkTimeBreak break2)
        {
            if (break1.TimeInterval > break2.TimeInterval)
                return 1;
            else if (break1.TimeInterval < break2.TimeInterval)
                return -1;
            else
                return 0;
        }

        /// <summary>
        /// Comparer for two <c>DriveTimeBrakes</c>.
        /// </summary>
        /// <param name="break1">First <c>Brake</c>.</param>
        /// <param name="break2">Second <c>Brake</c>.</param>
        /// <returns>Result of comapring. Breaks are comapred by timeinterval.</returns>
        private static int _DriveTimeBreakComparer(DriveTimeBreak break1, DriveTimeBreak break2)
        {
            if (break1.TimeInterval > break2.TimeInterval)
                return 1;
            else if (break1.TimeInterval < break2.TimeInterval)
                return -1;
            else
                return 0;
        }

        /// <summary>
        /// Comparer for two <c>TimeWindowBrakes</c>.
        /// </summary>
        /// <param name="break1">First <c>Brake</c>.</param>
        /// <param name="break2">Second <c>Brake</c>.</param>
        /// <returns>Result of comparing. Breaks are compared by EffectiveFrom property.</returns>
        private static int _TimeWindowBreakComparer(TimeWindowBreak break1, TimeWindowBreak break2)
        {
            if (break1.EffectiveFrom > break2.EffectiveFrom)
                return 1;
            else if (break1.EffectiveFrom < break2.EffectiveFrom)
                return -1;
            else
                return 0;
        }

        #endregion
    }
}
