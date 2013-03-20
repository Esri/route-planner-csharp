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
