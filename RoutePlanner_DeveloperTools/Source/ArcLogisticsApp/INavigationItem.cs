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

namespace ESRI.ArcLogistics.App
{
    internal enum NavigationItemType
    {
        Category,
        Page
    }

    /// <summary>
    /// Interface for navigation item, which can be either UI category or page.
    /// </summary>
    internal interface INavigationItem
    {
        /// <summary>
        /// Type of navigation item.
        /// </summary>
        NavigationItemType Type { get; }

        /// <summary>
        /// Name of navigation item. Must be unque inside the same category.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Localized caption that is shown in the UI.
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// Parent for this navigation item.
        /// </summary>
        INavigationItem Parent { get; set; }
        
        /// <summary>
        /// Navigation child items read-only collection.
        /// </summary>
        IList<INavigationItem> Children { get; }
        
        /// <summary>
        /// Adds child navigation item to the children collection
        /// </summary>
        /// <param name="item"></param>
        void AddChild(INavigationItem item);

        /// <summary>
        /// Removes navigation child item from children collection.
        /// </summary>
        /// <param name="item"></param>
        void RemoveChild(INavigationItem item);

        /// <summary>
        /// Removes all children.
        /// </summary>
        void RemoveAllChildren();

        /// <summary>
        /// Finds item by path.
        /// </summary>
        /// <param name="path">String in format CategoryName\PageName.</param>
        /// <param name="foundItem">Output parameter gets reference on the found item.</param>
        /// <returns>Returns true if navigation item is found or false otherwise.</returns>
        bool FindItem(string path, out INavigationItem foundItem);

        /// <summary>
        /// Finds item by type.
        /// </summary>
        /// <param name="type">Category or page type.</param>
        /// <param name="foundItem">Output parameter gets reference on the found item.</param>
        /// <returns>Returns true if navigation item is found or false otherwise.</returns>
        bool FindItem(Type type, out INavigationItem foundItem);

        /// <summary>
        /// Visible in GUI flag.
        /// </summary>
        /// <remarks>Used only for NavigationItemType.Category</remarks>
        bool IsVisible { get; }
    }
}
