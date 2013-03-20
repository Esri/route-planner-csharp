using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.Data;
using Data = ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App
{
    internal class DataObjectNamesConstructor
    {
        #region Public Static methods
        /// <summary>
        /// Gets new name for object from collection.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="collection"></param>
        /// <returns>Returns new name for object from collection.</returns>
        public static string GetDuplicateName(string oldName, IEnumerable collection)
        {
            int k = 2;

            string newName = App.Current.GetString("ItemCopyShortName", oldName);

            if (_FindItem(newName, collection))
            {
                newName = App.Current.GetString("ItemCopyLongName", k, oldName);
                while (_FindItem(newName, collection))
                {
                    k++;
                    newName = App.Current.GetString("ItemCopyLongName", k, oldName);
                }
            }

            return newName;
        }

        /// <summary>
        /// Gets new name for DataObject which is not duplicated in collection.
        /// </summary>
        /// <param name="collection">Collection in which this object will be placed.</param>
        /// <param name="newItem">New object.</param>
        /// <param name="nameContainNew">If 'true' the new name will be like 'New Driver 2',otherwise
        /// it would be like 'Driver 2'.</param>
        /// <returns>New name.</returns>
        public static string GetNameForNewDataObject(IEnumerable collection,
            DataObject newItem, bool nameContainNew)
        {
            // Calculate new item's name.
            string name = newItem.TypeTitle;

            // If flag is 'true' then apply format from resources.
            if (nameContainNew)
                name = App.Current.GetString("NewItemNameFormat", name);

            return _GetUniqueNewName(collection, name, false, 
                delegate(object obj)
                {
                    return (obj as DataObject).Name; 
                });
        }

        /// <summary>
        /// Gets name for new project.
        /// </summary>
        /// <returns>Returns new name for project.</returns>
        public static string GetNewNameForProject()
        {
            // Calculate new item's name.
            string typeName = App.Current.GetString("Project");

            // Apply format from resources to DataObject.TypeTitle.
            string name = App.Current.GetString("NewItemNameFormat", typeName);

            return _GetUniqueNewName(App.Current.ProjectCatalog.Projects, name, false,
                delegate(object obj)
                { return (obj as ProjectConfiguration).Name; });
        }

        /// <summary>
        /// Gets new unique name for custom order property.
        /// </summary>
        /// <param name="collection">Collection of custom order properties with unique names.</param>
        /// <param name="nameContainsNewPrefix">If 'true' the new name will contain "New" prefix like
        /// "New Custom Order Property", otherwise it will be like "Custom Order Property."</param>
        /// <returns>Unique new name of Custom order property.</returns>
        public static string GetNewNameForCustomOrderProperty(IEnumerable collection, bool nameContainsNewPrefix)
        {
            Debug.Assert(collection != null);

            // Get from resources default name for new Custom order property.
            string defaultNewName = App.Current.GetString("CustomOrderProperty");

            // Add "New" prefix to the name if it is necessary.
            if (nameContainsNewPrefix)
                defaultNewName = App.Current.GetString("NewItemNameFormat", defaultNewName);

            Debug.Assert(!string.IsNullOrEmpty(defaultNewName));

            // Function which gets name of given object.
            Func<object, string> objectNameGetter =
                delegate (object someObject)
                {
                    return (someObject as CustomOrderProperty).Name;
                };

            return _GetUniqueNewName(collection, defaultNewName, false, objectNameGetter);
        }

        /// <summary>
        /// Get new empty name, which has no duplicate in collection.
        /// </summary>
        /// <param name="collection">Collection in which new object will be placed.</param>
        /// <returns>Whitespaces name.</returns>
        public static string GetNewWhiteSpacesName(IEnumerable collection)
        {
            return _GetUniqueNewName(collection, " ", true, 
                delegate(object obj) 
                { 
                    return (obj as DataObject).Name; 
                });
        }

        #endregion

        #region Private Static Method

        private delegate string GetObjectName(object obj);

        /// <summary>
        /// Calculate not duplicate new name.
        /// </summary>
        /// <param name="collection">Collection in which this name must be unique.</param>
        /// <param name="nameFormat">Name format.</param>
        /// <param name="returnWhiteSpaces">If yes, then to nameFormat will be added whitespaces,
        /// not digits.</param>
        /// <param name="checkName">Delegat, which returns name of the object from collection.</param>
        /// <returns>Unique name for object.</returns>
        private static string _GetUniqueNewName(IEnumerable collection, string nameFormat,
            bool returnWhiteSpaces, Func<object, string> checkName)
        {
            // Init variables.
            int duplicateIndex = 0;
            bool unique = false;
            string currentName = nameFormat;

            // While name not unique.
            while (!unique)
            {
                unique = true;

                // Check are there objects with equal name in collection.
                foreach (var dataObject in collection)
                    if (checkName(dataObject) == currentName)
                    {
                        // If there are the item with the same name - calculate next new name
                        // and start new collection check.
                        duplicateIndex++;
                        if (returnWhiteSpaces)
                            currentName += currentName + " ";
                        else
                            currentName = string.Format(NEW_NAME_FORMAT, nameFormat, 
                                duplicateIndex.ToString());
                        unique = false;
                        break;
                    }
            }

            // Return unique name.
            return currentName;
        }
        #endregion

        #region Protected static methods

        /// <summary>
        /// Checks is collection contains item. 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        protected static bool _FindItem(string item, IEnumerable collection)
        {
            foreach (Data.DataObject obj in collection)
            {
                if (item == obj.ToString())
                    return true;
            }
            return false;
        } 

        #endregion

        #region Private Static Constants

        private static string NEW_NAME_FORMAT = "{0} {1}";

        #endregion
    }
}
