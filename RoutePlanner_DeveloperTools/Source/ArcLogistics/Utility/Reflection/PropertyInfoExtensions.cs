using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Utility.Reflection
{
    internal static class PropertyInfoExtensions
    {
        /// <summary>
        /// Gets <see cref="PropertyDescriptor"/> object for the specified
        /// <see cref="PropertyInfo"/> object.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object for the property
        /// to get descriptor for.</param>
        /// <returns>A <see cref="PropertyDescriptor"/> object for the specified property.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="propertyInfo"/> argument
        /// is a null reference.</exception>
        public static PropertyDescriptor GetDescriptor(this PropertyInfo propertyInfo)
        {
            CodeContract.RequiresNotNull("propertyInfo", propertyInfo);

            var descriptor = TypeDescriptor.GetProperties(propertyInfo.DeclaringType)
                .Cast<PropertyDescriptor>()
                .First(property => property.Name == propertyInfo.Name);

            return descriptor;
        }
    }
}
