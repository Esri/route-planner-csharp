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

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides helper methods for the <see cref="T:System.Enum"/> class.
    /// </summary>
    internal static class EnumHelpers
    {
        /// <summary>
        /// Retrieves values of the specified enumeration type.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration to retrieve values for.</typeparam>
        /// <returns>Collection of <typeparamref name="TEnum"/> enumeration values.</returns>
        /// <exception cref="T:System.ArgumentException"><typeparamref name="TEnum"/>
        /// is not an <see cref="T:System.Enum"/>.</exception>
        public static IEnumerable<TEnum> GetValues<TEnum>()
            where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }
    }

}
