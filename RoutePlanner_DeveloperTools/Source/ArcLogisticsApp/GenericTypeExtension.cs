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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Type extension class that allows to specify generic types in XAML.
    /// </summary>
    /// <remarks>
    /// You may specify in XAML something like this:
    /// DataType="{app:GenericType TypeName='<Common .NET generic type name>', TypeArguments='<Common .NET or XAML type name>'}"/>
    /// For example, DataType="{app:GenericType TypeName='System.Collections.Generic.ICollection', TypeArguments='domainObjects:VehicleSpecialty'}"/>
    /// Common .NET type name is used instead of XAML type, because in .NET4 IXamlTypeResolver cannot resolve generic type names like "collections:ICollection`1".
    /// </remarks>
    [ContentProperty("TypeArguments")]
    [MarkupExtensionReturnType(typeof(Type))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GenericTypeExtension : TypeExtension
    {
        #region Constructors

        /// <summary>
        /// Creates new instance of <c>GenericTypeExtension</c>.
        /// </summary>
        public GenericTypeExtension()
            : base()
        {
            _typeArguments = new List<Type>();
        }

        /// <summary>
        /// Creates new instance of <c>GenericTypeExtension</c>.
        /// </summary>
        /// <param name="typeName">Generic type name.</param>
        public GenericTypeExtension(String typeName)
            : base(typeName)
        {
            _typeArguments = new List<Type>();
        }

        /// <summary>
        /// Creates new instance of <c>GenericTypeExtension</c>.
        /// </summary>
        /// <param name="typeName">Generic type name.</param>
        /// <param name="typeArgument">Generic type parameter.</param>
        public GenericTypeExtension(String typeName, Type typeArgument)
            : base(typeName)
        {
            _typeArguments = new List<Type>();
            _typeArguments.Add(typeArgument);
        }

        /// <summary>
        /// Creates new instance of <c>GenericTypeExtension</c>.
        /// </summary>
        /// <param name="type">Generic type name.</param>
        public GenericTypeExtension(Type type)
            : base(type)
        {
            _typeArguments = new List<Type>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// List of generic type arguments.
        /// </summary>
        [DefaultValue(null)]
        [TypeConverter(typeof(TypeArgumentsConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList<Type> TypeArguments
        {
            get
            {
                return _typeArguments;
            }
            set
            {
                _typeArguments.Clear();
                _typeArguments.AddRange(value);
            }
        }

        #endregion

        /// <summary>
        /// Returns an object that should be set on the property where this extension is applied.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension. This service is expected to provide results for <c>IXamlTypeResolver</c>. </param>
        /// <returns>The <c>Type</c> object value to set on the property where the extension is applied.</returns>
        public override Object ProvideValue(IServiceProvider serviceProvider)
        {
            // If type arguments are not specified, we can fallback to TypeExtension's implementation.
            if (_typeArguments == null || _typeArguments.Count == 0)
            {
                return base.ProvideValue(serviceProvider);
            }

            // Create open generic type name.
            String typeName = TypeName + "`" + TypeArguments.Count.ToString();

            // Instnatiate open generic type.
            Type genericType = Type.GetType(typeName);
            if (genericType == null)
                throw new InvalidOperationException();

            // Convert arguments to array.
            Type[] arguments = _typeArguments.ToArray();
            if (arguments == null || arguments.Length == 0)
                throw new InvalidOperationException();

            // Create closed generic type.
            Type constructedType = genericType.MakeGenericType(arguments);
            if (constructedType == null)
                throw new InvalidOperationException();

            // Assign new type name and type.
            TypeName = typeName;
            Type = constructedType;
            return constructedType;
        }

        #region private members

        /// <summary>
        /// Generic type arguments.
        /// </summary>
        private List<Type> _typeArguments;

        #endregion
    }

    /// <summary>
    /// Converter class that converts string representation of type list to <c>List<Type></c>.
    /// </summary>
    internal class TypeArgumentsConverter : TypeConverter
    {
        public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            IXamlTypeResolver resolver = context.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
            if (sourceType == typeof(Type) || context != null)
                return true;
            return false;
        }

        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            IXamlTypeResolver resolver = context.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;

            if (value is String)
            {
                String[] stringArray = ((String)value).Split(',');
                List<Type> types = new List<Type>();

                for (Int32 i = 0; i < stringArray.Length; i++)
                {
                    // Try to resolve type using XAML resolver.
                    Type type = resolver.Resolve(stringArray[i].Trim());

                    // Try to resolve using Type class.
                    if (type == null)
                        type = Type.GetType(stringArray[i].Trim());

                    if (type == null)
                        throw new InvalidOperationException();
                    else
                        types.Add(type);
                }
                return types;
            }

            return null;
        }

        public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
