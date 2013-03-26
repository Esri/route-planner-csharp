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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.BreaksHelpers;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_CellLabel", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]

    /// <summary>
    /// Internal logic for break editor.
    /// </summary>
    internal class BreakEditor : ComboBox
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        static BreakEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreakEditor),
                new FrameworkPropertyMetadata(typeof(BreakEditor)));
        }

        #endregion

        #region Public Override Members
        
        /// <summary>
        /// Initializing components, adding controls for editing Brakes.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
            _InitBindings();
            _SetCellLabelText();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Identifies the Breaks dependency property.
        /// </summary>
        public static readonly DependencyProperty BreaksProperty =
            DependencyProperty.Register("Breaks", typeof(Breaks), typeof(BreakEditor));

        /// <summary>
        /// Gets/sets Break value.
        /// </summary>
        public Breaks Breaks
        {
            get
            {
                return (Breaks)GetValue(BreaksProperty);
            }
            set
            {
                SetValue(BreaksProperty, value);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inits components.
        /// </summary>
        private void _InitComponents()
        {
            _Link = this.GetTemplateChild("PART_Ref") as Hyperlink;
            _CellLabel = this.GetTemplateChild("PART_CellLabel") as TextBlock;
            _PopupPanel = this.GetTemplateChild("PART_PopupPanel") as Popup;
            _TopLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;
            _List = this.GetTemplateChild("PART_List") as ListView;
            _BreakEditorColumn = this.GetTemplateChild("PART_BreakEditorColumn") as GridViewColumn;
        }

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            // If breaks changed - update cell label.
            Breaks.PropertyChanged += new PropertyChangedEventHandler(_BreaksPropertyChanged);

            this.Loaded += new RoutedEventHandler(_BreakEditorLoaded);
            this.KeyDown += new KeyEventHandler(_BreakEditorKeyDown);
            this.PreviewMouseLeftButtonDown += 
                new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            _Link.Click += new RoutedEventHandler(_AddBreakLinkClicked);
            _PopupPanel.Opened += new EventHandler(_PopupPanelOpened);
            _PopupPanel.Closed += new EventHandler(_PopupPanelClosed);
            _List.SelectionChanged += new SelectionChangedEventHandler(_ListSelectionChanged);
        }

        /// <summary>
        /// Init Bindings for controls.
        /// </summary>
        private void _InitBindings()
        {
            Binding breaksBinding = new Binding(BREAKS_PROPERTY_NAME);
            breaksBinding.Source = this;
            _List.SetBinding(ListView.ItemsSourceProperty, breaksBinding);
        }

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

        /// <summary>
        /// Sets changed text to cell TextBlock.
        /// </summary>
        private void _SetCellLabelText()
        {
            _CellLabel.Text = Breaks.ToString();
        }

        /// <summary>
        /// Add`s Break to Breaks.
        /// </summary>
        private void _AddBreak()
        {
            if (Breaks.Count == 0)
            {
                // If no breaks in a route, then add break of the default type.
                if (App.Current.Project.BreaksSettings.BreaksType  == BreakType.TimeWindow)
                    Breaks.Add(new TimeWindowBreak());
                else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.DriveTime)
                    Breaks.Add(new DriveTimeBreak());
                else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.WorkTime)
                    Breaks.Add(new WorkTimeBreak());
            }
            // If there are breaks, then add break of the same type.
            else if (Breaks[0].GetType() == typeof(TimeWindowBreak))
                Breaks.Add(new TimeWindowBreak());
            else if (Breaks[0].GetType() == typeof(WorkTimeBreak))
                Breaks.Add(new WorkTimeBreak());
            else if (Breaks[0].GetType() == typeof(DriveTimeBreak))
                Breaks.Add(new DriveTimeBreak());
        }
        #endregion

        #region Private Event Handlers

        /// <summary>
        /// If Breaks was changed - renew cell label text.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // This checking is orkaround, because we cant unsubscribe 
            // from BreaksPropertyChanged when this editor is closing.
            if (Breaks != null)
            {
                // Set label.
                _SetCellLabelText();

                // If we deleted all breaks from this route, set new editor column width.
                if (Breaks.Count == 0)
                    _SetEditorsColumnWidth();
            }
        }

        /// <summary>
        /// Set width of the column with breaks editor corresponding to editor's width.
        /// </summary>
        private void _SetEditorsColumnWidth()
        {
            if (App.Current.Project.BreaksSettings.BreaksType == BreakType.WorkTime)
                _BreakEditorColumn.Width = WorkTimeBreakEditor.EditorWidth + EDITOR_COLUMN_PADDING;
            else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.DriveTime)
                _BreakEditorColumn.Width = DriveTimeBreakEditor.EditorWidth + EDITOR_COLUMN_PADDING;
            else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.TimeWindow)
                _BreakEditorColumn.Width = TimeWindowBreakEditor.EditorWidth + EDITOR_COLUMN_PADDING;
            else
                // Not supported breaks type.
                System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// If ListView selection changed, then set index to -1 to prevent selection on the screen.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _List.SelectedIndex = -1;
        }

        /// <summary>
        /// Add`s break after button pressed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AddBreakLinkClicked(object sender, RoutedEventArgs e)
        {
            if (Breaks.Count < Breaks.MaximumBreakCount)
            {
                _AddBreak();
                _List.UpdateLayout();
            }
        }

        /// <summary>
        /// If Esc or Enter was pressed, closing Popupwindow.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreakEditorKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape || e.Key == Key.Enter) && _PopupPanel.IsOpen)
            {
                this.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        /// <summary>
        /// BreakEditor loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreakEditorLoaded(object sender, RoutedEventArgs e)
        {
            // Sort breaks.
            Breaks.Sort();

            //NOTE: set property IsDropDownOpen to true for open control when it was loaded
            this.IsDropDownOpen = true;
        }

        /// <summary>
        /// PopupPanel closed, need to syncronize it position with cell position.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PopupPanelOpened(object sender, EventArgs e)
        {
            PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _PopupPanel);
            // Set popup's position.
            synchronizer.PositionPopupBelowCellEditor();
        }

        /// <summary>
        /// PopupPanel closed, need to give focus to grid cell.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PopupPanelClosed(object sender, EventArgs e)
        {
            // NOTE : set focus to parent cell for support arrow keys navigation.
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Padding for editor's column.
        /// </summary>
        private double EDITOR_COLUMN_PADDING = 5;

        /// <summary>
        /// Name of the 'Breaks' property.
        /// </summary>
        private const string BREAKS_PROPERTY_NAME = "Breaks";

        #endregion

        #region Private Fields

        /// <summary>
        /// Popup control.
        /// </summary>
        private Popup _PopupPanel;
        
        /// <summary>
        /// CellLabel.
        /// </summary>
        private TextBlock _CellLabel;
        
        /// <summary>
        /// HyperLink for adding new Break.
        /// </summary>
        private Hyperlink _Link;

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _TopLevelGrid;

        /// <summary>
        /// Listview for break editors.
        /// </summary>
        private ListView _List;

        /// <summary>
        /// Grid view's column with break editors.
        /// </summary>
        private GridViewColumn _BreakEditorColumn;

        #endregion
    }

    /// <summary>
    /// Class that selects proper template for edited break.
    /// </summary>
    internal class BreakEditorTemplateSelector : DataTemplateSelector
    {

        #region Public Method

        /// <summary>
        /// Selecting template from dictionary.
        /// </summary>
        /// <param name="item">Break which is editing.</param>
        /// <param name="container">Ignored.</param>
        /// <returns>DataTemplate.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Break br = (Break)item;
            return DataTemplates[br.GetType()];
        } 

        #endregion

        #region Private Static Property

        /// <summary>
        /// Dictionary wich return DataTemplate corresponding to Type.
        /// Dictionary fills on first access.
        /// </summary>
        private static Dictionary<Type, DataTemplate> DataTemplates
        {
            get
            {
                if (_dataTemplates == null)
                {
                    _dataTemplates = new Dictionary<Type, DataTemplate>();

                    _dataTemplates.Add(typeof(TimeWindowBreak),
                        App.Current.MainWindow.FindResource(TIMEWINDOW) as DataTemplate);

                    _dataTemplates.Add(typeof(DriveTimeBreak),
                        App.Current.MainWindow.FindResource(DRIVETIME) as DataTemplate);

                    _dataTemplates.Add(typeof(WorkTimeBreak),
                        App.Current.MainWindow.FindResource(WORKTIME) as DataTemplate);
                }
                return _dataTemplates;
            }
        }

        #endregion

        #region Private Static Field
        
        /// <summary>
        /// Dictionary with templates.
        /// </summary>
        private static Dictionary<Type, DataTemplate> _dataTemplates;
        
        #endregion

        #region Private Constants

        /// <summary>
        /// Templates names.
        /// </summary>
        private const string TIMEWINDOW = "TimeWindowTemplate";
        private const string DRIVETIME = "DriveTimeTemplate";
        private const string WORKTIME = "WorkTimeTemplate";
        
        #endregion
    }
}