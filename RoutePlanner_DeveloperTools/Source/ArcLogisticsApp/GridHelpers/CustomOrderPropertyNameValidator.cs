using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

using ESRI.ArcLogistics.Export;
using ESRI.ArcLogistics.Properties;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Class implements interface ICustomOrderPropertyNameValidator used for validation
    /// of custom order property name.
    /// </summary>
    internal class CustomOrderPropertyNameValidator : ICustomOrderPropertyNameValidator
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of CustomOrderPropertyNameValidator.
        /// </summary>
        /// <param name="customOrderProperties">Observable collection of custom order properties.</param>
        public CustomOrderPropertyNameValidator(ObservableCollection<CustomOrderProperty> customOrderProperties)
        {
            Debug.Assert(customOrderProperties != null);

            _customOrderProperties = customOrderProperties;

            // Set handler for CollectionChanged event.
            _customOrderProperties.CollectionChanged +=
                new NotifyCollectionChangedEventHandler(_customOrderProperties_CollectionChanged);
        }

        #endregion Constructors

        #region ICustomOrderPropertyNameValidator methods

        /// <summary>
        /// Performs validation of custom order property name.
        /// </summary>
        /// <param name="propertyName">Custom order property name.</param>
        /// <param name="errorMessage">Output parameter to store error message.
        /// If property name is valid this parameter is set to empty string.</param>
        /// <returns>True - if property name is valid, otherwise - false.</returns>
        public bool Validate(string propertyName, out string errorMessage)
        {
            bool validationResult = false;

            errorMessage = string.Empty;

            // If property name is empty.
            if (string.IsNullOrEmpty(propertyName))
            {
                validationResult = false;

                errorMessage = string.Format(Messages.Error_NullName,
                                             App.Current.GetString("CustomOrderProperty"));
            }
            // Property name is not empty, check it's uniqueness.
            else
            {
                // Get collection of custom order properties names.
                ICollection<string> names = _GetCurrentNamesCollection();

                // Check if collection contains only one item with given name.
                validationResult = _IsPropertyNameUnique(propertyName, names);

                if (validationResult)
                {
                    // Check that current name is not reserved.
                    ExportValidator exportValidator = _GetExportValidator();
                    validationResult =
                        exportValidator.IsCustomOrderFieldNameUnique(propertyName, names);

                    if (!validationResult)
                    {
                        // Name is reserved.
                        errorMessage = string.Format(Messages.Error_ReservedCustomOrderPropertyName,
                                                     App.Current.GetString("CustomOrderProperty"),
                                                     propertyName);
                    }
                }
                // Name is not unique in current collection.
                else
                {
                    errorMessage = string.Format(Messages.Error_DuplicateName,
                                                 App.Current.GetString("CustomOrderProperty"),
                                                 propertyName);
                }
            }

            return validationResult;
        }

        #endregion ICustomOrderPropertyNameValidator methods

        #region Private methods

        /// <summary>
        /// Checks if property with given name is unique in collection.
        /// </summary>
        /// <param name="propertyName">Name of a property.</param>
        /// <param name="names">Current property name collection.</param>
        /// <returns>True - if collection contains only one item with given name, otherwise - false.</returns>
        private bool _IsPropertyNameUnique(string propertyName, ICollection<string> names)
        {
            Debug.Assert(propertyName != null);
            Debug.Assert(names != null);

            // Counter of items with given name.
            int equalNamesCount = 0;

            // Look for custom order property with the same name in collection.
            foreach (string name in names)
            {
                // Count of items in collection with given name.
                if (name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase))
                    equalNamesCount++;

                // If at least two items found - stop looking through collection.
                if (equalNamesCount > 1)
                    break;
            }

            bool checkResult = equalNamesCount <= 1;
            return checkResult;
        }

        /// <summary>
        /// Sets given name validator for collection of items.
        /// </summary>
        /// <param name="items">Collection of items (CustomOrderProperty objects).</param>
        /// <param name="nameValidator">Name validator.</param>
        private void _SetNameValidatorForItems(IList items, ICustomOrderPropertyNameValidator nameValidator)
        {
            if (items == null)
                return;

            // Iterate thor items in collection.
            foreach (object item in items)
            {
                // Get current item from collection, it should be CustomOrderProperty object.
                CustomOrderProperty orderProperty = item as CustomOrderProperty;
                Debug.Assert(orderProperty != null);

                // Set name validator for custom order property.
                orderProperty.NameValidator = nameValidator;
            }
        }

        /// <summary>
        /// Gets export validator.
        /// Create if need or use created.
        /// </summary>
        /// <returns>Export validator.</returns>
        private ExportValidator _GetExportValidator()
        {
            if (null == _exportValidator)
            {
                App app = App.Current;
                _exportValidator =
                    new ExportValidator(app.Project.CapacitiesInfo, app.Geocoder.AddressFields);
            }

            Debug.Assert(_exportValidator != null);
            return _exportValidator;
        }

        /// <summary>
        /// Gets current collection of custom order properties names.
        /// </summary>
        /// <returns>Collection of names.</returns>
        private ICollection<string> _GetCurrentNamesCollection()
        {
            Debug.Assert(null != _customOrderProperties);

            IList<string> names = new List<string>(_customOrderProperties.Count);

            foreach (CustomOrderProperty orderProperty in _customOrderProperties)
            {
                if (orderProperty.Name != string.Empty)
                    names.Add(orderProperty.Name);
            }

            return names;
        }

        #endregion Private methods

        #region Private event handlers

        /// <summary>
        /// Handler for event rised when _customOrderProperties collection is changed.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">Information about the event.</param>
        private void _customOrderProperties_CollectionChanged(Object sender,
                                                              NotifyCollectionChangedEventArgs eventArgs)
        {
            switch(eventArgs.Action)
            {
                // New items were added.
                case NotifyCollectionChangedAction.Add:
                    _SetNameValidatorForItems(eventArgs.NewItems, this);
                    break;

                // Some items were deleted.
                case NotifyCollectionChangedAction.Remove:
                    _SetNameValidatorForItems(eventArgs.OldItems, null);
                    break;

                // Some items were replaced.
                case NotifyCollectionChangedAction.Replace:
                // Collection was changed dramatically.
                case NotifyCollectionChangedAction.Reset:
                    _SetNameValidatorForItems(eventArgs.NewItems, this);
                    _SetNameValidatorForItems(eventArgs.OldItems, null);
                    break;

                // Some items were moved within collection.
                case NotifyCollectionChangedAction.Move:
                    // No action is required.
                    break;

                default:
                    // Unknown enumeration value, this case should never happen.
                    Debug.Assert(false);
                    break;
            }
        }

        #endregion Private event handlers

        #region Private Fields

        /// <summary>
        /// Observable collection of custom order properties.
        /// </summary>
        private ObservableCollection<CustomOrderProperty> _customOrderProperties;

        /// <summary>
        /// Export validator.
        /// </summary>
        private ExportValidator _exportValidator;

        #endregion Private Fields
    }
}
