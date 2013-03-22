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
using System.IO;
using System.Xml;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class load update settings from xmls when application loaded and save settings when closed
    /// </summary>
    internal class UpdateSettingsHelper
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateSettingsHelper()
        {
            // load _userEditable value
            try
            {
                string commonSettingsPath = Path.Combine(DataFolder.Path, COMMON_SETTINGS_FILE_PATH);
                _userEditable = _GetSettingValue(commonSettingsPath, USER_EDITABLE_TAG);
            }
            catch
            {
            }

            // load _checkForUpdate and _silentUpdate values
            try
            {
                string userSettingsPath = Path.Combine(DataFolder.Path, USER_SETTINGS_FILE_PATH);
                _checkForUpdate = _GetSettingValue(userSettingsPath, CHECK_FOR_UPDATE_TAG);
                _silentUpdate = _GetSettingValue(userSettingsPath, SILENT_UPDATE_TAG);
            }
            catch
            {
                // we should disable editind check boxes in GeneralPreferencesPage and set _userEditable to "false"
                _userEditable = false;
            }

            // save values
            _storedCheckForUpdate = _checkForUpdate;
            _storedSilentUpdate = _silentUpdate;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Store user's settings
        /// </summary>
        public void StoreSettings()
        {
            // save _checkForUpdate value if necessary
            if (_checkForUpdate != _storedCheckForUpdate)
                _SaveUserSettingValue(CHECK_FOR_UPDATE_TAG, _checkForUpdate);

            // save _silentUpdate value if necessary
            if (_silentUpdate != _storedSilentUpdate)
                _SaveUserSettingValue(SILENT_UPDATE_TAG, _silentUpdate);

            _storedCheckForUpdate = _checkForUpdate;
            _storedSilentUpdate = _silentUpdate;
        }

        #endregion

        #region Public Properties
        /// <summary>
        ///  Gets/sets user editable value
        /// </summary>
        public bool UserEditable
        {
            get { return _userEditable; }
            set { _userEditable = value; }
        }

        /// <summary>
        /// Gets/sets checks for update value 
        /// </summary>
        public bool CheckForUpdate
        {
            get { return _checkForUpdate; }
            set { _checkForUpdate = value; }
        }

        /// <summary>
        /// Gets/sets silence update value
        /// </summary>
        public bool SilenceUpdate
        {
            get { return _silentUpdate; }
            set { _silentUpdate = value; }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Method gets necessary update setting from stated file
        /// </summary>
        private bool _GetSettingValue(string filePath, string settingTagName)
        {
            bool result = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                //get all the elements for the specified tag
                XmlNodeList nodeList = doc.GetElementsByTagName(settingTagName);
                if (0 < nodeList.Count)
                {
                    // get value from specified tag
                    string value = nodeList[0].ChildNodes[0].InnerText;

                    // define value of result flag depending on tag value 
                    result = (string.Compare(value, YES_STRING_VALUE, true) == 0) ? true : false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Method saves user setting value to al_update_user.xml
        /// </summary>
        /// <param name="settingValue"></param>
        /// <param name="settingTag"></param>
        private void _SaveUserSettingValue(string settingTag, bool settingValue)
        {
            try
            {
                string filePath = Path.Combine(DataFolder.Path, USER_SETTINGS_FILE_PATH);
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                //get all the elements for the specified tag
                XmlNodeList nodeList = doc.GetElementsByTagName(settingTag);

                if (0 < nodeList.Count)
                {
                    // set new value to specified tag
                    string newValue = (settingValue) ? YES_STRING_VALUE : NO_STRING_VALUE;
                    nodeList[0].ChildNodes[0].InnerText = newValue;
                }

                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        #endregion

        #region Private Fields

        private const string USER_EDITABLE_TAG = "UserEditable";
        private const string CHECK_FOR_UPDATE_TAG = "CheckForUpdate";
        private const string SILENT_UPDATE_TAG = "SilentUpdate";
        private const string COMMON_SETTINGS_FILE_PATH = "al_update.xml";
        private const string USER_SETTINGS_FILE_PATH = "al_update_user.xml";
        private const string YES_STRING_VALUE = "yes";
        private const string NO_STRING_VALUE = "no";

        // settings properties values
        private bool _userEditable = false;
        private bool _checkForUpdate = false;
        private bool _silentUpdate = false;

        // stored settings values (we should save new values)
        private bool _storedCheckForUpdate = false;
        private bool _storedSilentUpdate = false;

        #endregion
    }
}
