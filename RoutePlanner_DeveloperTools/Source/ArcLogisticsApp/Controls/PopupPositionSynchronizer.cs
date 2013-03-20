using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.Views;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class is intended for using in data grid cell editors that have popup panel. Instantiate it once popup is shown the first time.
    /// When cell is focused popup is automatically shown. But since grid also scrolls its content popup is shown in a wrong position.
    /// This class listens table scroll viewer and adjust popup position on each ScrollChanged event.
    /// </summary>
    internal class PopupPositionSynchronizer
    {
        #region Constructors

        /// <summary>
        /// Creates new instance of synchronizer and inits fields.
        /// </summary>
        /// <param name="cellEditor">Cell editor.</param>
        /// <param name="popup">Popup.</param>
        public PopupPositionSynchronizer(FrameworkElement cellEditor, Popup popup)
        {
            Debug.Assert(cellEditor != null);
            Debug.Assert(popup != null);

            _cellEditor = cellEditor;
            _cellEditor.Unloaded += new RoutedEventHandler(_CellEditorUnloaded);

            _popup = popup;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets popup's position relative to cell editor
        /// </summary>
        public void PositionPopupBelowCellEditor()
        {
            Debug.Assert(_cellEditor != null);
            Debug.Assert(_popup != null);

            // Define popup's max height.
            _SetPopupMaxHeight();

            // Define popup's position.
            _PositionPopupBelowCellEditor(_cellEditor, _popup);

            // Subscribe to necessaru events.
            _SubscribeEvents();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Positions popup below cell editor.
        /// </summary>
        /// <param name="cellEditor">Cell editor.</param>
        /// <param name="popup">Popup.</param>
        private void _PositionPopupBelowCellEditor(FrameworkElement cellEditor, Popup popup)
        {
            // Get top left control point in normalized coordinates.
            Point point = _GetNormalizedControlPosition(cellEditor);

            // Position popup to the left bottom corner of the cell editor.
            popup.VerticalOffset = point.Y + cellEditor.ActualHeight;
            popup.HorizontalOffset = point.X;
        }

        /// <summary>
        /// Defines max popup height dependent on popup's position and screen size. 
        /// </summary>
        private void _SetPopupMaxHeight()
        {
            Debug.Assert(_cellEditor != null);
            Debug.Assert(_popup != null);

            // Get top left control point in normalized coordinates.
            Point point = _GetNormalizedControlPosition(_cellEditor);

            // NOTE : System.Windows.SystemParameters.PrimaryScreenHeight returns normalized value (dependent on DPI).
            _popup.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - point.Y - _cellEditor.ActualHeight;
        }

        /// <summary>
        /// Returns normalized (bound with DPI) coordinates of necessary FrameworkElement.
        /// </summary>
        /// <param name="element">FrameworkElement.</param>
        /// <returns>Normalized coordinates.</returns>
        private Point _GetNormalizedControlPosition(FrameworkElement element)
        {
            // Get top left control point in screen coordinates (not normolized according to current DPI).
            Point point = element.PointToScreen(new Point(0, 0));

            // Normalize point.
            PresentationSource source = PresentationSource.FromVisual(element);

            // Get current DPI.
            double dpiX = BASIC_DPI * source.CompositionTarget.TransformToDevice.M11;
            double dpiY = BASIC_DPI * source.CompositionTarget.TransformToDevice.M22;

            point.X *= BASIC_DPI / dpiX;
            point.Y *= BASIC_DPI / dpiY;

            return point;
        }

        /// <summary>
        /// Finds TableViewScrollViewer in visual tree and adds handler to it's "ScrollChanged" event.
        /// Also adds handler to "Unloaded" event of cell editor to remove handlers when it will be unloaded.
        /// </summary>
        private void _SubscribeEvents()
        {
            Debug.Assert(_cellEditor != null);
            Debug.Assert(_popup != null);

            TableViewScrollViewer scrollViewer = XceedVisualTreeHelper.FindScrollViewer(_cellEditor);
            if (scrollViewer != null)
            {
                _scrollViewer = scrollViewer;
                _scrollViewer.ScrollChanged += new ScrollChangedEventHandler(_ScrollViewerScrollChanged);
            }
            else
            {
                // Do nothing, because scroll viewer wasn't found.
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Shifts popup panel.
        /// </summary>
        /// <param name="sender">Scroll viewer instance.</param>
        /// <param name="e">Event args.</param>
        private void _ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // We need to check that event source is TableViewScrollViewer 
            // because event "ScrollChanged" from cell's template ScrollViewer can happen too.
            if (e.OriginalSource is TableViewScrollViewer && _popup != null)
            {
                // Offset popup.
                _popup.VerticalOffset -= e.VerticalChange;
                _popup.HorizontalOffset -= e.HorizontalChange;
            }
        }

        /// <summary>
        /// Unsubscribes from all events and clears private fields.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CellEditorUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from events.
            _cellEditor.Unloaded -= _CellEditorUnloaded;

            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged -= _ScrollViewerScrollChanged;

            // Set variables to null.
            _scrollViewer = null;
            _cellEditor = null;
            _popup = null;
        }

        #endregion

        #region Private memebers

        /// <summary>
        /// Basic monitor DPI.
        /// </summary>
        private const double BASIC_DPI = 96;

        /// <summary>
        /// Cell editor.
        /// </summary>
        private FrameworkElement _cellEditor;

        /// <summary>
        /// Popup panel.
        /// </summary>
        private Popup _popup;

        /// <summary>
        /// Table scroll viewer.
        /// </summary>
        private TableViewScrollViewer _scrollViewer;

        #endregion
    }
}
