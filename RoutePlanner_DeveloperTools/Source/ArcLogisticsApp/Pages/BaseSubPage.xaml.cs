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
using System.Collections;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// BaseSubPage class populated basic routine for page with subpages.
    /// </summary>
    internal abstract partial class BaseSubPage : PageBase
    {
        protected struct SubPage
        {
            public SubPage(object subPageKey, Type subPageType)
            {
                _subPageKey = subPageKey;
                _subPageType = subPageType;
            }

            public object SubPageKey
            {
                get { return _subPageKey; }
            }

            public Type SubPageType
            {
                get { return _subPageType; }
            }

            object _subPageKey;
            Type _subPageType;
        };

        /// <summary>
        /// Current sub page key.
        /// </summary>
        public object CurrentSubPageKey
        {
            get
            {
                return _currentSubPage;
            }
            set
            {
                _currentSubPage = value;
                _NavigateToSubPage(_currentSubPage);
            }
        }

        #region Constructors

        public BaseSubPage()
        {
            InitializeComponent();
            _currentSubPage = _StartupSubPage;

            this.Loaded += new RoutedEventHandler(BaseSubPage_Loaded);
            SubPageFrame.CommandBindings.Clear();

            if (_IsNeedCreateSubPages())
                _CreateAllSubPages();
        }

        void BaseSubPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInited)
            {
                _NavigateToSubPage(_currentSubPage);

                _isInited = true;
            }
        }

        #endregion

        #region Proptected Methods

        /// <summary>
        /// Returns startup sub page key. Must be overriden.
        /// </summary>
        protected abstract object _StartupSubPage
        {
            get;
        }

        /// <summary>
        /// Returns sub pages list. Must be overriden.
        /// </summary>
        /// <param name="subPageKey"></param>
        /// <returns></returns>
        protected abstract SubPage[] _GetSubPageList();

        protected virtual bool _IsNeedCreateSubPages()
        {
            return false;
        }

        protected System.Windows.Controls.Grid _GetSubPage(object subPageKey)
        {
            System.Diagnostics.Debug.Assert(_subPages.Contains(subPageKey));
            return (System.Windows.Controls.Grid)_subPages[subPageKey];
        }

        #endregion

        #region private methods

        /// <summary>
        /// Navigates to sub page by key.
        /// </summary>
        /// <param name="subPageKey"></param>
        private void _NavigateToSubPage(object subPageKey)
        {
            System.Windows.Controls.Grid page =
                _subPages.Contains(subPageKey)? (System.Windows.Controls.Grid)_subPages[subPageKey] : _CreateSubPage(subPageKey);

            if (SubPageFrame.Content == null || !SubPageFrame.Content.Equals(page))
                SubPageFrame.Content = page;
        }

        private Type _GetSubPageType(object subPageKey)
        {
            foreach (SubPage subPage in _GetSubPageList())
            {
                if (subPage.SubPageKey.Equals(subPageKey))
                    return subPage.SubPageType; // NOTE: founded
            }

            Debug.Assert(false);
            return null;
        }

        private System.Windows.Controls.Grid _CreateSubPage(object subPageKey)
        {
            System.Diagnostics.Debug.Assert(!_subPages.Contains(subPageKey));

            System.Windows.Controls.Grid page = (System.Windows.Controls.Grid)Activator.CreateInstance(_GetSubPageType(subPageKey));
            _subPages.Add(subPageKey, page);

            return page;
        }

        private void _CreateAllSubPages()
        {
            foreach (SubPage subPage in _GetSubPageList())
                _CreateSubPage(subPage.SubPageKey);
        }

        #endregion

        #region Private variables

        private Hashtable _subPages = new Hashtable();
        private object _currentSubPage;
        private bool _isInited = false;

        #endregion
    }
}
