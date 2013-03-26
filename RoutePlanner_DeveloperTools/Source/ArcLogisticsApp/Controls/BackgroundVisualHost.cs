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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Background rootVisual host class.
    /// </summary>
    internal class BackgroundVisualHost : FrameworkElement
    {
        #region IsContentShowing property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Identifies the IsContentShowing dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContentShowingProperty =
            DependencyProperty.Register("IsContentShowing",
                                        typeof(bool),
                                        typeof(BackgroundVisualHost),
                                        new FrameworkPropertyMetadata(false,
                                                                      OnIsContentShowingChanged));

        /// <summary>
        /// Gets or sets if the content is being displayed.
        /// </summary>
        public bool IsContentShowing
        {
            get { return (bool)GetValue(IsContentShowingProperty); }
            set { SetValue(IsContentShowingProperty, value); }
        }

        /// <summary>
        /// IsContentShowing changed handler.
        /// </summary>
        /// <param name="d">Dependency object.</param>
        /// <param name="e">Dependency property changed event arguments.</param>
        static void OnIsContentShowingChanged(DependencyObject d,
                                              DependencyPropertyChangedEventArgs e)
        {
            var bvh = d as BackgroundVisualHost;
            Debug.Assert(null != bvh);

            if (bvh.CreateContent != null)
            {
                if ((bool)e.NewValue)
                    bvh._CreateContentHelper();
                else
                    bvh._HideContentHelper();
            }
        }

        #endregion // IsContentShowing property

        #region CreateContent property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create content delegate.
        /// </summary>
        /// <returns>Created rootVisual object.</returns>
        internal delegate Visual CreateContentFunction();

        /// <summary>
        /// Identifies the CreateContent dependency property.
        /// </summary>
        public static readonly DependencyProperty CreateContentProperty =
            DependencyProperty.Register("CreateContent",
                                        typeof(CreateContentFunction),
                                        typeof(BackgroundVisualHost),
                                        new FrameworkPropertyMetadata(OnCreateContentChanged));

        /// <summary>
        /// Gets or sets the function used to create the rootVisual to display in a background thread.
        /// </summary>
        public CreateContentFunction CreateContent
        {
            get { return (CreateContentFunction)GetValue(CreateContentProperty); }
            set { SetValue(CreateContentProperty, value); }
        }

        /// <summary>
        /// CreateContent changed handler.
        /// </summary>
        /// <param name="d">Dependency object.</param>
        /// <param name="e">Dependency property changed event arguments.</param>
        static void OnCreateContentChanged(DependencyObject d,
                                           DependencyPropertyChangedEventArgs e)
        {
            var bvh = d as BackgroundVisualHost;
            Debug.Assert(null != bvh);

            if (bvh.IsContentShowing)
            {
                bvh._HideContentHelper();
                if (e.NewValue != null)
                    bvh._CreateContentHelper();
            }
        }

        #endregion // CreateContent property

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update layout.
        /// </summary>
        /// <param name="layoutWidth">New layout width.</param>
        /// <param name="text">New text to showing.</param>
        public void UpdateLayout(double layoutWidth, string text)
        {
            _threadedHelper.UpdateLayout(layoutWidth, text);
        }

        #endregion // Public methods

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of rootVisual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return (null != _hostVisual)? 1 : 0; }
        }

        /// <summary>
        /// Gets a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element
        /// in the collection.</param>
        /// <returns>The requested child element. This should not return null; if the provided
        /// index is out of range, an exception is thrown.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (_hostVisual != null && index == 0)
                return _hostVisual;

            throw new IndexOutOfRangeException(); // exception
        }

        /// <summary>
        /// Gets an enumerator for logical child elements of this element.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (_hostVisual != null)
                    yield return _hostVisual;
            }
        }

        /// <summary>
        /// Measures the size in layout required for child elements.
        /// </summary>
        /// <param name="availableSize">The available size that this element can give
        /// to child elements.</param>
        /// <returns>The size that this element determines it needs during layout.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_threadedHelper != null)
                return _threadedHelper.DesiredSize;

            return base.MeasureOverride(availableSize);
        }
        #endregion // Override methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates content helper.
        /// </summary>
        private void _CreateContentHelper()
        {
            _threadedHelper = new ThreadedVisualHelper(CreateContent, _SafeInvalidateMeasure);
            _hostVisual = _threadedHelper.HostVisual;
        }

        /// <summary>
        /// Invalidates measure safe.
        /// </summary>
        private void _SafeInvalidateMeasure()
        {
            Dispatcher.BeginInvoke(new Action(InvalidateMeasure), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Hides content helper.
        /// </summary>
        private void _HideContentHelper()
        {
            if (_threadedHelper != null)
            {
                _threadedHelper.Exit();
                _threadedHelper = null;
                InvalidateMeasure();
            }
        }

        #endregion // Procate methods

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Threaded rootVisual helper class.
        /// </summary>
        internal class ThreadedVisualHelper
        {
            #region Constructor
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Creates a new instance on the <c>ThreadedVisualHelper</c> class.
            /// </summary>
            /// <param name="createContent">Create content delegate.</param>
            /// <param name="invalidateMeasure">Invalide measure action.</param>
            public ThreadedVisualHelper(CreateContentFunction createContent,
                                        Action invalidateMeasure)
            {
                _hostVisual = new HostVisual();
                _createContent = createContent;
                _invalidateMeasure = invalidateMeasure;

                // create and start background GUI thread
                Thread backgroundUI = new Thread(_CreateAndShowContent);
                backgroundUI.SetApartmentState(ApartmentState.STA);
                backgroundUI.Name = "BackgroundVisualHostThread";
                backgroundUI.IsBackground = true;
                backgroundUI.Start();

                _sync.WaitOne();
            }

            #endregion // Constructor

            #region Public properties
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Host rootVisual.
            /// </summary>
            public HostVisual HostVisual { get { return _hostVisual; } }
            /// <summary>
            /// Desired size.
            /// </summary>
            public Size DesiredSize { get; private set; }

            #endregion // Public properties

            #region Public functions
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Update layout delegate.
            /// </summary>
            /// <param name="layoutWidth">New layout width.</param>
            /// <param name="text">New text to showing.</param>
            private delegate void UpdateLayoutDelegate(double layoutWidth, string text);

            /// <summary>
            /// Updates layout.
            /// </summary>
            /// <param name="layoutWidth">New layout width.</param>
            /// <param name="text">New text to showing.</param>
            public void UpdateLayout(double layoutWidth, string text)
            {
                Dispatcher.BeginInvoke(new UpdateLayoutDelegate(_UpdateLayout),
                                       layoutWidth, text);
            }

            /// <summary>
            /// Stop background GUI thread.
            /// </summary>
            public void Exit()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }

            #endregion // Public functions

            #region Private functions
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Updates visibility in rootVisual composition.
            /// </summary>
            /// <param name="rootVisual">Root rootVisual.</param>
            /// <param name="visibility">New visibility.</param>
            /// <remarks>Function use recursion.</remarks>
            private void _UpdateVisibility(Visual rootVisual, Visibility visibility)
            {
                var control = rootVisual as Control;
                if (null != control)
                    control.Visibility = visibility;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootVisual); i++)
                {
                    // retrieve child rootVisual at specified index value
                    var childVisual = (Visual)VisualTreeHelper.GetChild(rootVisual, i);

                    // do processing of the child rootVisual object.
                    if (0 < VisualTreeHelper.GetChildrenCount(childVisual))
                        _UpdateVisibility(childVisual, visibility);
                }
            }

            /// <summary>
            /// Sets text to first child label element.
            /// </summary>
            /// <param name="rootVisual">Root visual.</param>
            /// <param name="text">New visibility.</param>
            /// <remarks>Function use recursion.</remarks>
            private bool _UpdateText(Visual rootVisual, string text)
            {
                bool isUpdated = false;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootVisual); i++)
                {
                    // retrieve child rootVisual at specified index value.
                    var childVisual = (Visual)VisualTreeHelper.GetChild(rootVisual, i);
                    var label = childVisual as Label;
                    if (null != label)
                    {
                        label.Content = text;
                        isUpdated = true;
                    }
                    else
                    {
                        // do processing of the child rootVisual object.
                        // enumerate children of the child rootVisual object.
                        isUpdated = _UpdateText(childVisual, text);
                    }

                    if (isUpdated)
                        break;
                }

                return isUpdated;
            }

            /// <summary>
            /// Updates layout.
            /// </summary>
            /// <param name="rootVisual">Root visual.</param>
            /// <param name="text">New visibility.</param>
            private void _UpdateLayout(double layoutWidth, string text)
            {
                Visibility newVisibility = string.IsNullOrEmpty(text) ?
                                                Visibility.Hidden : Visibility.Visible;
                if (!string.IsNullOrEmpty(text))
                {   // update text
                    _UpdateText(_rootVisual, text); // NOTE: ignore result
                    _rootVisual.Width = layoutWidth;
                }

                // update visibility only if need change
                if (newVisibility != _rootVisual.Visibility)
                    _UpdateVisibility(_rootVisual, newVisibility);

                // force update
                _invalidateMeasure();
            }

            /// <summary>
            /// Creates and show content.
            /// </summary>
            private void _CreateAndShowContent()
            {
                Dispatcher = Dispatcher.CurrentDispatcher;
                var source = new VisualTargetPresentationSource(_hostVisual);
                _sync.Set();
                source.RootVisual = _createContent();
                DesiredSize = source.DesiredSize;
                _rootVisual = source.RootVisual as Control;
                Debug.Assert(null != _rootVisual);
                _invalidateMeasure();

                Dispatcher.Run();
                source.Dispose();
            }

            #endregion // Private functions

            #region Private properties
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Selected dispatcher.
            /// </summary>
            private Dispatcher Dispatcher { get; set; }

            #endregion // Private properties

            #region Private properties
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Host visual.
            /// </summary>
            private readonly HostVisual _hostVisual;
            /// <summary>
            /// Syncronous event.
            /// </summary>
            private readonly AutoResetEvent _sync = new AutoResetEvent(false);
            /// <summary>
            /// Create content delegate.
            /// </summary>
            private readonly CreateContentFunction _createContent;
            /// <summary>
            /// Invalidate measure action.
            /// </summary>
            private readonly Action _invalidateMeasure;
            /// <summary>
            /// Root visual element.
            /// </summary>
            private Control _rootVisual;

            #endregion // Private properties
        }

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Threaded visual helper.
        /// </summary>
        private ThreadedVisualHelper _threadedHelper;
        /// <summary>
        /// Host visual.
        /// </summary>
        private HostVisual _hostVisual;

        #endregion // Private properties
    }
}
