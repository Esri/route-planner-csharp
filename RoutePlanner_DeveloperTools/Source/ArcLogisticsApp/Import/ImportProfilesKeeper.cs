using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class contains methods for management of import profiles.
    /// </summary>
    internal class ImportProfilesKeeper
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create and init object.
        /// </summary>
        /// <param name="fileName">Import storage file name.</param>
        public ImportProfilesKeeper(string fileName)
        {
            _file = new ImportFile(fileName); // initialize state
        }

        #endregion // Constructor

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Application import profiles read-only collection.
        /// </summary>
        public ICollection<ImportProfile> Profiles
        {
            get { return _file.Profiles.AsReadOnly(); }
        }

        /// <summary>
        /// Application import auto field name.
        /// </summary>
        /// <remarks>Call after Load()</remarks>
        public StringDictionary FieldAliases
        {
            get { return _file.FieldAliases; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add new or update presented import profile in application collection.
        /// </summary>
        /// <param name="profile">Profile object.</param>
        public void AddOrUpdateProfile(ImportProfile profile)
        {
            AddOrUpdateProfile(profile, null);
        }

        /// <summary>
        /// Add new or update presented import profile in application collection.
        /// </summary>
        /// <param name="profile">Profile object.</param>
        /// <param name="previouslyName">Previously profile name (for edited profile).</param>
        public void AddOrUpdateProfile(ImportProfile profile, string previouslyName)
        {
            List<ImportProfile> profiles = _file.Profiles;

            bool isEditProcess = (null != previouslyName);
            string nameForSeek = isEditProcess ? previouslyName : profile.Name;

            // remove older profile with equals name
            ImportProfile profileToRemove = null;
            for (int i = 0; i < profiles.Count; ++i)
            {
                if (profiles[i].Name.Equals(nameForSeek, StringComparison.OrdinalIgnoreCase))
                {
                    profileToRemove = profiles[i];
                    break; // NOTE: founded
                }
            }

            if (null != profileToRemove)
                profiles.Remove(profileToRemove);

            // update default state
            if (profile.IsDefault)
                ResetDefaultProfileForType(profile.Type);
            else
            {   // select first profile to type as default
                if (isEditProcess && !_DoesDefaultProfileSetForType(profile.Type))
                    profile.IsDefault = true;
            }

            // add created profile
            _file.Profiles.Add(profile);
            StoreState();
        }

        /// <summary>
        /// Removes profile from application collection.
        /// </summary>
        /// <param name="profile">Profile object to removing.</param>
        public void Remove(ImportProfile profile)
        {
            _file.Profiles.Remove(profile);
        }

        /// <summary>
        /// Gets default profile for type.
        /// </summary>
        /// <param name="type">Type identificator.</param>
        /// <returns>Default import profile.</returns>
        /// <remarks>Can be return NULL if default import profile not set for this type.</remarks>
        public ImportProfile GetDefaultProfile(ImportType type)
        {
            foreach (ImportProfile profile in Profiles)
            {
                if (type == profile.Type)
                {
                    if (profile.IsDefault)
                        return profile;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets one time profile for type.
        /// </summary>
        /// <param name="type">Type identificator.</param>
        /// <returns>Default import profile.</returns>
        /// <remarks>Can be return NULL if default import profile not set for this type.</remarks>
        public ImportProfile GetOneTimeProfile(ImportType type)
        {
            foreach (ImportProfile profile in Profiles)
            {
                if (type == profile.Type)
                {
                    if (profile.IsOnTime)
                        return profile;
                }
            }

            return null;
        }

        /// <summary>
        /// Resets default profile for selected type.
        /// </summary>
        /// <param name="type">Type identificator.</param>
        /// <returns>TRUE if make real changes.</returns>
        public bool ResetDefaultProfileForType(ImportType type)
        {
            bool isChanged = false;
            List<ImportProfile> profiles = _file.Profiles;
            for (int i = 0; i < profiles.Count; ++i)
            {
                if (type == profiles[i].Type)
                {
                    isChanged |= profiles[i].IsDefault;
                    profiles[i].IsDefault = false;
                }
            }

            return isChanged;
        }

        /// <summary>
        /// Updates project defaults import profiles.
        /// </summary>
        /// <returns>TRUE if do real update.</returns>
        public bool UpdateDefaults()
        {
            bool doesUpdate = false;
            Project project = App.Current.Project;
            if (null != project)
            {
                string defaults = project.ProjectProperties[PROJECT_COFIGURATION_PROPERTY_NAME];
                if (!string.IsNullOrEmpty(defaults))
                {
                    _file.UpdateDefaults(defaults);
                    doesUpdate = true;
                }
            }

            return doesUpdate;
        }

        /// <summary>
        /// Stores changes.
        /// </summary>
        public void StoreState()
        {
            _file.Save();

            if (null != App.Current)
            {
                Project project = App.Current.Project;
                if (null != project)
                {
                    project.ProjectProperties.UpdateProperty(PROJECT_COFIGURATION_PROPERTY_NAME, _file.CreateDefaultsDescription());
                    project.Save();
                }
            }
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks present default profile for type.
        /// </summary>
        /// <param name="type">Type identificator.</param>
        /// <returns>TRUE if default profile presented.</returns>
        private bool _DoesDefaultProfileSetForType(ImportType type)
        {
            bool isDefaultPresent = false;
            List<ImportProfile> profiles = _file.Profiles;
            for (int i = 0; i < profiles.Count; ++i)
            {
                if ((type == profiles[i].Type) && (profiles[i].IsDefault))
                {
                    isDefaultPresent = true;
                    break; // NOTE: stop process
                }
            }

            return isDefaultPresent;
        }

        #endregion // Private methods

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Project configuration property name.
        /// </summary>
        private const string PROJECT_COFIGURATION_PROPERTY_NAME = "DefaultImportProfiles";

        #endregion // Private consts

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ImportFile _file = null;

        #endregion // Private members
    }
}
