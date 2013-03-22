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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.Geocoding;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for GeocodingHint.xaml
    /// </summary>
    internal partial class GeocodingHint : UserControl
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public GeocodingHint()
        {
            InitializeComponent();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initialize hint.
        /// </summary>
        /// <param name="canvas">Canvas to show hint.</param>
        /// <param name="mapControl">Map control.</param>
        /// <param name="addressByPointTool">Reverse geocoding tool.</param>
        public void Initialize(Canvas canvas, MapControl mapControl, AddressByPointTool addressByPointTool)
        {
            Debug.Assert(canvas != null);
            Debug.Assert(mapControl != null);
            Debug.Assert(addressByPointTool != null);

            _canvas = canvas;
            _mapControl = mapControl;
            _addressByPointTool = addressByPointTool;

            // Subscribe to mouse events for dragging hint.
            canvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            canvas.PreviewMouseMove += new MouseEventHandler(_PreviewMouseMove);
            canvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(_PreviewMouseLeftButtonUp);

            INotifyCollectionChanged selection = _mapControl.SelectedItems as INotifyCollectionChanged;
            selection.CollectionChanged += new NotifyCollectionChangedEventHandler(_SelectionCollectionChanged);
        }

        /// <summary>
        /// Show hint.
        /// </summary>
        /// <param name="geocodableItem">Geocodable item.</param>
        /// <param name="candidatesToZoom">Candidates to zoom.</param>
        public void ShowHint(IGeocodable geocodableItem, AddressCandidate[] candidatesToZoom)
        {
            _candidatesToZoom = candidatesToZoom;

            string titleFmt = (string)App.Current.FindResource(GEOCODE_RESULT_NOT_ZOOMED_RESOURCE_NAME);
            TitleText.Text = string.Format(titleFmt, geocodableItem.ToString());

            if (candidatesToZoom == null || candidatesToZoom.Length == 0)
            {
                // If we cant zoom to candidtes - hide corresponding rextblocks.
                ZoomedToText.Visibility = Visibility.Collapsed;
                ZoomedAddress.Visibility = Visibility.Collapsed;
                ZoomedAddressType.Visibility = Visibility.Collapsed;
            }
            else
            {
                string zoomedAddress = GeocodeHelpers.GetZoomedAddress(geocodableItem, candidatesToZoom[0]);

                _InitZoomedAddressTypeText(candidatesToZoom[0]);

                // Show "Zoomed to..." text.
                ZoomedAddress.Text = zoomedAddress;
                ZoomedAddress.Visibility = Visibility.Visible;
                ZoomedToText.Visibility = Visibility.Visible;
            }

            Visibility = Visibility.Visible;

            UpdateLayout();

            Vector toolsPanelOffset = VisualTreeHelper.GetOffset(_mapControl.toolPanel);
            
            // Set top of hint under title, tools.
            Canvas.SetTop(this, _mapControl.toolPanel.Margin.Top + toolsPanelOffset.Y +
                _mapControl.toolPanel.ActualHeight);
            Canvas.SetLeft(this, _mapControl.ActualWidth - this.ActualWidth - _mapControl.toolPanel.Margin.Right);
        }

        /// <summary>
        /// Hide hint.
        /// </summary>
        public void HideHint()
        {
            if (Visibility == Visibility.Visible)
            {
                Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Hide text block if street candidate. Show and fill otherwise.
        /// </summary>
        /// <param name="zoomedCandidate">Candidate to zoom.</param>
        private void _InitZoomedAddressTypeText(AddressCandidate zoomedCandidate)
        {
            LocatorType? locatorType = GeocodeHelpers.GetLocatorTypeOfCandidate(zoomedCandidate);

            // Set text for citystate and zip candidates. Hide otherwise.
            switch (locatorType)
            {
                case LocatorType.CityState:
                    {
                        ZoomedAddressType.Text = (string)App.Current.FindResource(CITYSTATE_LOCATOR_TITLE_RESOURCE_NAME);
                        ZoomedAddressType.Visibility = Visibility.Visible;
                        break;
                    }
                case LocatorType.Zip:
                    {
                        ZoomedAddressType.Text = (string)App.Current.FindResource(ZIP_LOCATOR_TITLE_RESOURCE_NAME);
                        ZoomedAddressType.Visibility = Visibility.Visible;
                        break;
                    }
                case LocatorType.Street:
                    {
                        ZoomedAddressType.Visibility = Visibility.Collapsed;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }
        }

        /// <summary>
        /// React on selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SelectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PushpinToolButton.IsEnabled = _mapControl.SelectedItems.Count > 0;
        }

        /// <summary>
        /// React on left button down. Check click on buttons or start drag otherwise.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check close button pressed.
            if (_IsElementPressed(CloseButton, e))
            {
                // Hide hint.
                Visibility = Visibility.Collapsed;
            }
            // Check tool button pressed.
            else if (_IsElementPressed(PushpinToolButton, e))
            {
                // Change tool state.
                _AddressByPointToolPressed();
            }
            // Check link click.
            else if (_IsElementPressed(ZoomedAddress, e))
            {
                // Zoom to candidates.
                MapExtentHelpers.ZoomToCandidates(_mapControl, _candidatesToZoom);
            }
            else
            {
                // Start drag if mouse was clicked on canvas.
                GeocodingHint geocodingHint = XceedVisualTreeHelper.FindParent<GeocodingHint>((DependencyObject)e.Source);
                if (geocodingHint != null || e.Source == this)
                {
                    _isDown = true;
                    _startPoint = e.GetPosition(_canvas);
                    _canvas.CaptureMouse();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Change tool state.
        /// </summary>
        private void _AddressByPointToolPressed()
        {
            if (_mapControl.SelectedItems.Count > 0 && PushpinToolButton.IsEnabled)
            {
                if (_mapControl.CurrentTool == _addressByPointTool)
                {
                    _mapControl.CurrentTool.Deactivate();
                }
                else
                {
                    _mapControl.CurrentTool = _addressByPointTool;
                }
            }
        }

        /// <summary>
        /// Check is element to find pressed.
        /// </summary>
        /// <param name="elementToFind">Element to find.</param>
        /// <param name="e">Mouse event args.</param>
        /// <returns>Is element was pressed.</returns>
        private bool _IsElementPressed(DependencyObject elementToFind, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == elementToFind)
                return true;

            Type elementType = elementToFind.GetType();

            bool elementFound = false;

            // Find parent.
            DependencyObject element = _FindParent(e.OriginalSource as DependencyObject, elementToFind.GetType());
            if (element == null)
            {
                // Find in grid children. Actual for geocoding tool.
                element = _FindInGridChildren(e.OriginalSource as DependencyObject, elementToFind);
            }

            if (elementToFind == element)
            {
                elementFound = true;
            }

            return elementFound;
        }

        /// <summary>
        /// Find in grid children. Actual for geocoding tool.
        /// </summary>
        /// <param name="dependencyObject">Original source of mouse down event.</param>
        /// <param name="elementToFind">Element to find.</param>
        /// <returns>Founded element.</returns>
        private DependencyObject _FindInGridChildren(DependencyObject dependencyObject, DependencyObject elementToFind)
        {
            DependencyObject result = null;
            
            Grid grid = dependencyObject as Grid;
            if (grid != null)
            {
                foreach (UIElement element in grid.Children)
                {
                    if (element == elementToFind)
                    {
                        result = element;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Method searches visual parent with necessary type from visual tree of DependencyObject.
        /// </summary>
        /// <typeparam name="T">Type of object which should be found.</typeparam>
        /// <param name="from">Source object.</param>
        /// <returns>Found element of visual tree ao null if such element not exist there.</returns>
        private DependencyObject _FindParent(DependencyObject from, Type parentType)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(from);

            if (parent != null && parent.GetType() != parentType)
                parent = _FindParent(parent, parentType);

            return parent;
        }

        /// <summary>
        /// React on mouse move.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                if ((_isDragging == false) && ((Math.Abs(e.GetPosition(_canvas).X - _startPoint.X) >
                    SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(_canvas).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                {
                    _DragStarted();
                }
                if (_isDragging)
                {
                    _DragMoved();
                }
            }
        }

        /// <summary>
        /// React on mouse up.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDown)
            {
                _DragFinished();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Start drag.
        /// </summary>
        private void _DragStarted()
        {
            _isDragging = true;
            _originalLeft = Canvas.GetLeft(this);
            _originalTop = Canvas.GetTop(this);
        }

        /// <summary>
        /// Move dragged element.
        /// </summary>
        private void _DragMoved()
        {
            Point currentPosition = System.Windows.Input.Mouse.GetPosition(_canvas);

            Point currentPositionOnHint = System.Windows.Input.Mouse.GetPosition(this);

            // Set top of hint under title, tools.
            double left = _originalLeft + currentPosition.X - _startPoint.X;
            double top = _originalTop + currentPosition.Y - _startPoint.Y;

            // Deny moving hint out of map control.
            if (left < 0)
                left = 0;
            if (top < 0)
                top = 0;
            if (left + ActualWidth > _mapControl.ActualWidth )
                left = _mapControl.ActualWidth - ActualWidth;
            if (top + ActualHeight > _mapControl.ActualHeight)
                top = _mapControl.ActualHeight - ActualHeight;

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
        }

        /// <summary>
        /// Finish dragging.
        /// </summary>
        private void _DragFinished()
        {
            System.Windows.Input.Mouse.Capture(null);

            _isDragging = false;
            _isDown = false;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Resource name for format string of zoomed geocode result.
        /// </summary>
        private const string GEOCODE_RESULT_ZOOMED_RESOURCE_NAME = "FleetSetupWizardCandidatesNotFoundButZoomedFmt";

        /// <summary>
        /// Resource name for format string of not zoomed geocode result.
        /// </summary>
        private const string GEOCODE_RESULT_NOT_ZOOMED_RESOURCE_NAME = "NoAppropriateCandidateFound";

        /// <summary>
        /// Pushpin button style name.
        /// </summary>
        private const string PUSHPIN_STYLE_NAME = "ToolButtonImageStyle";

        /// <summary>
        /// Path to icon image.
        /// </summary>
        private const string ICON_PATH = @"..\Resources\PNG_Icons\FindAddressByPoint24.png";
        
        /// <summary>
        /// Resource name for zip locator title.
        /// </summary>
        private const string ZIP_LOCATOR_TITLE_RESOURCE_NAME = "ZipLocatorTitle";
        
        /// <summary>
        /// Resource name for city state locator title.
        /// </summary>
        private const string CITYSTATE_LOCATOR_TITLE_RESOURCE_NAME = "CityStateLocatorTitle";

        #endregion

        #region Private members

        /// <summary>
        /// If geocoding is not successful - during geocoding process this property means best candidate to zoom.
        /// </summary>
        private AddressCandidate[] _candidatesToZoom;

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Reverse geocoding tool.
        /// </summary>
        private AddressByPointTool _addressByPointTool;

        /// <summary>
        /// Parent canvas.
        /// </summary>
        private Canvas _canvas;

        /// <summary>
        /// Drag start point.
        /// </summary>
        private Point _startPoint;

        /// <summary>
        /// Left offset on start drag.
        /// </summary>
        private double _originalLeft;

        /// <summary>
        /// Top offset on start drag.
        /// </summary>
        private double _originalTop;

        /// <summary>
        /// Is mouse down.
        /// </summary>
        private bool _isDown;

        /// <summary>
        /// Is dragging in progress.
        /// </summary>
        private bool _isDragging;

        #endregion
    }
}
