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
using System.Collections;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Page navigation item.
    /// </summary>
    internal class PageItem : INavigationItem
    {
        #region INavigationItem members

        public NavigationItemType Type 
        {
            get
            {
                return NavigationItemType.Page;
            }
        }

        public bool IsVisible
        {
            get { return true; }
        }

        public string Name
        {
            get
            {
                if (null == Page)
                    System.Diagnostics.Debug.Assert(false); // NOTE: not inited
                return Page.Name;
            }

            set
            {
                System.Diagnostics.Debug.Assert(false); // NOTE: not supported
                throw new InvalidOperationException();
            }
        }

        public string Caption
        {
            get
            {
                if (null == Page)
                    throw new InvalidOperationException();
                return Page.Title;
            }

            set
            {
                System.Diagnostics.Debug.Assert(false); // NOTE: not supported
                throw new InvalidOperationException();
            }
        }

        public INavigationItem Parent { get; set; }

        public IList<INavigationItem> Children 
        {
            get
            {
                List<INavigationItem> tempList = new List<INavigationItem>();
                return tempList.AsReadOnly();
            }
        }

        public void AddChild(INavigationItem item)
        {
            throw new NotSupportedException();
        }

        public void RemoveChild(INavigationItem item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAllChildren()
        {
            throw new NotSupportedException();
        }

        public bool FindItem(string path, out INavigationItem foundItem)
        {
            throw new NotSupportedException();
        }

        public bool FindItem(Type type, out INavigationItem foundItem)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region public members

        /// <summary>
        /// Type of page class that can be used for its instantiation.
        /// </summary>
        public Type PageType { get; set; }

        public Page Page { get; set; }

        #endregion
    }
}
