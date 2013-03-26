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
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Comparer class to sort objects alphabetically by name.
    /// </summary>
    internal class ToStringComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            if (x == null)
                return -1;
            else if (y == null)
                return 1;

            return (string.Compare(x.ToString(), y.ToString()));
        }

        #endregion
    }

    /// <summary>
    /// Comparer class to sort objects by TimeWindow value (object where IsWideopen == true is higher).
    /// </summary>
    internal class TimeWindowComparer : IComparer
    {
        #region IComparer Members

        /// <summary>
        /// Compares two time windows.
        /// </summary>
        /// <param name="object1">First time window.</param>
        /// <param name="object2">Second time window.</param>
        /// <returns>Less than zero - first time window is less than second.
        ///          Zero - time windows are equal.
        ///          Greater than zero  - first time window is greater than second.</returns>
        public int Compare(object object1, object object2)
        {
            int comparisonResult = 0;

            if (object1 == null)
            {
                comparisonResult = - 1;
            }
            else if (object2 == null)
            {
                comparisonResult = 1;
            }
            // Both time windows are not null.
            else
            {
                TimeWindow timeWindow1 = object1 as TimeWindow;
                TimeWindow timeWindow2 = object2 as TimeWindow;

                Debug.Assert(timeWindow1 != null);
                Debug.Assert(timeWindow2 != null);

                // Only 1-st time window is wide open.
                if (timeWindow1.IsWideOpen && !timeWindow2.IsWideOpen)
                    comparisonResult = 1;
                // Only 2-nd time window is wide open.
                else if (!timeWindow1.IsWideOpen && timeWindow2.IsWideOpen)
                    comparisonResult = - 1;
                // Both time windows are wide open.
                else if (timeWindow1.IsWideOpen && timeWindow2.IsWideOpen)
                    comparisonResult = 0;
                // None of time windows is wide open.
                else
                {
                    // Compare time windows start time.
                    if (timeWindow1.EffectiveFrom.Ticks > timeWindow2.EffectiveFrom.Ticks)
                        comparisonResult = 1;
                    else if (timeWindow1.EffectiveFrom.Ticks < timeWindow2.EffectiveFrom.Ticks)
                        comparisonResult = -1;
                    else
                        comparisonResult = 0;
                }
            }

            return comparisonResult;
        }

        #endregion
    }

    /// <summary>
    /// Comparer to sort objects by Break duration.
    /// </summary>
    internal class BreakComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            Debug.Assert(x is Break);
            Debug.Assert(y is Break);

            if (((Break)x).Duration > ((Break)y).Duration)
                return 1;
            else if (((Break)x).Duration < ((Break)y).Duration)
                return -1;
            return 0;
        }

        #endregion
    }

    /// <summary>
    /// Comparer to sort objects by Breaks count.
    /// </summary>
    internal class BreaksComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            Debug.Assert(x is Breaks);
            Debug.Assert(y is Breaks);

            if (((Breaks)x).Count > ((Breaks)y).Count)
                return 1;
            else if (((Breaks)x).Count < ((Breaks)y).Count)
                return -1;
            return 0;
        }

        #endregion
    }

    /// <summary>
    /// Comparer to sort objects by color value.
    /// </summary>
    internal class ColorComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            Debug.Assert(x is Color);
            Debug.Assert(y is Color);

            float xBrightness = ((Color)x).GetBrightness();
            float yBrightness = ((Color)y).GetBrightness();

            int result = _CompareValues(xBrightness, yBrightness);

            // if brightness values are equals
            if (result == 0)
            {
                float xHue = ((Color)x).GetHue();
                float yHue = ((Color)y).GetHue();

                result = _CompareValues(xHue, yHue);

                // if hue values are equals
                if (result == 0)
                {
                    float xSaturation = ((Color)x).GetSaturation();
                    float ySaturation = ((Color)y).GetSaturation();

                    return _CompareValues(xSaturation, ySaturation);
                }
                else
                    return result;
            }
            else
                return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Compares two float values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int _CompareValues(float x, float y)
        {
            if (x > y)
                return 1;
            else if (x < y)
                return -1;
            return 0;
        }

        #endregion
    }

    /// <summary>
    /// Comparer for sorting Stops in ascending OrderSequenceNumber values.
    /// </summary>
    internal class StopsComparer : IComparer<Stop>
    {
        #region IComparer<Stop> Members

        /// <summary>
        /// Method returns 1 if first stop sequence number is greather, -1 if the second stop sequence number is greather and 0 if sequence numbers are equals.
        /// </summary>
        /// <param name="x">First stop.</param>
        /// <param name="y">Second stop.</param>
        /// <returns>1/-1/0 value.</returns>
        int IComparer<Stop>.Compare(Stop x, Stop y)
        {
            if (x.SequenceNumber > y.SequenceNumber)
                return 1;
            else if (x.SequenceNumber < y.SequenceNumber)
                return -1;
            else
                return 0;
        }

        #endregion
    }

    /// <summary>
    /// Comparer for sorting Routes in descending Name and Stops values.
    /// </summary>
    internal class RoutesComparer : IComparer<Route>
    {
        #region IComparer<Route> Members

        /// <summary>
        ///     Method returns 1 if first route name is greather then second and their stops collections are "the same" - 
        /// both routes have stops or both routes haven't stops. Or if first route has stops and the second - hasn't.
        ///     Returns -1 if second route name is greather then first and their stops collections are "the same" - 
        /// both routes have stops or both routes haven't stops. Or if second route has stops and the first - hasn't.
        ///     Returns 0 only if routes names are equals or their stops collections are "the same".
        /// </summary>
        /// <param name="x">First route.</param>
        /// <param name="y">Second route.</param>
        /// <returns>1/-1/0 value.</returns>
        public int Compare(Route x, Route y)
        {
            if (_CompareStops(x, y) == 0) // If both routes contains or not contains stops - return value of comparison of they names.
                return _CompareNames(x, y);
            else if (_CompareNames(x, y) != 0) // If names of routes are not equals - return result of comparison of their stops.
                return _CompareStops(x, y);
            else return 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method compare routes names.
        /// </summary>
        /// <param name="x">First route.</param>
        /// <param name="y">Second route.</param>
        /// <returns>1/-1/0 value.</returns>
        private int _CompareNames(Route x, Route y)
        {
            return string.Compare(x.Name, y.Name);
        }

        /// <summary>
        /// Method compare routes stops collections by stops count.
        /// </summary>
        /// <param name="x">First route.</param>
        /// <param name="y">Second route.</param>
        /// <returns>1/-1/0 value.</returns>
        private int _CompareStops(Route x, Route y)
        {
            if (x.Stops.Count > 0 && y.Stops.Count == 0) // If count of stops in first route is > 0, and second route not contains stops - return 1.
                return -1;
            else if (x.Stops.Count == 0 && y.Stops.Count > 0) // If count of stops in second route is > 0, and first route not contains stops - return -1. 
                return 1;
            else
                return 0; // If both routes contains stops or not contains stops - return 0.
        }

        #endregion
    }
}
