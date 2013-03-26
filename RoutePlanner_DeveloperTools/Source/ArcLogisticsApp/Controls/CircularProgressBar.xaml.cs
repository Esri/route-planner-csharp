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
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// A circular type progress bar, that is simliar to popular web based progress bars.
    /// </summary>
    internal partial class CircularProgressBar
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>CircularProgressBar</c> class.
        /// </summary>
        public CircularProgressBar()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, ANIMATION_INTERVAL);
        }

        #endregion // Constructor

        #region Private handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Animation timer tick event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _animationTimerTick(object sender, EventArgs e)
        {
            frame.Source = _bitmapSources[_currentFrame++];
            _currentFrame = _currentFrame % _bitmapSources.Length;
        }

        /// <summary>
        /// Loaded event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Loaded(object sender, RoutedEventArgs e)
        {
            if (null == _bitmapSources)
                _Init();

            _currentFrame = 0; // animation show by start index
        }

        /// <summary>
        /// Unloaded event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Unloaded(object sender, RoutedEventArgs e)
        {
            _Stop();
        }

        /// <summary>
        /// Visible changed event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments (bool value expected).</param>
        private void _VisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;
            if (isVisible)
                _Start();
            else
                _Stop();
        }

        #endregion // Private handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates bitmap source from resource.
        /// </summary>
        /// <param name="assembly">Assembly with image resource.</param>
        /// <param name="name">Image resource name.</param>
        /// <returns>Loaded bitmap source.</returns>
        private BitmapSource _CreateBitmapSource(Assembly assembly, string name)
        {
            Debug.Assert(null != assembly);
            Debug.Assert(!string.IsNullOrEmpty(name));

            // load image resource from assembly
            BitmapSource result = null;
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                // read a stream and decode a PNG image
                PngBitmapDecoder decoder =
                    new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat,
                                         BitmapCacheOption.Default);
                result = decoder.Frames[0];
            }

            return result;
        }

        /// <summary>
        /// Inits state.
        /// </summary>
        /// <remarks>Call only once.</remarks>
        private void _Init()
        {
            Debug.Assert(null == _bitmapSources); // only once

            // create images to animation
            var currAssembly = Assembly.GetExecutingAssembly();

            int imageNumber = RESOURCE_IMAGES.Length;
            _bitmapSources = new BitmapSource[imageNumber];
            for (int index = 0; index < imageNumber; ++index)
            {
                string name = RESOURCE_IMAGES[index];
                BitmapSource image = _CreateBitmapSource(currAssembly, name);

                _bitmapSources[index] = image;
            }
        }

        /// <summary>
        /// Starts animation.
        /// </summary>
        private void _Start()
        {
            Debug.Assert(null != _animationTimer);

            _animationTimer.Tick += _animationTimerTick;
            _animationTimer.Start();
        }

        /// <summary>
        /// Stop animation.
        /// </summary>
        private void _Stop()
        {
            Debug.Assert(null != _animationTimer);

            _animationTimer.Stop();
            _animationTimer.Tick -= _animationTimerTick;
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Assembly resource name prefix.
        /// </summary>
        private const string RESOURCE_NAME_PREFIX =
                                     "ESRI.ArcLogistics.App.Resources.PNG_Icons.ProcessAnimation.";

        /// <summary>
        /// Image resource file names.
        /// </summary>
        private readonly string[] RESOURCE_IMAGES = new string[]
        {
            RESOURCE_NAME_PREFIX + "ProgressA24.png",
            RESOURCE_NAME_PREFIX + "ProgressB24.png",
            RESOURCE_NAME_PREFIX + "ProgressC24.png",
            RESOURCE_NAME_PREFIX + "ProgressD24.png",
            RESOURCE_NAME_PREFIX + "ProgressE24.png",
            RESOURCE_NAME_PREFIX + "ProgressF24.png",
            RESOURCE_NAME_PREFIX + "ProgressG24.png",
            RESOURCE_NAME_PREFIX + "ProgressH24.png"
        };

        /// <summary>
        /// Animation interval.
        /// </summary>
        private const int ANIMATION_INTERVAL = 150;

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Animation bitmap sources.
        /// </summary>
        private BitmapSource[] _bitmapSources;

        /// <summary>
        /// Current frame.
        /// </summary>
        private int _currentFrame;

        /// <summary>
        /// Animation timer.
        /// </summary>
        private readonly DispatcherTimer _animationTimer;

        #endregion // Private fields
    }
}
