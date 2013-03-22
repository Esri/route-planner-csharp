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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using System.ComponentModel;

namespace ESRI.ArcLogistics.BreaksHelpers
{
    /// <summary>
    /// All types of brakes which are available in project.
    /// </summary>
    public enum BreakType
    {
        TimeWindow,
        DriveTime,
        WorkTime
    }

    /// <summary>
    ///  Project's default breaks settings.
    /// </summary>
    public class BreaksSettings: INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="projectProperties"><c>ProjectProperties</c></param>
        internal BreaksSettings(IProjectProperties projectProperties)
        {
            Debug.Assert(null != projectProperties);

            _projectProperties = projectProperties;

            _InitBreaksSettings();
            DefaultBreaks.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_DefaultBreaksPropertyChanged);
        }

        #endregion 

        #region Public properties
        
        /// <summary>
        /// Project's selected type of breaks.
        /// </summary>
        public BreakType? BreaksType
        {
            get { return _config.BreaksType; }
            set
            {
                if (_config.BreaksType != value)
                {
                    _config.BreaksType = value;
                    _UpdateConfig();
                    _NotifyPropertyChanged(BREAKS_TYPE_PROPERTY_NAME);
                }
            }
        }

        /// <summary>
        /// Project's collection of default breaks.
        /// </summary>
        public Breaks DefaultBreaks
        {
            get { return _config.DefaultBreaks; }
            set
            {
                if (_config.DefaultBreaks != value)
                {
                    _config.DefaultBreaks = value;
                    _UpdateConfig();
                    _NotifyPropertyChanged(DEFAULT_BREAKS_PROPERTY_NAME);
                }
            }
        }
        
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private methods

        /// <summary>
        /// Notifies about change of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void _NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Create default config.
        /// </summary>
        private void _CreateDefaultConfig()
        {
            _config = new BreaksConfig();
            _config.BreaksType = null;
            _config.DefaultBreaks = new Breaks();
        }

        /// <summary>
        /// Read Project's Breaks Settings from XML.
        /// </summary>
        private void _InitBreaksSettings()
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

                    DataContractSerializer ser = new DataContractSerializer(typeof(BreaksConfig));
                    _config = (BreaksConfig)ser.ReadObject(stream);
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
                // If no config was read, then create default.
                _CreateDefaultConfig();
        }

        /// <summary>
        /// Save Project's Brakes Settings to XML.
        /// </summary>
        private void _UpdateConfig()
        {
            Debug.Assert(null != _projectProperties);
            DataContractSerializer ser = new DataContractSerializer(typeof(BreaksConfig));

            string serialized = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ser.WriteObject(memoryStream, _config);
                serialized = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            _projectProperties.UpdateProperty(CONFIG_PROPERTY_NAME, serialized);
        }

        #endregion 

        #region Private event handlers

        /// <summary>
        /// When DefaultBreaks changed, saving settings to XML.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DefaultBreaksPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _UpdateConfig();
        }

        #endregion

        #region Private members

        /// <summary>
        /// BreaksConfiguration.
        /// </summary>
        private BreaksConfig _config = null;

        /// <summary>
        /// ProjectProperties.
        /// </summary>
        private IProjectProperties _projectProperties = null;

        #endregion 

        #region Constants

        /// <summary>
        /// Name of the BreaksConfig property.
        /// </summary>
        private const string CONFIG_PROPERTY_NAME = "BreaksConfig";

        /// <summary>
        /// Name of the Breaks Type property.
        /// </summary>
        private const string BREAKS_TYPE_PROPERTY_NAME = "BreaksType";

        /// <summary>
        /// Name of the Breaks Type property.
        /// </summary>
        private const string DEFAULT_BREAKS_PROPERTY_NAME = "DefaultBreaks";
        
        #endregion 
    

    }
}
