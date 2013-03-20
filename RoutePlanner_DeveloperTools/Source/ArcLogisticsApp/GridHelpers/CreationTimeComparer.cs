using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Comparer for sorting DataObjects in descending CreationTime values.
    /// </summary>
    class CreationTimeComparer<T> : IComparer<T> where T : ESRI.ArcLogistics.Data.DataObject
    {
        #region IComparer<T> Members
        /// <summary>
        /// Compares two DataObjects by CreationTime values.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>CreationTime Less than zero x is less than y. Zero x equals y. Greater
        /// than zero x is greater than y.</returns>
        public int Compare(T x, T y)
        {
            if (x.CreationTime == null || y.CreationTime == null)
                return 0;
            else if (x.CreationTime < y.CreationTime)
                return 1;
            else if (x.CreationTime > y.CreationTime)
                return -1;
            else
                return 0;
        }

        #endregion
    }
}
