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
