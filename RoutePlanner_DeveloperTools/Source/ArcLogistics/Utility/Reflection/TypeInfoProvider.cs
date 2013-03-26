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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ESRI.ArcLogistics.Utility.Reflection
{
    /// <summary>
    /// Provides compiler-checked way to access type information.
    /// </summary>
    /// <typeparam name="T">The type to provide info for.</typeparam>
    internal static class TypeInfoProvider<T>
    {
        /// <summary>
        /// Gets <see cref="PropertyInfo"/> object for property specified with member expression.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The member expression specifying property to get
        /// info for.</param>
        /// <returns>A <see cref="PropertyInfo"/> object for the specified property.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="expression"/> argument
        /// or <paramref name="expression"/>.Body is a null reference.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="expression"/> node
        /// type is not <see cref="ExpressionType.MemberAccess"/>.</exception>
        public static PropertyInfo GetPropertyInfo<TProperty>(
            Expression<Func<T, TProperty>> expression)
        {
            if (expression == null || expression.Body == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            var memberExpression = (MemberExpression)expression.Body;

            return (PropertyInfo)memberExpression.Member;
        }

        /// <summary>
        /// Gets <see cref="PropertyDescriptor"/> object for property specified with member
        /// expression.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The member expression specifying property to get
        /// descriptor for.</param>
        /// <returns>A <see cref="PropertyDescriptor"/> object for the specified property.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="expression"/> argument
        /// or <paramref name="expression"/>.Body is a null reference.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="expression"/> node
        /// type is not <see cref="ExpressionType.MemberAccess"/>.</exception>
        public static PropertyDescriptor GetPropertyDescriptor<TProperty>(
            Expression<Func<T, TProperty>> expression)
        {
            // Arguments are checked inside GetPropertyInfo.
            var propertyInfo = GetPropertyInfo(expression);

            var descriptor = TypeDescriptor.GetProperties(propertyInfo.DeclaringType)
                .Cast<PropertyDescriptor>()
                .First(property => property.Name == propertyInfo.Name);

            return descriptor;
        }
    }
}
