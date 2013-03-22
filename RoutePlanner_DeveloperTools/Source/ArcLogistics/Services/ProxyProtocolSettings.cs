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
using ESRI.ArcLogistics.Utility.ComponentModel;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to a proxy settings for particular protocol (e.g. HTTP or HTTPS).
    /// </summary>
    [Serializable]
    internal sealed class ProxyProtocolSettings : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Gets or sets proxy host address.
        /// </summary>
        public string Host
        {
            get
            {
                return _host;
            }

            set
            {
                if (_host != value)
                {
                    _host = value;
                    this.NotifyPropertyChanged("Host");
                }
            }
        }

        /// <summary>
        /// Gets or sets proxy port number.
        /// </summary>
        public int? Port
        {
            get
            {
                return _port;
            }

            set
            {
                if (_port != value)
                {
                    _port = value;
                    this.NotifyPropertyChanged("Port");
                }
            }
        }

        #region private fields
        /// <summary>
        /// Stores value of the <see cref="Host"/> property.
        /// </summary>
        private string _host;

        /// <summary>
        /// Stores value of the <see cref="Port"/> property.
        /// </summary>
        private int? _port;
        #endregion
    }
}
