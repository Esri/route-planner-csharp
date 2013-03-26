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

namespace ESRI.ArcLogistics.Utility.ComponentModel
{
    /// <summary>
    /// Indicates that a property depends on another property.
    /// </summary>
    /// <remarks>The attribute is intended to be used on inheritors of the
    /// <see cref="T:ESRI.ArcLogistics.Data.DataObject"/> class. In order
    /// to mark a property as being dependent on another one changes just
    /// add the attribute to it. E.g.
    /// <example>
    /// <![CDATA[
    /// class Stop : DataObject
    /// {
    ///     public DateTime? ArriveTime
    ///     {
    ///         get { return _Entity.ArriveTime; }
    ///         internal set
    ///         {
    ///             _Entity.ArriveTime = value;
    ///             this.NotifyPropertyChanged("ArriveTime");
    ///         }
    ///     }
    ///
    ///     [PropertyDependsOn("ArriveTime")]
    ///     public DateTime? DepartTime
    ///     {
    ///         get
    ///         {
    ///             return this.ArriveTime
    ///                 .AddMinutes(this.WaitTime)
    ///                 .AddMinutes(this.TimeAtStop);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    /// Now whenever the <see cref="P:ESRI.ArcLogistics.DomainObjects.Stop.ArriveTime"/>
    /// property is changed there will be a notification about change of the
    /// <see cref="P:ESRI.ArcLogistics.DomainObjects.Stop.DepartTime"/> property
    /// as well.
    /// </remarks>
    [global::System.AttributeUsage(
        AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = true)]
    internal sealed class PropertyDependsOnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the PropertyDependsOnAttribute class.
        /// </summary>
        /// <param name="propertyName">The name of the source property.</param>
        public PropertyDependsOnAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets name of the source property.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }
    }
}
