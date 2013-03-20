using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Properties;

namespace ESRI.ArcLogistics.App
{
    class NavigationTree
    {
        /// <summary>
        /// Full page name format (Category\Name).
        /// </summary>
        internal const string FULL_PAGE_NAME_FORMAT = @"{0}\{1}";

        #region Constructors

        public NavigationTree()
        {
            try
            {
                // load application structure during creation
                _LoadApplicationStructure();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new Exception((string)App.Current.FindResource("NavigationTreeLoadingError"), ex);
            }
        }

        #endregion

        #region Public members

        /// <summary>
        /// Returns navigation tree root item. This item contains main categories, that
        /// can be accessed via Children property.
        /// </summary>
        public INavigationItem NaviationTreeRoot
        {
            get
            {
                return _navigationTree;
            }
        }

        /// <summary>
        /// Finds item by path.
        /// </summary>
        /// <param name="path">See INavigationItem.FindItem method description.</param>
        /// <param name="foundItem">See INavigationItem.FindItem method description.</param>
        /// <returns></returns>
        public bool FindItem(string path, out INavigationItem foundItem)
        {
            return _navigationTree.FindItem(path, out foundItem);
        }

        /// <summary>
        /// Finds item by type.
        /// </summary>
        /// <param name="type">See INavigationItem.FindItem method description.</param>
        /// <param name="foundItem">See INavigationItem.FindItem method description.</param>
        /// <returns></returns>
        public bool FindItem(Type type, out INavigationItem foundItem)
        {
            return _navigationTree.FindItem(type, out foundItem);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads application structure from resource xml file.
        /// </summary>
        private void _LoadApplicationStructure()
        {
            // load navigation tree from resources
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream xmlStream = assembly.GetManifestResourceStream("ESRI.ArcLogistics.App.NavigationTree.xml"))
            {
                XmlDocument navTreeDoc = new XmlDocument();
                navTreeDoc.Load(xmlStream);

                // create top level navigation category
                _navigationTree = new CategoryItem(false); // NOTE: root not have GUI
                _navigationTree.Name = TREE_ITEM_NAME;

                // create navigation tree in memory
                XmlElement rootElement = navTreeDoc.DocumentElement;
                foreach(XmlNode node in rootElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue; // skip comments and other non element nodes

                    if (node.Name.Equals(XML_CATEGORY_ELEMENT_NAME, StringComparison.OrdinalIgnoreCase))
                        _AddCategoryItem(_navigationTree as CategoryItem, node as XmlElement);
                    else if (node.Name.Equals(XML_PAGE_ELEMENT_NAME, StringComparison.OrdinalIgnoreCase))
                        _AddPageItem(_navigationTree as CategoryItem, node as XmlElement);
                    else
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Reads category item from Xml element and adds it as a child to the parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="categoryNode"></param>
        private void _AddCategoryItem(CategoryItem parent, XmlElement categoryNode)
        {
            bool visible = true;
            if (null != categoryNode.Attributes[XML_VISIBLE_ATTRIBUTE_NAME])
                visible = bool.Parse(categoryNode.Attributes[XML_VISIBLE_ATTRIBUTE_NAME].Value);

            CategoryItem newCategory = new CategoryItem(visible);

            newCategory.Name = categoryNode.GetAttribute(XML_NAME_ATTRIBUTE_NAME, "");
            string captionID = categoryNode.GetAttribute(XML_CAPTIONID_ATTRIBUTE_NAME, "");
            newCategory.Caption = (string)App.Current.FindResource(captionID);

            string typeName = categoryNode.GetAttribute(XML_TYPE_ATTRIBUTE_NAME, "");
            newCategory.CategoryType = Type.GetType(typeName);

            // add new category to the parent
            newCategory.Parent = parent;
            parent.AddChild(newCategory);

            // process all other category childs
            foreach (XmlNode node in categoryNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(XML_CATEGORY_ELEMENT_NAME, StringComparison.OrdinalIgnoreCase))
                    _AddCategoryItem(newCategory as CategoryItem, node as XmlElement);
                else if (node.Name.Equals(XML_PAGE_ELEMENT_NAME, StringComparison.OrdinalIgnoreCase))
                    _AddPageItem(newCategory as CategoryItem, node as XmlElement);
                else
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Reads page item from Xml element and adds it as a child to the parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="categoryNode"></param>
        private void _AddPageItem(CategoryItem parent, XmlElement categoryNode)
        {
            PageItem newPage = new PageItem();

            string typeName = categoryNode.GetAttribute(XML_TYPE_ATTRIBUTE_NAME, "");
            newPage.PageType = Type.GetType(typeName);

            // page type should exist
            Debug.Assert(newPage.PageType != null);

            // add new page to the parent
            newPage.Parent = parent;
            parent.AddChild(newPage);
        }

        #endregion

        #region Private members

        private const string TREE_ITEM_NAME = "Root";

        // XML elements and attribute names.
        private const string XML_CATEGORY_ELEMENT_NAME = "ALCategory";
        private const string XML_PAGE_ELEMENT_NAME = "ALPage";
        private const string XML_NAME_ATTRIBUTE_NAME = "name";
        private const string XML_CAPTIONID_ATTRIBUTE_NAME = "captionID";
        private const string XML_TYPE_ATTRIBUTE_NAME = "type";
        private const string XML_VISIBLE_ATTRIBUTE_NAME = "visible";

        /// <summary>
        /// Navigation tree.
        /// </summary>
        private INavigationItem _navigationTree = null;

        #endregion
    }
}
