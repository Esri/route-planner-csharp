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

namespace ESRI.ArcLogistics.Archiving
{
    /// <summary>
    /// ProjectArchivingConfig class.
    /// </summary>
    [DataContract]
    internal class ProjectArchivingConfig
    {
        [DataMember]
        public bool IsArchive
        {
            get;
            set;
        }

        [DataMember]
        public bool IsAutoArchivingEnabled
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? LastArchivingDate
        {
            get;
            set;
        }

        [DataMember]
        public int AutoArchivingPeriod
        {
            get;
            set;
        }

        [DataMember]
        public int TimeDomain
        {
            get;
            set;
        }
    }
}
