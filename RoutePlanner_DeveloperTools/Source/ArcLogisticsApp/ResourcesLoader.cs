using System.Collections;
using System.Globalization;
using System.Threading;
using System.Windows;
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
using System.Resources;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class which load resources from resx files to resource dictionary.
    /// </summary>
    internal static class ResourcesLoader
    {

        #region Public member

        /// <summary>
        /// Add resources from resource manager to resource dictionary.
        /// </summary>
        /// <param name="manager">ResourceManager.</param>
        /// <param name="resourceDictionary">ResourceDictionary.</param>
        static public void LoadResourcesToDictionary(ResourceManager manager,
            ResourceDictionary resourceDictionary)
        {
            // Get dictionary for main assembly.
            ResourceDictionary defaultDictionary = LoadResourceDictionaryFromManager
                (manager, DEFAULT_CULTURE);

            // Add this dictionary to application resources.
            resourceDictionary.MergedDictionaries.Add(defaultDictionary);

            // If the current culture differs from default - add 
            // resources from sattelite assembly.
            // So if resource was localized - localized version will be used, otherwise 
            // resource from main assembly will be returned.
            if (Thread.CurrentThread.CurrentUICulture.CompareInfo !=
                DEFAULT_CULTURE.CompareInfo)
            {
                ResourceDictionary localizedDictionary =
                    LoadResourceDictionaryFromManager(manager, Thread.CurrentThread.CurrentUICulture);
                resourceDictionary.MergedDictionaries.Add(localizedDictionary);
            }
        } 

        /// <summary>
        /// Return resource dictionary with resources for specific culture.
        /// </summary>
        /// <param name="manager">ResourceManager.</param>
        /// <param name="cultureInfo">CultureInfo.</param>
        /// <returns>ResourceDictionary.</returns>
        static private ResourceDictionary LoadResourceDictionaryFromManager(ResourceManager manager,
            CultureInfo cultureInfo)
        {
            ResourceDictionary dictionary = new ResourceDictionary();

            // Get all resources from sattelite assembly.
            var resourceSet = manager.GetResourceSet(cultureInfo, true, true);

            // Add each resource to resource dictionary.
            foreach (DictionaryEntry item in resourceSet)
                dictionary.Add(item.Key, item.Value);

            return dictionary;
        }

        #endregion

        #region Private static field

        /// <summary>
        /// Application default culture.
        /// </summary>
        private static CultureInfo DEFAULT_CULTURE = CultureInfo.GetCultureInfo("en-US");
        
        #endregion
    }
}
