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
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class helper for show popup address on map after mouse not moved.
    /// </summary>
    internal class AddressPopupHelper
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public AddressPopupHelper()
        {
            // Init timer for show address under cursor.
            _timer = new Timer(SHOW_ADDRESS_TIMER_INTERVAL);
            _timer.Elapsed += new ElapsedEventHandler(_OnTimedEvent);
        }
        
        #endregion

        #region Public properties

        /// <summary>
        /// Current edited object.
        /// </summary>
        public object EditingObject
        {
            get;
            set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes with map control.
        /// </summary>
        /// <param name="mapControl">Map control.</param>
        public void Initialize(MapControl mapControl)
        {
            _mapControl = mapControl;
            _mapControl.map.MouseUp += new MouseButtonEventHandler(_MouseUp);
            _mapControl.map.ExtentChanging +=new EventHandler<ESRI.ArcGIS.Client.ExtentEventArgs>(_ExtentChanging);
        }

        /// <summary>
        /// Enable timer.
        /// </summary>
        public void Enable()
        {
            _timer.Enabled = true;

            _mouseCoordsFromTool = null;

            App.Current.Geocoder.AsyncReverseGeocodeCompleted += new AsyncReverseGeocodedEventHandler(_AsyncReverseGeocodeCompleted);

            _enabled = true;
        }

        /// <summary>
        /// Disable timer.
        /// </summary>
        public void Disable()
        {
            _timer.Enabled = false;
            _popupPointerPos = null;

            App.Current.Geocoder.AsyncReverseGeocodeCompleted -= new AsyncReverseGeocodedEventHandler(_AsyncReverseGeocodeCompleted);

            _enabled = false;
        }

        /// <summary>
        /// React on mouse event.
        /// </summary>
        /// <param name="mouseX">Mouse X coord.</param>
        /// <param name="mouseY">Mouse Y coord.</param>
        public void OnMouseMove(double mouseX, double mouseY)
        {
            _HideAddressPopups();

            if (_timer.Enabled)
            {
                _timer.Stop();
                _timer.Start();

                _popupPointerPos = null;

                _mouseCoordsFromTool = new ESRI.ArcLogistics.Geometry.Point(mouseX, mouseY);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on mouse up.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MouseUp(object sender, MouseButtonEventArgs e)
        {
            _HideAddressPopups();
        }

        /// <summary>
        /// React on extent changing.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ExtentChanging(object sender, ESRI.ArcGIS.Client.ExtentEventArgs e)
        {
            _HideAddressPopups();
        }

        /// <summary>
        /// Hide maptips.
        /// </summary>
        private void _HideAddressPopups()
        {
            if (_ellipsePopup != null)
            {
                _ellipsePopup.MouseDown -= _EllipsePopupMouseDown;
                _ellipsePopup.IsOpen = false;
                _ellipsePopup = null;
            }

            if (_addressPopup != null)
            {
                _addressPopup.IsOpen = false;
                _addressPopup = null;
            }

            if (_dottedLinePopup != null)
            {
                _dottedLinePopup.IsOpen = false;
                _dottedLinePopup = null;
            }
        }

        /// <summary>
        /// React on show address timer event.
        /// </summary>
        /// <param name="source">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // If geocoding service is available - make request to show address in tooltip
            // otherwise - eat exceptions from service.
            try
            {
                IGeocodable geocodable = EditingObject as IGeocodable;
                if (geocodable != null && _mouseCoordsFromTool.HasValue)
                {
                    // If pointer position changed than make geocode request.
                    if (_popupPointerPos == null || _mouseCoordsFromTool.Value.X != _popupPointerPos.Value.X || _mouseCoordsFromTool.Value.Y != _popupPointerPos.Value.Y)
                    {
                        // Save current position to prevent server calls on each timer event.
                        _popupPointerPos = new ESRI.ArcLogistics.Geometry.Point(
                            _mouseCoordsFromTool.Value.X, _mouseCoordsFromTool.Value.Y);

                        if (_popupPointerPos.HasValue)
                        {
                            _tokenList.Clear();
                            _tokenList.Add(_mapControl.LastCursorPos.Value);

                            App.Current.Geocoder.ReverseGeocodeAsync(_popupPointerPos.Value, _mapControl.LastCursorPos.Value);
                        }
                    }
                }
            }
            catch (AuthenticationException)
            { }
            catch (CommunicationException)
            { }
        }

        /// <summary>
        /// React on reverse geocoding completed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Reverse geocoding completed event args.</param>
        private void _AsyncReverseGeocodeCompleted(object sender, AsyncReverseGeocodedEventArgs e)
        {
            if (_tokenList.Contains(e.UserState))
            {
                _tokenList.Remove(e.UserState);
                _mapControl.Dispatcher.BeginInvoke(new Action(delegate() { _ShowAddress(e); }),
                    DispatcherPriority.Normal);
            }
        }

        /// <summary>
        /// Show popup with geocoded address.
        /// </summary>
        /// <param name="e">Reverse geocoding completed event args.</param>
        private void _ShowAddress(AsyncReverseGeocodedEventArgs e)
        {
            IGeocodable geocodable = _GetCurrentGeocodableObject(_mapControl);
            System.Windows.Point initialPoint = (System.Windows.Point)e.UserState;

            if (geocodable != null && _enabled && _mapControl.LastCursorPos.HasValue &&
                _mapControl.LastCursorPos.Value.X == initialPoint.X &&
                _mapControl.LastCursorPos.Value.Y == initialPoint.Y)
            {
                Point cursorPos = _mapControl.LastCursorPos.Value;

                _HideAddressPopups();

                Rect mapRect = new Rect(0, 0, _mapControl.map.ActualWidth, _mapControl.map.ActualHeight);

                Point objectLocation = _ConvertToScreenPos(e.Location);

                // Create canvas with dotted line.
                Canvas canvas = _CreateDottedLineCanvas(objectLocation, _mapControl.LastCursorPos.Value);

                // Get coordinates of top left of canvas.
                double canvasLeft = cursorPos.X;
                if (objectLocation.X < canvasLeft)
                    canvasLeft = objectLocation.X;

                double canvasTop = cursorPos.Y;
                if (objectLocation.Y < canvasTop)
                    canvasTop = objectLocation.Y;

                Rect canvasRect = new System.Windows.Rect(canvasLeft,
                    canvasTop, canvas.Width, canvas.Height);

                if (mapRect.Contains(canvasRect))
                {
                    canvasRect.Offset(0, -canvasRect.Height);

                    _dottedLinePopup = _CreatePopup(null, canvas, canvasRect);

                    Point popupPosition = _ConvertToScreenPos(e.Location);

                    // Show ellipse popup in position of reverse geocoded object.
                    Rect ellipseRect = new System.Windows.Rect(popupPosition.X - _sizeOfEllipse / 2,
                        popupPosition.Y - 3 * _sizeOfEllipse / 2, _popupWidth, _popupHeigth);
                    _ellipsePopup = _CreatePopup("MapPopupEllipseStyle", null, ellipseRect);

                    // Set cursor to popup. Cursor will be set on ellipse.
                    _ellipsePopup.Cursor = _mapControl.map.Cursor;

                    // Subscribe on mouse down to reraise mouse down on map.
                    _ellipsePopup.MouseDown += new MouseButtonEventHandler(_EllipsePopupMouseDown);

                    // Show address popup higher than IGeocodable object.
                    Rect addressRect = new System.Windows.Rect(popupPosition.X + _popupX,
                        popupPosition.Y - _popupY, _popupWidth, _popupHeigth);

                    string address = _GetAddressValue(e.Address);
                    _addressPopup = _CreatePopup("MapPopupStyle", address, addressRect);
                }
            }
        }

        /// <summary>
        /// React on mouse down.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _EllipsePopupMouseDown(object sender, MouseButtonEventArgs e)
        {
            _HideAddressPopups();
            // Reraise mouse down on map.
            _mapControl.map.RaiseEvent(e);
        }

        /// <summary>
        /// Convert from mappoint to screen coordinates.
        /// </summary>
        /// <returns>Converted from mappoint to screen coordinates.</returns>
        private System.Windows.Point _ConvertToScreenPos(ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint)
        {
            ESRI.ArcLogistics.Geometry.Point projectedPoint = new ESRI.ArcLogistics.Geometry.Point(
                mapPoint.X, mapPoint.Y);

            if (_mapControl.Map.SpatialReferenceID.HasValue)
            {
                projectedPoint = WebMercatorUtil.ProjectPointToWebMercator(projectedPoint,
                    _mapControl.Map.SpatialReferenceID.Value);
            }

            MapPoint location = new MapPoint(projectedPoint.X, projectedPoint.Y);
            Point point = _mapControl.map.MapToScreen(location);

            return point;
        }

        /// <summary>
        /// Create canvas with dotted line.
        /// </summary>
        /// <param name="objectLocation">Reverse geocoded object location.</param>
        /// <param name="cursorPos">Cursor position.</param>
        /// <returns>Canvas with dotted line.</returns>
        private Canvas _CreateDottedLineCanvas(System.Windows.Point objectLocation, System.Windows.Point cursorPos)
        {
            Canvas canvas = new Canvas();

            canvas.Width = Math.Abs(cursorPos.X - objectLocation.X);
            canvas.Height = Math.Abs(cursorPos.Y - objectLocation.Y);

            if (canvas.Height < _dottedLineWidth)
                canvas.Height = _dottedLineWidth;
            if (canvas.Width < _dottedLineWidth)
                canvas.Width = _dottedLineWidth;
            Line line = new Line();

            line.Y1 = 0;
            line.Y2 = canvas.Height;

            // Check coordinate quarter and set correct angle.
            if (Math.Sign((cursorPos.X - objectLocation.X) * (cursorPos.Y - objectLocation.Y)) < 0)
            {
                line.X2 = 0;
                line.X1 = canvas.Width;
            }
            else
            {
                line.X1 = 0;
                line.X2 = canvas.Width;
            }

            line.Style = (Style)App.Current.FindResource("MapPopupDottedLineStyle");

            canvas.Children.Add(line);

            return canvas;
        }

        /// <summary>
        /// Create and init popup.
        /// </summary>
        /// <param name="styleName">Popup style.</param>
        /// <param name="child">Child of popup.</param>
        /// <param name="rect">Rect to show popup.</param>
        /// <returns>Created popup.</returns>
        private Popup _CreatePopup(string styleName, object child, System.Windows.Rect rect)
        {
            Popup popup = new Popup();
            popup.AllowsTransparency = true;

            ContentControl control = new ContentControl();

            if (styleName != null)
                control.Style = (System.Windows.Style)App.Current.FindResource(styleName);
            control.Content = child;

            popup.Child = control;
            popup.PlacementTarget = _mapControl;
            popup.PlacementRectangle = rect;
            popup.Visibility = Visibility.Visible;
            popup.IsOpen = true;

            return popup;
        }

        /// <summary>
        /// Get address value, divided into two rows.
        /// </summary>
        /// <param name="address">Address to divide.</param>
        /// <returns>Address value, divided into two rows.</returns>
        private string _GetAddressValue(Address address)
        {
            string result;

            // Show all address, except address line.
            string fullAddress = address.FullAddress;

            // Address line is before first separator.
            int index = fullAddress.IndexOf(',');

            if (index != -1)
            {
                result = address.FullAddress.Substring(0, index).Trim();
                result += System.Environment.NewLine;
                result += fullAddress.Substring(index + 1, fullAddress.Length - index - 1).Trim();
                result = result.Trim();
            }
            else
            {
                result = fullAddress;
            }

            return result;
        }

        /// <summary>
        /// Get current geocodable object.
        /// </summary>
        /// <param name="mapctrl">Map control.</param>
        /// <returns>Current geocodable object.</returns>
        private IGeocodable _GetCurrentGeocodableObject(MapControl mapctrl)
        {
            // Current geocodable object is edited object or,
            // in case of address by point tool activated, selected object.
            IGeocodable result = mapctrl.EditedObject as IGeocodable;

            if (result == null && mapctrl.CurrentTool is AddressByPointTool)
            {
                Debug.Assert(mapctrl.SelectedItems.Count == 1);
                result = mapctrl.SelectedItems[0] as IGeocodable;
            }

            return result;
        }

        #endregion

        #region Private constants
        
        /// <summary>
        /// Interval to make asinchronous reverse geocoding request.
        /// </summary>
        private const int SHOW_ADDRESS_TIMER_INTERVAL = 500;

        #endregion

        #region Private static fields

        /// <summary>
        /// X coord of popup control.
        /// </summary>
        private static int _popupX = (int)Application.Current.FindResource("AddressTipPopupX");

        /// <summary>
        /// Y coord of popup control.
        /// </summary>
        private static int _popupY = (int)Application.Current.FindResource("AddressTipPopupY");

        /// <summary>
        /// Width of popup control.
        /// </summary>
        private static int _popupWidth = (int)Application.Current.FindResource("AddressTipPopupWidth");

        /// <summary>
        /// Heigth of popup control.
        /// </summary>
        private static int _popupHeigth = (int)Application.Current.FindResource("AddressTipPopupHeigth");

        /// <summary>
        /// Size of popup ellipse.
        /// </summary>
        private static int _sizeOfEllipse = (int)Application.Current.FindResource("SizeOfEllipse");

        /// <summary>
        /// Width of dotted line.
        /// </summary>
        private static int _dottedLineWidth = (int)Application.Current.FindResource("DottedLineWidth");

        #endregion

        #region Private members

        /// <summary>
        /// Is helper enabled.
        /// </summary>
        private bool _enabled;
        
        /// <summary>
        /// Popup to show address.
        /// </summary>
        private Popup _addressPopup;

        /// <summary>
        /// Popup to show ellipse.
        /// </summary>
        private Popup _ellipsePopup;

        /// <summary>
        /// Popup to show dotted line.
        /// </summary>
        private Popup _dottedLinePopup;

        /// <summary>
        /// Map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Timer to show address under cursor.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Address popup position.
        /// </summary>
        private ESRI.ArcLogistics.Geometry.Point? _popupPointerPos;

        /// <summary>
        /// Tools mouse coords.
        /// </summary>
        private ESRI.ArcLogistics.Geometry.Point? _mouseCoordsFromTool;

        /// <summary>
        /// List of token.
        /// </summary>
        private List<object> _tokenList = new List<object>();

        #endregion
    }
}
