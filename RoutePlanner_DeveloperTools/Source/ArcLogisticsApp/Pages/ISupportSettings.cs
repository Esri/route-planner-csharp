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

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Implement this interface in your page to support settings for their storing\restoring.
    /// </summary>
    public interface ISupportSettings
    {
        /// <summary>
        /// Saves user settigns to a string. This methid is called by application directly after the page is initialized.
        /// </summary>
        string SaveUserSettings();

        /// <summary>
        /// Loads user settings from a string. This method is called by application when it is closing.
        /// </summary>
        void LoadUserSettings(string settingsString);
    }
}
