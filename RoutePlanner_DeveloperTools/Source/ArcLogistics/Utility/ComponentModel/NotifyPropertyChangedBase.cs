using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Utility.Reflection;

namespace ESRI.ArcLogistics.Utility.ComponentModel
{
    /// <summary>
    /// Provides implementation of the <see cref="T:System.ComponentModel.INotifyPropertyChanged"/>
    /// interface.
    /// </summary>
    [Serializable]
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the NotifyPropertyChangedBase class.
        /// </summary>
        /// <exception cref="T:ESRI.ArcLogistics.Utility.ComponentModel.PropertyDependenciesCycleException">
        /// class properties dependencies contain a cycle.</exception>
        protected NotifyPropertyChangedBase()
        {
            _propertyDependencies = _InitPropertyDependencies();
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Fired when property value is changed.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region internal methods
        /// <summary>
        /// Finalizes this instance deserialization.
        /// </summary>
        /// <param name="context">The reference to the serialization stream description
        /// object.</param>
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            this.PropertyChanged = delegate { };
            _propertyDependencies = _InitPropertyDependencies();
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Raises <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
        /// event.
        /// </summary>
        /// <param name="e">The arguments for the event.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged(this, e);

            List<string> dependentProperties;
            if (_propertyDependencies.TryGetValue(e.PropertyName, out dependentProperties))
            {
                foreach (var propertyName in dependentProperties)
                {
                    this.NotifyPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Notifies about change of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region private methods
        /// <summary>
        /// Finds and initializes all property dependencies expressed via
        /// <see cref="T:ESRI.ArcLogistics.Data.PropertyDependsOnAttribute"/> attribute.
        /// </summary>
        /// <returns>Dictionary mapping names of source properties to a list
        /// of dependendent property names.</returns>
        private IDictionary<string, List<string>> _InitPropertyDependencies()
        {
            var bindingFlags =
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public;
            var dependentProperties =
                from property in this.GetType().GetProperties(bindingFlags)
                from attribute in property.GetCustomAttributes<PropertyDependsOnAttribute>()
                where !string.IsNullOrEmpty(attribute.PropertyName)
                group property.Name by attribute.PropertyName;

            var dependencies = dependentProperties.ToDictionary(
                group => group.Key,
                group => group.ToList());

            var cycleDetector = new CycleDetector<string>(property =>
            {
                List<string> peers;
                if (dependencies.TryGetValue(property, out peers))
                {
                    return peers;
                }

                return Enumerable.Empty<string>();
            });
            foreach (var property in dependencies.Keys)
            {
                var cycle = cycleDetector.FindCycle(property);
                if (cycle.Any())
                {
                    throw new PropertyDependenciesCycleException(
                        property,
                        cycle);
                }
            }

            return dependencies;
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores property dependencies, the key is a name of the property
        /// whose value changes trigger notifications for all properties with
        /// names contained in the value list.
        /// </summary>
        [NonSerialized]
        private IDictionary<string, List<string>> _propertyDependencies;
        #endregion
    }
}
