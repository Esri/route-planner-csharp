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

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class for comparing two collections of <c>DataObject</c>.
    /// </summary>
    /// <typeparam name="T">Type of <c>DataObject</c>.</typeparam>
    internal static class DataObjectCollectionsComparer<T>
            where T : DataObject
    {
        /// <summary>
        /// Compare two collections of <c>DataObject</c>s. In collections each item must be unique.
        /// </summary>
        /// <param name="firstCollection">Fist collection to compare.</param>
        /// <param name="secondCollection">Second collection to compare.</param>
        /// <param name="func">Delegate, that compare two objects of type T and return
        /// true if they are equal, false otherwise. First parameter will be from the
        /// first collection, second from the second.</param>
        /// <returns>'True' if collections contain same items, 'false' otherwise.</returns>
        public static bool AreEqual(IList<T> firstCollection,
            IList<T> secondCollection, Func<T, T, bool> func)
        {
            // Both collections must be not null.
            if (firstCollection == null || secondCollection == null)
                return false;

            // Collections must have same items count.
            if (secondCollection.Count != firstCollection.Count)
                return false;

            // If they both dont have items - they are equal.
            else if (secondCollection.Count == 0)
                return true;

            foreach (var dataObject in firstCollection)
            {
                // If second collection doesnt contain object, which 
                // is the same with current object from the first collection - 
                // collections differs.
                if (!secondCollection.Any(
                    delegate(T secondDataObject)
                    {
                        return func(dataObject, secondDataObject);
                    }))
                    return false;
            }

            // If we come here, collections are equal.
            return true;
        }

        
        /// <summary>
        /// Compare two collections of <c>DataObject</c>s. In collections each item must be unique.
        /// </summary>
        /// <param name="firstCollection">Fist collection to compare.</param>
        /// <param name="secondCollection">Second collection to compare.</param>
        /// <returns>'True' if collections contain same items, 'false' otherwise.</returns>
        public static bool AreEqual(IList<T> firstCollection, IList<T> secondCollection)
        {
            return AreEqual(firstCollection, secondCollection, delegate(T firstObject, T secondObject)
            { return firstObject == secondObject; });
        }
    }
}
