using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ESRI.ArcLogistics.Utility.Reflection
{
    /// <summary>
    /// Provides methods simplifying usage of
    /// <see cref="T:System.Reflection.ICustomAttributeProvider"/> objects.
    /// </summary>
    internal static class CustomAttributeProviderExtensions
    {
        /// <summary>
        /// Gets collection of custom attributes of the specified type defined
        /// for the specified source.
        /// </summary>
        /// <typeparam name="TAttribute">The type of custom attributes.</typeparam>
        /// <param name="source">The reference to the reflection object to search
        /// custom attributes for.</param>
        /// <param name="inherit">Specifies whether to search for custom attributes
        /// in the hierarchy chain.</param>
        /// <returns>A collection of custom attributes of the
        /// <typeparamref name="TAttrubyte"/> type.</returns>
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
            this ICustomAttributeProvider source,
            bool inherit)
        {
            Debug.Assert(source != null);

            return source
                .GetCustomAttributes(typeof(TAttribute), inherit)
                .Cast<TAttribute>();
        }

        /// <summary>
        /// Gets collection of custom attributes of the specified type defined
        /// for the specified source without searching in the hierarchy chain.
        /// </summary>
        /// <typeparam name="TAttribute">The type of custom attributes.</typeparam>
        /// <param name="source">The reference to the reflection object to search
        /// custom attributes for.</param>
        /// <returns>A collection of custom attributes of the
        /// <typeparamref name="TAttrubyte"/> type.</returns>
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(
            this ICustomAttributeProvider source)
        {
            Debug.Assert(source != null);

            return source.GetCustomAttributes<TAttribute>(false);
        }
    }
}
