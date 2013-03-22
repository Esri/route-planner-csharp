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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility.Reflection;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for ProxySettingsEditor.xaml
    /// </summary>
    internal partial class ProxySettingsEditor : UserControl, IWeakEventListener
    {
        public ProxySettingsEditor()
        {
            InitializeComponent();

            this.DataContextChanged += delegate
            {
                _DataContextChanged();
            };
        }

        /// <summary>
        /// Handle template application.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // The PasswordBox is cleared upon navigating event, so we need this workaround
            // to prevent this.
            var svc = NavigationService.GetNavigationService(this.ProxyPassword);
            svc.Navigating += delegate
            {
                _NavigationServiceNavigating();
            };
        }

        /// <summary>
        /// Handles weak event reception.
        /// </summary>
        /// <param name="managerType">The type of the event manager calling this method.</param>
        /// <param name="sender">The event sender object.</param>
        /// <param name="e">The event arguments object.</param>
        /// <returns>true if and only if the event was handled.</returns>
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (sender != this.DataContext)
            {
                return true;
            }

            var propertyChangedArgs = e as PropertyChangedEventArgs;
            if (propertyChangedArgs == null)
            {
                return true;
            }

            if (propertyChangedArgs.PropertyName == PASSWORD_PROPERTY_NAME && !_changing)
            {
                _OnPasswordChanged();
            }

            return true;
        }

        #region private methods
        /// <summary>
        /// Handles navigation events to prevent password clearing.
        /// </summary>
        private void _NavigationServiceNavigating()
        {
            var model = this.DataContext as ProxySettings;
            if (model == null)
            {
                return;
            }

            model.Password = _password;
        }

        /// <summary>
        /// Handles data context password changes.
        /// </summary>
        private void _OnPasswordChanged()
        {
            var model = this.DataContext as ProxySettings;
            if (model == null)
            {
                return;
            }

            this.ProxyPassword.Password = model.Password;
        }

        /// <summary>
        /// Handles data context changes.
        /// </summary>
        private void _DataContextChanged()
        {
            var model = this.DataContext as ProxySettings;
            if (model == null)
            {
                return;
            }

            PropertyChangedEventManager.AddListener(model, this, PASSWORD_PROPERTY_NAME);
            _OnPasswordChanged();
        }

        /// <summary>
        /// Handles password changes.
        /// </summary>
        /// <param name="sender">The reference to the event sender.</param>
        /// <param name="e">The reference to the event arguments object.</param>
        private void _PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            var element = sender as PasswordBox;
            if (element == null)
            {
                return;
            }

            var model = element.DataContext as ProxySettings;
            if (model == null)
            {
                return;
            }

            _changing = true;
            _password = model.Password;
            model.Password = element.Password;
            _changing = false;
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the <see cref="ProxySettings.Password"/> property.
        /// </summary>
        private static readonly string PASSWORD_PROPERTY_NAME =
            TypeInfoProvider<ProxySettings>.GetPropertyInfo(_ => _.Password).Name;
        #endregion

        #region private fields
        /// <summary>
        /// The last password set for the proxy.
        /// </summary>
        private string _password;

        /// <summary>
        /// A value indicating if the password is being changed.
        /// </summary>
        private bool _changing;
        #endregion
    }
}
