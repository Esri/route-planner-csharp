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
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Archiving
{
    /// <summary>
    /// Project archiving settings
    /// </summary>
    public class ProjectArchivingSettings
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal ProjectArchivingSettings(IProjectProperties projectProperties)
        {
            Debug.Assert(null != projectProperties);

            _projectProperties = projectProperties;
            _InitArchivingSettings();
        }

        #endregion // Constructor

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is project archive flag
        /// </summary>
        public bool IsArchive
        {
            get { return _config.IsArchive; }
            set
            {
                if (_config.IsArchive != value)
                {
                    _config.IsArchive = value;
                    _UpdateConfig();
                }
            }
        }

        /// <summary>
        /// Is auto archiving enabled flag
        /// </summary>
        public bool IsAutoArchivingEnabled
        {
            get { return _config.IsAutoArchivingEnabled; }
            set
            {
                if (_config.IsAutoArchivingEnabled != value)
                {
                    _config.IsAutoArchivingEnabled = value;
                    _UpdateConfig();
                }
            }
        }

        /// <summary>
        /// Date of last archiving
        /// </summary>
        public DateTime? LastArchivingDate
        {
            get { return _config.LastArchivingDate; }
            set
            {
                if (_config.LastArchivingDate != value)
                {
                    _config.LastArchivingDate = value;
                    _UpdateConfig();
                }
            }
        }

        /// <summary>
        /// Auto archiving period
        /// </summary>
        public int AutoArchivingPeriod
        {
            get { return _config.AutoArchivingPeriod; }
            set
            {
                if (_config.AutoArchivingPeriod != value)
                {
                    _config.AutoArchivingPeriod = value;
                    _UpdateConfig();
                }
            }
        }

        /// <summary>
        /// Auto archiving time domain
        /// </summary>
        public int TimeDomain
        {
            get { return _config.TimeDomain; }
            set
            {
                if (_config.TimeDomain != value)
                {
                    _config.TimeDomain = value;
                    _UpdateConfig();
                }
            }
        }

        #endregion // Public properties

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create default config
        /// </summary>
        private void _CreateDefaultConfig()
        {
            _config = new ProjectArchivingConfig();

            _config.IsArchive = false;
            _config.IsAutoArchivingEnabled = false;
            _config.LastArchivingDate = null;
            _config.AutoArchivingPeriod = CONFIG_ARCHIVING_PERIOD;
            _config.TimeDomain = CONFIG_ARCHIVING_TIMEDOMAIN;
        }

        /// <summary>
        /// Init project Archiving Settings
        /// </summary>
        private void _InitArchivingSettings()
        {
            Debug.Assert(null != _projectProperties);
            _config = null;

            MemoryStream stream = null;
            try
            {
                string configText = _projectProperties.GetPropertyByName(CONFIG_PROPERTY_NAME);
                if (!string.IsNullOrEmpty(configText))
                {
                    stream = new MemoryStream(Encoding.UTF8.GetBytes(configText));

                    DataContractSerializer ser = new DataContractSerializer(typeof(ProjectArchivingConfig));
                    _config = (ProjectArchivingConfig)ser.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            finally
            {
                if (null != stream)
                    stream.Close();
            }

            if (null == _config)
                _CreateDefaultConfig();
        }

        /// <summary>
        /// Update config
        /// </summary>
        private void _UpdateConfig()
        {
            Debug.Assert(null != _projectProperties);
            DataContractSerializer ser = new DataContractSerializer(typeof(ProjectArchivingConfig));

            string serialized = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ser.WriteObject(memoryStream, _config);
                serialized = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            _projectProperties.UpdateProperty(CONFIG_PROPERTY_NAME, serialized);
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ProjectArchivingConfig _config = null;
        private IProjectProperties _projectProperties = null;

        #endregion // Private members

        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string CONFIG_PROPERTY_NAME = "ArchivingConfig";
        private const int CONFIG_ARCHIVING_PERIOD = 1;
        private const int CONFIG_ARCHIVING_TIMEDOMAIN = 2;

        #endregion // Constants
    }
}
