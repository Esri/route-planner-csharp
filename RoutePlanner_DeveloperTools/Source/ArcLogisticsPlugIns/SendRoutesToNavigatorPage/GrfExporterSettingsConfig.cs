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
using System.Xml.Serialization;
using ESRI.ArcLogistics;
using ESRI.ArcLogistics.Utility;
using System.Diagnostics;
using System.ComponentModel;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// GrfExporter settings config class
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GrfExporterSettingsConfig
    {
        #region constants

        private const int MAIL_PORT = 25;

        #endregion

        #region constructors

        /// <summary>
        /// Hide constructor
        /// </summary>
        private GrfExporterSettingsConfig()
        { }

        #endregion

        #region public methods

        /// <summary>
        /// Deserialize grf exporter settings config from string
        /// </summary>
        /// <param name="configText">String with serialized settings</param>
        public void Deserialize(string configText)
        {
            XmlSerializer ser = new XmlSerializer(typeof(GrfExporterSettingsConfig));

            StringReader reader = new StringReader(configText);

            GrfExporterSettingsConfig deserialized = null;
            try
            {
                deserialized = (GrfExporterSettingsConfig)ser.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                reader.Close();
            }

            // if settings loaded - copy it to instance
            if (deserialized != null)
            {
                ServerAddress = deserialized.ServerAddress;
                ServerPort = deserialized.ServerPort;
                AutenticationRequired = deserialized.AutenticationRequired;
                UserName = deserialized.UserName;
                Password = deserialized.Password;
                RememberPassword = deserialized.RememberPassword;
                EncryptedConnectionRequires = deserialized.EncryptedConnectionRequires;
            }
        }

        /// <summary>
        /// Serialize mailer settings config
        /// </summary>
        /// <returns>Serialized mailer settings config</returns>
        public string Serialize()
        {
            XmlSerializer ser = new XmlSerializer(typeof(GrfExporterSettingsConfig));
            MemoryStream writer = new MemoryStream();

            string serialized = "";
            try
            {
                ser.Serialize(writer, this);
                serialized = Encoding.UTF8.GetString(writer.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                writer.Close();
            }

            return serialized;
        }

        #endregion

        #region public members

        /// <summary>
        /// Plugin settings instance
        /// </summary>
        public static GrfExporterSettingsConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = _Create();
                }

                return _instance;
            }
        }


        /// <summary>
        /// Server Address
        /// </summary>
        [XmlElement("ServerAddress")]
        public string ServerAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Server port
        /// </summary>
        [XmlElement("ServerPort")]
        public int ServerPort
        {
            get;
            set;
        }

        /// <summary>
        /// Is autentication required
        /// </summary>
        [XmlElement("AutenticationRequired")]
        public bool AutenticationRequired
        {
            get;
            set;
        }

        /// <summary>
        /// User name
        /// </summary>
        [XmlElement("UserName")]
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Password
        /// </summary>
        [XmlIgnore()]
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Encrypted password
        /// </summary>
        [XmlAttribute("password")]
        public string XML_Password
        {
            get
            {
                return Password != null ?
                    StringProcessor.TransformData(Password) : Password;
            }
            set
            {
                var password = default(string);
                if (value != null && StringProcessor.TryTransformDataBack(value, out password))
                {
                    Password = password;
                }
            }
        }

        /// <summary>
        /// Remember password
        /// </summary>
        [XmlElement("RememberPassword")]
        public bool RememberPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Is encrypted connection requires
        /// </summary>
        [XmlElement("EncryptedConnectionRequires")]
        public bool EncryptedConnectionRequires
        {
            get;
            set;
        }

        /// <summary>
        /// Route grf compression
        /// </summary>
        [XmlElement("RouteGrfCompression")]
        public bool RouteGrfCompression
        {
            get;
            set;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Serialize grf exporter settings config
        /// </summary>
        /// <returns>Serialized grf exporter settings config</returns>
        private static GrfExporterSettingsConfig _Create()
        {
            GrfExporterSettingsConfig grfExporterSettingsConfig = new GrfExporterSettingsConfig();

            grfExporterSettingsConfig.ServerPort = MAIL_PORT;
            grfExporterSettingsConfig.AutenticationRequired = true;
            grfExporterSettingsConfig.RememberPassword = true;

            return grfExporterSettingsConfig;
        }

        #endregion

        #region private members

        private static GrfExporterSettingsConfig _instance;

        #endregion
    }
}
