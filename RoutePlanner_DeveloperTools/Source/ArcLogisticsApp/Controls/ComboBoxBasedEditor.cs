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
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// ComboBoxBased editor internal logic.
    /// </summary>
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]
    internal class ComboBoxBasedEditor : ComboBox
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>ComboBoxBasedEditor</c> class.
        /// </summary>
        static ComboBoxBasedEditor()
        {
            var metadate = new FrameworkPropertyMetadata(typeof(ComboBoxBasedEditor));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxBasedEditor), metadate);
        }

        #endregion // Constructors

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public new event EventHandler DropDownClosed;

        #endregion // Public events

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        #region AllItemsProperty property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Identifies the AllItemsProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty AllItemsProperty =
            DependencyProperty.Register("AllItems",
                                        typeof(IEnumerable),
                                        typeof(ComboBoxBasedEditor));

        /// <summary>
        /// Gets/sets collection of all items.
        /// </summary>
        public IEnumerable AllItems
        {
            get { return (IEnumerable)GetValue(AllItemsProperty); }
            set { SetValue(AllItemsProperty, value); }
        }

        #endregion // AllItemsProperty property

        #region SelectedElementProperty property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Identifies the SelectedElementProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedElementProperty =
            DependencyProperty.Register("SelectedElement",
                                        typeof(object),
                                        typeof(ComboBoxBasedEditor));

        /// <summary>
        /// Gets/sets selected item.
        /// </summary>
        public object SelectedElement
        {
            get { return (object)GetValue(SelectedElementProperty); }
            set
            {
                object val = value;
                if ((null != val) && (null != Converter))
                {
                    val = Converter.ConvertBack(val,
                                                val.GetType(),
                                                null,
                                                CultureInfo.CurrentCulture);
                }

                SetValue(SelectedElementProperty, val);
            }
        }

        #endregion // SelectedElementProperty property

        #region ConverterProperty property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Identifies the ConverterProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter",
                                        typeof(IValueConverter),
                                        typeof(ComboBoxBasedEditor));

        /// <summary>
        /// Gets/sets elements converter.
        /// </summary>
        public IValueConverter Converter
        {
            get { return (IValueConverter)GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }

        #endregion // ConverterProperty property

        #endregion // Public properties

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Applyes template. Inits control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _InitComponents();
            _InitEventHandlers();
        }

        #endregion // Override methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets focus to relative cell.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);

            if (DropDownClosed != null)
                DropDownClosed(this, null);
        }

        /// <summary>
        /// Synchroniz popup position with relative cell editor when it loaded
        /// and update elements collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            this.SelectionChanged -= _ComboBox_SelectionChanged;

            _BuildAvailableCollection();

            _SetComboBoxState();

            this.SelectedIndex = Math.Max(_selectedIndex, 0);

            // set popup's position
            var synchronizer = new PopupPositionSynchronizer(this, _PopupPanel);
            synchronizer.PositionPopupBelowCellEditor();

            this.SelectionChanged += new SelectionChangedEventHandler(_ComboBox_SelectionChanged);
        }

        /// <summary>
        /// Updates SelectedElement property by selected item from ComboBox.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var obj = this.SelectedItem as object;
            SelectedElement = obj;
        }

        /// <summary>
        /// Gets current focused item and selects it if key "Tab" was pressed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Key event arguments.</param>
        private void _KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                foreach (object item in this.Items)
                {
                    if (((UIElement)this.ItemContainerGenerator.ContainerFromItem(item)).IsFocused)
                    {
                        this.SelectedItem = item;
                        SelectedElement = item;
                        this.IsDropDownOpen = false;
                    }
                }
            }
        }

        /// <summary>
        /// Closes popup if it is opened and user clicked outside the control.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If mouse is clicked outside the control and popup is shown - we need to close it,
            // so popup will lose its focus and mouse left button down event will come to grid.
            if (!_TopLevelGrid.IsMouseOver && _PopupPanel.IsOpen)
                _PopupPanel.IsOpen = false;
        }

        #endregion // Event handlers

        #region Protected
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets/sets available collection to provide inheritance.
        /// </summary>
        protected ArrayList AvailableCollection
        {
            get { return _availableCollection; }
            set { _availableCollection = value; }
        }

        #endregion // protected properties

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes components.
        /// </summary>
        protected void _InitComponents()
        {
            _PopupPanel = this.GetTemplateChild(POPUP_NAME) as Popup;
            _TopLevelGrid = this.GetTemplateChild(TOP_LEVEL_GRID_NAME) as Grid;

            Debug.Assert(_PopupPanel != null);
            Debug.Assert(_TopLevelGrid != null);
        }

        #region Protected virtual methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds collection of all available items.
        /// </summary>
        protected virtual void _BuildAvailableCollection()
        {
            _availableCollection.Clear();

            if (AllItems != null)
            {
                // If item has validation error - we wont show it.
                ArrayList allItems = new ArrayList();
                foreach (object item in AllItems)
                {
                    if ((item as IValidatable) != null)
                    {
                        if ((item as IValidatable).IsValid)
                            allItems.Add(item);
                    }
                    else
                        allItems.Add(item);
                }

                foreach (object obj in allItems)
                {
                    object realObj = obj;
                    if (null != Converter)
                    {
                        realObj = Converter.Convert(realObj,
                                                    realObj.GetType(),
                                                    null,
                                                    CultureInfo.CurrentCulture);
                    }

                    _availableCollection.Add(realObj);
                }
            }

            // Add selected item to available collection if it's absent there.
            if (SelectedElement != null)
            {
                object selectedObj = SelectedElement;
                if (null != Converter)
                {
                    selectedObj = Converter.Convert(selectedObj,
                                                    selectedObj.GetType(),
                                                    null,
                                                    CultureInfo.CurrentCulture);
                }

                bool found = false;
                foreach (object obj in _availableCollection)
                {
                    if (obj.ToString().Equals(selectedObj.ToString()))
                    {
                        found = true;
                        break; // result founded
                    }
                }

                if (!found)
                    _availableCollection.Add(SelectedElement);
            }
        }

        /// <summary>
        /// Inserts void item to the top of items list. In base class it's empty and
        /// should be overrided when it's necessary to add null row to items list.
        /// </summary>
        protected virtual void _InsertNullItem() { }

        #region Protected virtual event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Updates control state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ComboBoxBasedEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.SelectionChanged -= _ComboBox_SelectionChanged;

            _BuildAvailableCollection();
            _SetComboBoxState();

            this.SelectedIndex = _selectedIndex;
            this.IsDropDownOpen = true;

            this.SelectionChanged += new SelectionChangedEventHandler(_ComboBox_SelectionChanged);
        }

        #endregion // Protected virtual event handlers

        #endregion // Protected virtual methods

        #endregion Protected methods

        #endregion // Protected

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            this.PreviewKeyDown += new KeyEventHandler(_KeyDown);
            this.PreviewMouseLeftButtonDown +=
                new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            this.SelectionChanged += new SelectionChangedEventHandler(_ComboBox_SelectionChanged);
            this.DropDownOpened += new EventHandler(_ComboBox_DropDownOpened);
            this.DropDownClosed += new EventHandler(_ComboBox_DropDownClosed);
            this.Loaded += new RoutedEventHandler(ComboBoxBasedEditor_Loaded);
        }

        /// <summary>
        /// Sets combo box items sources.
        /// </summary>
        private void _SetComboBoxState()
        {
            this.SelectionChanged -= _ComboBox_SelectionChanged;

            _availableCollection.Sort(new DataObjectNameComparer());

            _InsertNullItem();

            object realObj = SelectedElement;
            if (null != Converter && realObj != null)
            {
                realObj =
                    Converter.Convert(realObj, realObj.GetType(), null, CultureInfo.CurrentCulture);
            }
            _selectedIndex = _availableCollection.IndexOf(realObj);

            this.ItemsSource = null;
            this.ItemsSource = _availableCollection;

            this.SelectionChanged += new SelectionChangedEventHandler(_ComboBox_SelectionChanged);
        }

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Resource name of Popup.
        /// </summary>
        private const string POPUP_NAME = "PART_PopupPanel";

        /// <summary>
        /// Resource name of Popup.
        /// </summary>
        private const string TOP_LEVEL_GRID_NAME = "PART_TopLevelGrid";

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Available object collection.
        /// </summary>
        private ArrayList _availableCollection = new ArrayList();
        /// <summary>
        /// Current selected index.
        /// </summary>
        private int _selectedIndex;

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _TopLevelGrid;

        /// <summary>
        /// Drop-down.
        /// </summary>
        private Popup _PopupPanel;

        #endregion // Private fields
    }
}
