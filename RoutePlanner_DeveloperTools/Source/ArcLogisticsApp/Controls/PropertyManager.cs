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
using System.Windows;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Defines attached properties for passing arbitrary data between elements
    /// of the logical tree.
    /// </summary>
    internal static class PropertyManager
    {
        #region public attached properties identifiers
        /// <summary>
        /// Identifies the "SourceData" attached property.
        /// </summary>
        public static readonly DependencyProperty SourceDataProperty =
            DependencyProperty.RegisterAttached(
                "SourceData",
                typeof(object),
                typeof(PropertyManager),
                new PropertyMetadata(_SourceDataChangedCallback));

        /// <summary>
        /// Identifies the "SourceDataContext" attached property.
        /// </summary>
        public static readonly DependencyProperty SourceDataContextProperty =
            DependencyProperty.RegisterAttached(
                "SourceDataContext",
                typeof(object),
                typeof(PropertyManager),
                new PropertyMetadata(_SourceDataChangedCallback));

        /// <summary>
        /// Identifies the "ReceiverProperty" attached property.
        /// </summary>
        public static readonly DependencyProperty ReceiverPropertyProperty =
            DependencyProperty.RegisterAttached(
                "ReceiverProperty",
                typeof(object),
                typeof(PropertyManager),
                new PropertyMetadata());

        /// <summary>
        /// Identifies the "TargetElement" attached property.
        /// </summary>
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.RegisterAttached(
                "TargetElement",
                typeof(string),
                typeof(PropertyManager),
                new PropertyMetadata(_TargetElementChangedCallback));
        #endregion

        #region public static methods
        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceData"/>
        /// attached property from the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceData"/>
        /// attached property.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="element"/>
        /// parameter is a null reference.</exception>
        public static object GetSourceData(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (object)element.GetValue(SourceDataProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceData"/>
        /// attached property for the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="element"/>
        /// parameter is a null reference.</exception>
        public static void SetSourceData(FrameworkElement element, object value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(SourceDataProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceDataContext"/>
        /// attached property from the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceDataContext"/>
        /// attached property.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="element"/>
        /// parameter is a null reference.</exception>
        public static object GetSourceDataContext(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (object)element.GetValue(SourceDataContextProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceDataContext"/>
        /// attached property for the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="element"/>
        /// parameter is a null reference.</exception>
        public static void SetSourceDataContext(FrameworkElement element, object value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(SourceDataContextProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.ReceiverProperty"/>
        /// attached property from the specified <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.ReceiverProperty"/>
        /// attached property.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/>
        /// parameter is a null reference.</exception>
        public static object GetReceiverProperty(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj.GetValue(ReceiverPropertyProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.ReceiverProperty"/>
        /// attached property for the specified <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/>
        /// parameter is a null reference.</exception>
        public static void SetReceiverProperty(DependencyObject obj, object value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(ReceiverPropertyProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.TargetElement"/>
        /// attached property from the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.TargetElement"/>
        /// attached property.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="element"/>
        /// parameter is a null reference.</exception>
        public static string GetTargetElement(FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (string)element.GetValue(TargetElementProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.TargetElement"/>
        /// attached property for the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="element"/>
        /// parameter is a null reference.</exception>
        public static void SetTargetElement(FrameworkElement element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(TargetElementProperty, value);
        }
        #endregion

        #region private static members
        /// <summary>
        /// Handles changes of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceData"/>
        /// attached property value.
        /// </summary>
        /// <param name="d">The object property value was changed for.</param>
        /// <param name="e">Event data with information about changes of the property value.</param>
        private static void _SourceDataChangedCallback(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element == null)
            {
                return;
            }

            _UpdateSourceDataContext(element);

            _UpdateValue(element);
        }

        /// <summary>
        /// Updates <see cref="P:System.Windows.FrameworkElement.DataContext"/>
        /// property for the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.SourceData"/>
        /// property elements.
        /// </summary>
        /// <param name="element">The element to update <see cref="P:System.Windows.FrameworkElement.DataContext"/>
        /// property for.</param>
        private static void _UpdateSourceDataContext(FrameworkElement element)
        {
            Debug.Assert(element != null);

            var data = PropertyManager.GetSourceData(element);
            if (data == null)
            {
                return;
            }

            var dataContext = PropertyManager.GetSourceDataContext(element);

            var sourceElement = data as FrameworkElement;
            if (sourceElement != null)
            {
                sourceElement.DataContext = dataContext;

                return;
            }

            var sourceList = data as IList;
            if (sourceList != null)
            {
                foreach (FrameworkElement item in sourceList)
                {
                    item.DataContext = dataContext;
                }

                return;
            }
        }

        /// <summary>
        /// Handles changes of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.TargetElement"/>
        /// attached property value.
        /// </summary>
        /// <param name="d">The object property value was changed for.</param>
        /// <param name="e">Event data with information about changes of the property value.</param>
        private static void _TargetElementChangedCallback(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element == null)
            {
                return;
            }

            if (!element.IsLoaded)
            {
                element.Loaded += _ElementLoadedHandler;

                return;
            }

            _UpdateValue(element);
        }

        /// <summary>
        /// Updates value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.ReceiverProperty"/>
        /// attached property for the element with name specified in
        /// the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.TargetElement"/>
        /// property.
        /// </summary>
        /// <param name="element">The framework element providing data.</param>
        private static void _UpdateValue(FrameworkElement element)
        {
            Debug.Assert(element != null);

            var targetElementName = PropertyManager.GetTargetElement(element);
            if (string.IsNullOrEmpty(targetElementName))
            {
                return;
            }

            var targetElement = element.FindNode(targetElementName);
            if (targetElement == null)
            {
                return;
            }

            var sourceData = PropertyManager.GetSourceData(element);
            PropertyManager.SetReceiverProperty(targetElement, sourceData);
        }

        /// <summary>
        /// Handles <see cref="E:System.Windows.FrameworkElement.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The reference to the loaded element.</param>
        /// <param name="e">Event data with information about element loaded event.</param>
        private static void _ElementLoadedHandler(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            var handled = PropertyManager._GetHandledLoadedEvent(element);
            if (!handled)
            {
                _UpdateValue(element);
            }

            PropertyManager._SetHandledLoadedEvent(element, true);
        }

        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.HandledLoadedEvent"/>
        /// attached property from the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.TargetElement"/>
        /// attached property.</returns>
        private static bool _GetHandledLoadedEvent(FrameworkElement element)
        {
            Debug.Assert(element != null);

            return (bool)element.GetValue(HandledLoadedEventProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.PropertyManager.HandledLoadedEvent"/>
        /// attached property for the specified <see cref="T:System.Windows.FrameworkElement"/>.
        /// </summary>
        /// <param name="element">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        private static void _SetHandledLoadedEvent(FrameworkElement element, bool value)
        {
            Debug.Assert(element != null);

            element.SetValue(HandledLoadedEventProperty, value);
        }
        #endregion

        #region private attached properties identifiers
        /// <summary>
        /// Identifies the "HandledLoadedEvent" attached property.
        /// </summary>
        private static readonly DependencyProperty HandledLoadedEventProperty =
            DependencyProperty.RegisterAttached(
                "HandledLoadedEvent",
                typeof(bool),
                typeof(PropertyManager),
                new PropertyMetadata());
        #endregion
    }
}
