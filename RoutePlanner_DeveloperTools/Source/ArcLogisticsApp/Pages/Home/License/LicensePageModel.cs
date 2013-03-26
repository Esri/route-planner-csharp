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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Provides model for the <see cref="T:ESRI.ArcLogistics.App.Pages.LicensePage"/>
    /// view.
    /// </summary>
    internal sealed class LicensePageModel :
        NotifyPropertyChangedBase,
        ILoginViewHost
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LicensePageModel class.
        /// </summary>
        /// <param name="servers">Collection of servers to provide separate
        /// authentication for.</param>
        /// <param name="messenger">The messenger object to be used for
        /// notifications.</param>
        /// <param name="workingStatusController">The object to be used for
        /// managing application working status.</param>
        /// <param name="uriNavigator">The reference to the object to be used
        /// for navigating to URI's.</param>
        public LicensePageModel(
            IEnumerable<AgsServer> servers,
            IMessenger messenger,
            IWorkingStatusController workingStatusController,
            IUriNavigator uriNavigator,
            ILicenseManager licenseManager)
        {
            Debug.Assert(servers != null);
            Debug.Assert(servers.All(server => server != null));
            Debug.Assert(messenger != null);
            Debug.Assert(workingStatusController != null);
            Debug.Assert(licenseManager != null);

            _licenseManager = licenseManager;
            _licenseExpirationChecker = _licenseManager.LicenseExpirationChecker;

            var items = new ObservableCollection<LoginViewModelBase>();

            var licensingViewModel = new LicensingViewModel(
                messenger,
                workingStatusController,
                uriNavigator,
                licenseManager);
            this.RequiresExpirationWarning = licensingViewModel.RequiresExpirationWarning;
            licensingViewModel.PropertyChanged += delegate
            {
                this.RequiresExpirationWarning = licensingViewModel.RequiresExpirationWarning;
                _UpdateCompletedState();
            };

            items.Add(licensingViewModel);

            _servers = servers.ToList();
            foreach (var server in _servers)
            {
                items.Add(new ArcGisServerLoginViewModel(
                    server,
                    messenger,
                    workingStatusController));
                server.StateChanged += delegate
                {
                    _UpdateCompletedState();
                };
            }

            this.LoginViewModels = new ReadOnlyObservableCollection<LoginViewModelBase>(items);
            _UpdateCompletedState();
            _UpdateLoggedInState();

            foreach (var vm in this.LoginViewModels)
            {
                vm.LoginState.LoginViewHost = this;
                vm.PropertyChanged += delegate
                {
                    _UpdateLoggedInState();
                };
            }
        }
        #endregion

        #region public static properties
        /// <summary>
        /// Gets name of the <see cref="P:IsComplete"/> property.
        /// </summary>
        public string PropertyNameIsComplete
        {
            get
            {
                return PROPERTY_NAME_IS_COMPLETE;
            }
        }

        /// <summary>
        /// Gets name of the <see cref="P:RequiresExpirationWarning"/> property.
        /// </summary>
        public string PropertyNameRequiresExpirationWarning
        {
            get
            {
                return PROPERTY_NAME_REQUIRES_EXPIRATION_WARNING;
            }
        }

        /// <summary>
        /// Gets name of the <see cref="P:LoggedIn"/> property.
        /// </summary>
        public string PropertyNameLoggedIn
        {
            get
            {
                return PROPERTY_NAME_LOGGED_IN;
            }
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets read-only collection of the login view models.
        /// </summary>
        public ReadOnlyObservableCollection<LoginViewModelBase> LoginViewModels
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the license page is complete.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _isComplete;
            }
            private set
            {
                if (_isComplete != value)
                {
                    _isComplete = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_IS_COMPLETE);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if user successfully logs in all servers
        /// and activated the licence.
        /// </summary>
        public bool LoggedIn
        {
            get
            {
                return _loggedIn;
            }
            private set
            {
                if (_loggedIn != value)
                {
                    _loggedIn = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LOGGED_IN);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the license expiration warning should be
        /// displayed.
        /// </summary>
        public bool RequiresExpirationWarning
        {
            get
            {
                return _requiresExpirationWarning;
            }
            private set
            {
                if (_requiresExpirationWarning != value)
                {
                    _requiresExpirationWarning = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_REQUIRES_EXPIRATION_WARNING);
                }
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Updates value of the <see cref="P:IsComplete"/> property.
        /// </summary>
        private void _UpdateCompletedState()
        {
            this.IsComplete = _CheckCompletedState();
        }

        /// <summary>
        /// Updates value of the <see cref="P:IsLoggedIn"/> property.
        /// </summary>
        private void _UpdateLoggedInState()
        {
            var loggedIn = this.LoginViewModels
                .All(vm => vm.LicenseState == AgsServerState.Authorized);
            this.LoggedIn = loggedIn;
        }

        /// <summary>
        /// Checks if the license page is complete.
        /// </summary>
        /// <returns>True if and only if the page is complete.</returns>
        private bool _CheckCompletedState()
        {
            if (_servers.Any(server => server.State != AgsServerState.Authorized))
            {
                return false;
            }

            var license = _licenseManager.AppLicense;
            if (license == null)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region ILoginViewHost Members
        /// <summary>
        /// Gets or sets a value indicating if focus was set on a control for entering username.
        /// </summary>
        public bool IsUsernameControlFocused
        {
            get
            {
                return _isUsernameControlFocused;
            }
            set
            {
                if (_isUsernameControlFocused != value)
                {
                    _isUsernameControlFocused = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_IS_USERNAME_CONTROL_FOCUSED);
                }
            }
        }
        #endregion

        #region private constants
        /// <summary>
        /// Name of the IsComplete property.
        /// </summary>
        private const string PROPERTY_NAME_IS_COMPLETE = "IsComplete";

        /// <summary>
        /// Name of the LoggedIn property.
        /// </summary>
        private const string PROPERTY_NAME_LOGGED_IN = "LoggedIn";

        /// <summary>
        /// Name of the RequiresExpirationWarning property.
        /// </summary>
        private const string PROPERTY_NAME_REQUIRES_EXPIRATION_WARNING =
            "RequiresExpirationWarning";

        /// <summary>
        /// Name of the IsUsernameControlFocused property.
        /// </summary>
        private const string PROPERTY_NAME_IS_USERNAME_CONTROL_FOCUSED =
            "IsUsernameControlFocused";
        #endregion


        #region private fields
        /// <summary>
        /// Stores collection of ArcGIS server objects requiring separate
        /// credentials.
        /// </summary>
        private List<AgsServer> _servers;

        /// <summary>
        /// Stores reference to the license manager object.
        /// </summary>
        private ILicenseManager _licenseManager;

        /// <summary>
        /// Stores value of the IsComplete property.
        /// </summary>
        private bool _isComplete;

        /// <summary>
        /// Stores value of the LoggedIn property.
        /// </summary>
        private bool _loggedIn;

        /// <summary>
        /// Stores value of the RequiresExpirationWarning property.
        /// </summary>
        private bool _requiresExpirationWarning;

        /// <summary>
        /// Stores value of the IsUsernameControlFocused property.
        /// </summary>
        private bool _isUsernameControlFocused;

        /// <summary>
        /// The reference to the license expiration checker object.
        /// </summary>
        private ILicenseExpirationChecker _licenseExpirationChecker;
        #endregion
    }
}
