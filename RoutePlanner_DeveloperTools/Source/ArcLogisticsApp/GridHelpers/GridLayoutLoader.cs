using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Class helper for loading grid layout.
    /// </summary>
    internal class GridLayoutLoader
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GridLayoutLoader()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repositoryName">Layout settings name.</param>
        /// <param name="actualProperties">Data grid item properties.</param>
        /// <param name="detailsProperties">Data grid details item properties.</param>
        public GridLayoutLoader(string repositoryName, ObservableCollection<DataGridItemPropertyBase> actualProperties,
            ObservableCollection<DataGridItemPropertyBase> detailsProperties)
        {
            _LoadGridSettingsFromRepository(repositoryName, actualProperties, false);
            _LoadGridSettingsFromRepository(repositoryName, detailsProperties, true);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repositoryName">Layout settings name.</param>
        /// <param name="actualProperties">Data grid item properties.</param>
        public GridLayoutLoader(string repositoryName, ObservableCollection<DataGridItemPropertyBase> actualProperties)
        {
            _LoadGridSettingsFromRepository(repositoryName, actualProperties, false);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Load layout from settings.
        /// </summary>
        /// <param name="dataGridControl">Data grid control.</param>
        public void LoadLayout(DataGridControl dataGridControl)
        {
            if (_settingRepository != null)
            {
                dataGridControl.LoadUserSettings(_settingRepository, UserSettings.All);
            }

            dataGridControl.UpdateLayout();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Load grid layout settings.
        /// </summary>
        /// <param name="repositoryName">Layout settings name.</param>
        /// <param name="actualProperties">Data grid item properties.</param>
        /// <param name="isDetails">Is details items.</param>
        private void _LoadGridSettingsFromRepository(string repositoryName,
            ICollection<DataGridItemPropertyBase> actualProperties, bool isDetails)
        {
            SettingsRepository repository = (SettingsRepository)Properties.Settings.Default[repositoryName];
            if (repository != null)
            {
                foreach (KeyValuePair<string, SettingsBase> setting in repository.Settings)
                {
                    ColumnSettings columnSetting = setting.Value as ColumnSettings;
                    if (columnSetting != null)
                    {
                        _LoadColumnSetting(columnSetting, actualProperties, isDetails);
                    }
                    else
                    {
                        _LoadSetting(setting.Value, isDetails);
                    }
                }
            }
        }

        /// <summary>
        /// Load column setting.
        /// </summary>
        /// <param name="columnSetting">Column setting.</param>
        /// <param name="actualProperties">Data grid item properties.</param>
        /// <param name="isDetails">Is details items.</param>
        private void _LoadColumnSetting(ColumnSettings columnSetting, 
            ICollection<DataGridItemPropertyBase> actualProperties, bool isDetails)
        {
            string key = columnSetting.SettingsKey;
            foreach (DataGridItemProperty property in actualProperties)
            {
                string formatString = COLUMNS_SETTINGS_NAMES_FORMAT_STRING;
                if (isDetails)
                {
                    formatString = ROUTE_INFO_SETTINGS_NAME + formatString;
                }

                if (String.Format(formatString, property.Name).Equals(key))
                {
                    _settingRepository.Settings.Add(key, columnSetting);
                }
            }
        }

        /// <summary>
        /// Load setting.
        /// </summary>
        /// <param name="settingValue">Setting value.</param>
        /// <param name="isDetails">Is details items.</param>
        private void _LoadSetting(SettingsBase settingValue, bool isDetails)
        {
            if (isDetails)
            {
                DetailConfigurationSettings detailControlSetting = settingValue as DetailConfigurationSettings;
                if (detailControlSetting != null)
                {
                    _settingRepository.Settings.Add(detailControlSetting.SettingsKey, detailControlSetting);
                }
            }
            else
            {
                DataGridControlSettings controlSetting = settingValue as DataGridControlSettings;
                if (controlSetting != null)
                {
                    // Get list of sort fields which are not exists in current Settings Repository.
                    List<string> redundantFields = _GetNonexistentSortFields(controlSetting.SortDescriptions);

                    // If some redundant fields found, need to remove it.
                    if (redundantFields.Count > 0)
                    {
                        controlSetting.SortDescriptions = _GetFieldsForSort(
                            controlSetting.SortDescriptions, redundantFields);
                    }

                    _settingRepository.Settings.Add(controlSetting.SettingsKey, controlSetting);
                }
            }
        }


        /// <summary>
        /// Method searches for Sort Fields which is not currently exists in Settings Repository.
        /// </summary>
        /// <param name="fieldsToSort">Collection of fields, marked to sort.</param>
        /// <returns>Collection of nonexistent fields, marked to sort.</returns>
        private List<string> _GetNonexistentSortFields(SortDescription[] fieldsToSort)
        {
            Debug.Assert(fieldsToSort != null);

            List<string> namesToRemove = new List<string>();

            foreach (SortDescription descr in fieldsToSort)
                if (!_settingRepository.Settings.ContainsKey(
                    String.Format(COLUMNS_SETTINGS_NAMES_FORMAT_STRING, descr.PropertyName)))
                    namesToRemove.Add(descr.PropertyName);

            return namesToRemove;
        }

        /// <summary>
        /// Method creates correct collection of Fields for sort.
        /// </summary>
        /// <param name="currentFieldsForSort">Collection of Fields marked for sort.</param>
        /// <param name="incorrectFieldsForSort">Collection of incorrect Fields for sort.</param>
        /// <returns>Collection of sortable fields.</returns>
        private SortDescription[] _GetFieldsForSort(SortDescription[] currentFieldsForSort,
            List<string> incorrectFieldsForSort)
        {
            // Calculate how many sort fields will stay in current Repository Settings.
            Int32 newPropertiesCount = currentFieldsForSort.Length - incorrectFieldsForSort.Count;
            SortDescription[] sortableFields = new SortDescription[newPropertiesCount];

            Int32 fieldsCounter = 0;

            // Copy all available fields from current Sort Descriptions to new ones.
            for (Int32 i = 0; i < currentFieldsForSort.Length; i++)
            {
                SortDescription descr = (SortDescription)currentFieldsForSort.GetValue(i);

                // Miss Fields which is not currently exists.
                if (!incorrectFieldsForSort.Contains(descr.PropertyName))
                {
                    sortableFields.SetValue(descr, fieldsCounter++);
                }
            }

            return sortableFields;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Format string for columns settings names.
        /// </summary>
        private const string COLUMNS_SETTINGS_NAMES_FORMAT_STRING = "/{0}";

        /// <summary>
        /// Route info settings name.
        /// </summary>
        private const string ROUTE_INFO_SETTINGS_NAME = "/RouteInfo";

        #endregion

        #region Private members

        /// <summary>
        /// Grid settings.
        /// </summary>
        private SettingsRepository _settingRepository = new SettingsRepository();

        #endregion
    }
}
