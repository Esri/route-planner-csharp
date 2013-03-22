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
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.PageCategories
{
    internal class SetupCategory : PageCategoryItem
    {
        #region Constructors

        public SetupCategory()
        {
            _CheckCategoryAllowed();

            App.Current.ProjectLoaded += new EventHandler(OnProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(OnProjectClosed);
        }

        #endregion

        #region Protected methods

        protected void _CheckCategoryAllowed()
        {
            IsEnabled = (App.Current.Project != null);
        }

        #endregion

        #region Event Handlers

        private void OnProjectClosed(object sender, EventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void OnProjectLoaded(object sender, EventArgs e)
        {
            _CheckCategoryAllowed();
        }

        #endregion
    }
}
