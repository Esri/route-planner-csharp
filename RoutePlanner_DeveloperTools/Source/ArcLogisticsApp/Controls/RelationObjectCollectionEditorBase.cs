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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Base class for combo box with checkboxes.
    /// </summary>
    
    [TemplatePart(Name = "PART_CheckBoxStack", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_CellLabel", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]
    
    internal abstract class RelationObjectCollectionEditorBase : ComboBox
    {
        static RelationObjectCollectionEditorBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RelationObjectCollectionEditorBase), new FrameworkPropertyMetadata(typeof(RelationObjectCollectionEditorBase)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        }

        #region Public Properties

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(RelationObjectCollectionEditorBase));

        public static readonly DependencyProperty AllItemsProperty =
            DependencyProperty.Register("AllItems", typeof(IEnumerable), typeof(RelationObjectCollectionEditorBase));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(RelationObjectCollectionEditorBase));

        /// <summary>
        /// Gets/sets LabelTextProperty
        /// </summary>
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        /// <summary>
        /// Gets/sets list of all items available in application.
        /// </summary>
        public IEnumerable AllItems
        {
            get { return (IEnumerable)GetValue(AllItemsProperty); }
            set
            { SetValue(AllItemsProperty, value); }
        }

        /// <summary>
        /// Gets/sets a list of items of current data row.
        /// </summary>
        public IEnumerable SelectedItems
        {
            get
            {
                return (IEnumerable)GetValue(SelectedItemsProperty);
            }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Gets/sets allow auto open property
        /// </summary>
        public bool AutoOpenBlocked
        {
            get;
            set;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Inits handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            this.Loaded += new RoutedEventHandler(CellEditor_Loaded);
            _PopupPanel.Opened += new EventHandler(_PopupPanel_Opened);
            _PopupPanel.Closed += new EventHandler(_PopupPanel_Closed);
            _CheckBoxStack.PreviewKeyDown += new KeyEventHandler(_CheckBoxStack_KeyDown);
            this.PreviewKeyDown += new KeyEventHandler(RelationObjectCollectionEditorBase_PreviewKeyDown);
            this.KeyDown += new KeyEventHandler(RelationObjectCollectionEditorBase_KeyDown);
        }

        /// <summary>
        /// Inits part visual components.
        /// </summary>
        protected void _InitComponents()
        {
            _CheckBoxStack = this.GetTemplateChild("PART_CheckBoxStack") as ListBox;
            _PopupPanel = this.GetTemplateChild("PART_PopupPanel") as Popup;
            _CellContent = this.GetTemplateChild("PART_CellLabel") as TextBlock;
            _TopLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;
        }

        /// <summary>
        /// Create check box for each items in current project.
        /// </summary>
        protected void _InitCheckBoxes()
        {
            if (AllItems != null)
            {
                // List with all selected items.
                List<object> selectedItems = new List<object>();

                if (SelectedItems != null)
                {
                    foreach (Object ssp in SelectedItems)
                    {
                        if (!_CheckAllContent(ssp))
                            _Available.Add(ssp);
                        selectedItems.Add(ssp);
                    }
                }

                // Foreach item in AllItems if item is valid or selected - then add it to 
                // availible items.
                foreach (object item in AllItems)
                {
                    if ((item as IValidatable) != null)
                    {
                        if ((item as IValidatable).IsValid)
                            _Available.Add(item);
                        else if (selectedItems.Contains(item))
                            _Available.Add(item);
                    }
                    else
                        _Available.Add(item);
                }

                _Available.Sort((IComparer)new DataObjectNameComparer());

                foreach (Object speciality in _Available)
                {
                    CheckBox newCheckBox = new CheckBox();
                    newCheckBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    newCheckBox.Style = (Style)Application.Current.FindResource("CheckBoxInRelationObjectCollectionEditorStyle");
                    newCheckBox.Focusable = false;
                    newCheckBox.Checked += new RoutedEventHandler(newCheckBox_Checked);
                    newCheckBox.Unchecked += new RoutedEventHandler(newCheckBox_Unchecked);
                    newCheckBox.GotFocus += new RoutedEventHandler(newCheckBox_GotFocus);
                    newCheckBox.MouseEnter += new MouseEventHandler(newCheckBox_MouseEnter);
                    newCheckBox.Content = speciality.ToString();
                    _CheckBox.Add(newCheckBox);
                    _CheckBoxStack.Items.Add(newCheckBox);
                }

                // mark current object's items as checked
                int count = _GetCollectionSize();

                for (int j = 0; j < count; j++)
                {
                    int i = 0;

                    foreach (CheckBox cb in _CheckBox)
                    {
                        if (_GetIndexItem(j).ToString().Equals(cb.Content))
                            cb.IsChecked = true;
                        i++;
                    }
                }

                _IsLoaded = true;
            }

            UpdateLayout();
        }

        /// <summary>
        /// Adds selected item to object's collection.
        /// </summary>
        /// <param name="cb"></param>
        protected void _AddSelected(CheckBox cb)
        {
            int index = _CheckBox.IndexOf(cb);

            Object current = _Available[index];

            if (!_CheckSelectedContent(current))
                _AddSelectedItem(current);

            _CellContent.Text = _InitText();
        }

        /// <summary>
        /// Removes selected item from object's collection.
        /// </summary>
        /// <param name="cb"></param>
        protected void _RemoveSelected(CheckBox cb)
        {
            int index = _CheckBox.IndexOf(cb);

            Object currentSpecialty = _Available[index];

            if (_CheckSelectedContent(currentSpecialty))
                _RemoveSelectedItem(currentSpecialty);

            _CellContent.Text = _InitText();
        }

        /// <summary>
        /// Method checks is item contents in SelectedElements collection. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract bool _CheckSelectedContent(Object item);

        /// <summary>
        /// Method checks is item contents in collection. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract bool _CheckAllContent(Object item);

        /// <summary>
        /// Method removes selected item from collection.
        /// </summary>
        /// <param name="sp"></param>
        protected abstract void _RemoveSelectedItem(Object item);

        /// <summary>
        /// Method adds item to selectedItems collection 
        /// </summary>
        /// <param name="item"></param>
        protected abstract void _AddSelectedItem(Object item);

        /// <summary>
        /// Inits string with all object's from collection.
        /// </summary>
        protected abstract string _InitText();

        /// <summary>
        /// Returns size of selected items collection.
        /// </summary>
        /// <returns></returns>
        protected abstract int _GetCollectionSize();

        /// <summary>
        /// Returns item by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract Object _GetIndexItem(int index);

        #endregion

        #region Event Handlers

        /// <summary>
        /// Method closes popup if it is opened and user clicked outside the control.
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

        void newCheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            _CheckBoxStack.SelectedItem = cb;
        }

        private void newCheckBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            _CheckBoxStack.SelectedItem = cb;
        }

        private void newCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            _RemoveSelected(cb);
        }

        private void newCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            _AddSelected(cb);
        }

        private void _CheckBoxStack_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape || e.Key == Key.Enter) && _PopupPanel.IsOpen)
            {
                this.IsDropDownOpen = false;
                e.Handled = true;
            }

            // if space pressed on any item - change it's selection state 
            if (e.Key.Equals(Key.Space) && _CheckBoxStack.SelectedItem != null)
                ((CheckBox)_CheckBoxStack.SelectedItem).IsChecked = !((CheckBox)_CheckBoxStack.SelectedItem).IsChecked;
        }

        private void RelationObjectCollectionEditorBase_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape || e.Key == Key.Enter || e.Key == Key.Tab) && _PopupPanel.IsOpen)
                _PopupPanel.IsOpen = false;
        }

        /// <summary>
        /// Handler sets focus to list with check boxes. Allows user to select check box by arrow keys.
        /// </summary>
        /// <param name="sender">Editor.</param>
        /// <param name="e">Key event args.</param>
        private void RelationObjectCollectionEditorBase_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
                _CheckBoxStack.Focus();
        }

        private void _PopupPanel_Opened(object sender, EventArgs e)
        {
            _CheckBoxStack.Focus();

            PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _PopupPanel);

            // Set popup's position.
            synchronizer.PositionPopupBelowCellEditor();
        }

        private void _PopupPanel_Closed(object sender, EventArgs e)
        {
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);
        }

        private void CellEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_IsLoaded)
                _InitCheckBoxes();

            // if control can be opened automatically (property AutoOpenBlocked is false) - open when control loaded
            if (!AutoOpenBlocked)
                this.IsDropDownOpen = true;
        }

        #endregion

        #region Private Fields

        private List<CheckBox> _CheckBox = new List<CheckBox>();

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _TopLevelGrid;

        /// All available items for current object.
        private ArrayList _Available = new ArrayList();

        private ListBox _CheckBoxStack;
        private Popup _PopupPanel;
        private TextBlock _CellContent;

        protected bool _IsLoaded = false;

        #endregion
    }
}
