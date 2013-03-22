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

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interface for event, which syngnalyze about "Next" button in page pressed.
    /// </summary>
    internal interface ISupportNext
    {
        event EventHandler NextRequired;
    }

    /// <summary>
    /// Interface for event, which syngnalyze about "Back" button in page pressed.
    /// </summary>
    internal interface ISupportBack
    {
        event EventHandler BackRequired;
    }

    /// <summary>
    /// Interface for event, which syngnalyze about "Cancel" button in page pressed.
    /// </summary>
    internal interface ISupportCancel
    {
        event EventHandler CancelRequired;
    }

    /// <summary>
    /// Interface for event, which syngnalyze about "Finish" button in page pressed.
    /// </summary>
    internal interface ISupportFinish
    {
        event EventHandler FinishRequired;
    }

    /// <summary>
    /// Class storage of wizard context.
    /// </summary>
    internal class WizardDataContext : Dictionary<string, object>
    {
    }

    /// <summary>
    /// Base class for wizards.
    /// </summary>
    internal class WizardBase
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pages">Pages types.</param>
        /// <param name="dataContext">Wizard data context.</param>
        public WizardBase(Type[] pages, WizardDataContext dataContext)
        {
            _Init(pages, dataContext);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Start executing wizard.
        /// </summary>
        public virtual void Start()
        {
            Debug.Assert(_isInited);

            // Clear status bar
            MainWindow mainWindow = App.Current.MainWindow;
            mainWindow.StatusBar.SetStatus(mainWindow.CurrentPage, "");
        }

        #endregion

        #region Protected members

        /// <summary>
        /// Wizard data context that stores all the data necessary to pass from page to page.
        /// </summary>
        protected WizardDataContext DataContext
        {
            get
            {
                return _dataContext;
            }
        }

        /// <summary>
        /// Wizard pages.
        /// </summary>
        protected IList<WizardPageBase> Pages
        {
            get
            {
                return _pages;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Special initializing of wizard pages.
        /// </summary>
        protected virtual void PostInit()
        {

        }

        /// <summary>
        /// "Next" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnNextRequired(object sender, EventArgs e)
        {
            // Go forward to next page.
            int currentPageIndex = _GetPageIndex(sender as WizardPageBase);
            _NavigateToPage(++currentPageIndex);
        }

        /// <summary>
        /// "Back" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected virtual void _OnBackRequired(object sender, EventArgs e)
        {
            // Go back to previously page.
            int currentPageIndex = _GetPageIndex(sender as WizardPageBase);
            _NavigateToPage(--currentPageIndex);
        }

        /// <summary>
        /// "Cancel" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected virtual void _OnCancelRequired(object sender, EventArgs e)
        {
            // Cancel wizard executing.
        }

        /// <summary>
        /// "Finish" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected virtual void _OnFinishRequired(object sender, EventArgs e)
        {
            // Finish wizard executing.
        }

        /// <summary>
        /// Navigate to wizard page.
        /// </summary>
        /// <param name="pageIndex"></param>
        protected void _NavigateToPage(int pageIndex)
        {
            Debug.Assert((0 <= pageIndex) && (pageIndex < _pages.Count));

            App.Current.MainWindow.PageFrame.Navigate(_pages[pageIndex]);
        }

        /// <summary>
        /// Gets page index in pages array by page.
        /// </summary>
        /// <param name="page">Page to check.</param>
        /// <returns>Page index in pages array.</returns>
        protected int _GetPageIndex(WizardPageBase page)
        {
            Debug.Assert(null != page);

            int result = -1;
            for (int index = 0; index < _pages.Count; ++index)
            {
                if (_pages[index] == page)
                {
                    result = index;
                    break; // result founded
                }
            }

            Debug.Assert(-1 != result); // not supported
            return result;
        }

        /// <summary>
        /// Gets page index in pages array by type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>Page index in pages array.</returns>
        protected int _GetPageIndex(Type type)
        {
            int result = -1;
            for (int index = 0; index < _pages.Count; ++index)
            {
                if (_pages[index].GetType() == type)
                {
                    result = index;
                    break; // result founded
                }
            }

            Debug.Assert(-1 != result); // not supported
            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Inits wizard state.
        /// </summary>
        private void _Init(Type[] _pageInfos, WizardDataContext dataContext)
        {
            Debug.Assert(!_isInited);

            _dataContext = dataContext;
            foreach (Type pageType in _pageInfos)
            {
                // Create page.
                var page = (WizardPageBase)Activator.CreateInstance(pageType);
                page.Initialize(_dataContext);

                // Attach to supported page event.
                if (page is ISupportNext)
                    ((ISupportNext)page).NextRequired +=
                        new EventHandler(OnNextRequired);

                if (page is ISupportBack)
                    ((ISupportBack)page).BackRequired +=
                        new EventHandler(_OnBackRequired);

                if (page is ISupportCancel)
                    ((ISupportCancel)page).CancelRequired +=
                        new EventHandler(_OnCancelRequired);

                if (page is ISupportFinish)
                    ((ISupportFinish)page).FinishRequired +=
                        new EventHandler(_OnFinishRequired);

                _pages.Add(page);
            }

            PostInit();

            _isInited = true;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Wizard data context that stores all the data necessary to pass from page to page.
        /// </summary>
        private WizardDataContext _dataContext;

        /// <summary>
        /// Wizard pages.
        /// </summary>
        private List<WizardPageBase> _pages = new List<WizardPageBase>();

        /// <summary>
        /// Is wizard inited.
        /// </summary>
        private bool _isInited;

        #endregion
    }
}

