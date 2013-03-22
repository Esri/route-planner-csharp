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
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GPResponse class.
    /// </summary>
    [DataContract]
    internal class GPResponse : IFaultInfo
    {
        #region IFaultInfo interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool IsFault
        {
            get { return _isFault; }
        }

        [DataMember(Name = "error")]
        public GPError FaultInfo
        {
            get { return _error; }
            set
            {
                _error = value;
                _isFault = true;
            }
        }

        #endregion IFaultInfo interface members

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private bool _isFault = false;
        private GPError _error;

        #endregion private fields
    }
}