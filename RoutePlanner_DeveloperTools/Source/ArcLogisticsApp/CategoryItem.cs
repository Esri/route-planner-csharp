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
using System.Diagnostics;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Category navigation item.
    /// </summary>
    class CategoryItem : INavigationItem
    {
        public CategoryItem(bool visible)
        {
            _visible = visible;
        }

        public NavigationItemType Type
        {
            get
            {
                return NavigationItemType.Category;
            }
        }

        public bool IsVisible
        {
            get { return _visible; }
        }

        public string Name { get; set; }
        public string Caption { get; set; }
        public Type CategoryType { get; set; }
        public PageCategoryItem PageCategory {get; set;}

        public INavigationItem Parent { get; set; }

        public IList<INavigationItem> Children 
        { 
            get
            {
                return _children.AsReadOnly();
            }
        }

        public void AddChild(INavigationItem item)
        {
            _children.Add(item);
            item.Parent = this;
        }

        public void RemoveChild(INavigationItem item)
        {
            if (!_children.Remove(item))
                throw new KeyNotFoundException();
        }

        public void RemoveAllChildren()
        {
            _children.Clear();
        }

        public bool FindItem(string path, out INavigationItem foundItem)
        {
            if (path == string.Empty)
            {
                foundItem = null;
                return false;
            }

            // split path to the first category and remaining path
            int delimPos = path.IndexOf(ITEM_DELIMITER);

            string categoryName = string.Empty;
            string remainPath = string.Empty;

            if (delimPos != -1)
            {
                categoryName = path.Substring(0, delimPos);
                remainPath = path.Substring(delimPos + 1);
            }
            else
                remainPath = path;

            bool isNeedPage = (categoryName == string.Empty);

            // try to find category or page by found values
            bool isFound = false;
            INavigationItem resItem = null;

            foreach (INavigationItem item in _children)
            {
                if (item.Type == NavigationItemType.Category)
                {
                    if (isNeedPage)
                        continue; // skip categories: we searching page

                    if (item.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        isFound = item.FindItem(remainPath, out resItem);
                        break;
                    }
                }
                else if (item.Type == NavigationItemType.Page)
                {
                    if (isNeedPage && item.Name.Equals(remainPath, StringComparison.OrdinalIgnoreCase))
                    {
                        isFound = true;
                        resItem = item;
                        break;
                    }
                }
                else
                    throw new NotSupportedException();
            }

            foundItem = resItem;
            return isFound;
        }

        public bool FindItem(Type type, out INavigationItem foundItem)
        {
            // try to find category or page by found values
            INavigationItem resItem = null;
            foreach (INavigationItem item in _children)
            {
                if (item.Type == NavigationItemType.Category)
                    item.FindItem(type, out resItem); // NOTE: ignore result
                else if (item.Type == NavigationItemType.Page)
                {
                    if (((PageItem)item).PageType == type)
                        resItem = item;
                }
                else
                    throw new NotSupportedException();

                if (null != resItem)
                    break;
            }

            foundItem = resItem;
            return (null != foundItem);
        }

        private const string ITEM_DELIMITER = @"\";

        private List<INavigationItem> _children = new List<INavigationItem>();
        private bool _visible = true;
    }
}
