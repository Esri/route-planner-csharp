using System.Collections.Generic;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents a single option item.
    /// </summary>
    /// <typeparam name="T">The underlying type of the option item.</typeparam>
    internal sealed class OptionItemViewModel<T> : NotifyPropertyChangedBase
    {
        #region public properties
        /// <summary>
        /// Gets or sets a reference to the object value associated with the option.
        /// </summary>
        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_VALUE);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the current option item is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_IS_SELECTED);
                }
            }
        }
        #endregion

        #region private constants
        /// <summary>
        /// Name of the Value property.
        /// </summary>
        private const string PROPERTY_NAME_VALUE = "Value";

        /// <summary>
        /// Name of the Value property.
        /// </summary>
        private const string PROPERTY_NAME_IS_SELECTED = "IsSelected";
        #endregion

        #region private fields
        /// <summary>
        /// Stores value of the Value property.
        /// </summary>
        private T _value;

        /// <summary>
        /// Stores value of the IsSelected property.
        /// </summary>
        private bool _isSelected;
        #endregion
    }
}
