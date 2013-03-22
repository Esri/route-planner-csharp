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

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Provides access to global setting. 
    /// </summary>
    [DataContract]
    internal sealed class Setting : DataRecordBase
    {
        /// <summary>
        /// Gets or sets a value identifying specific setting.
        /// </summary>
        [DataMember]
        public Guid KeyID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value the date to use route settings for.
        /// </summary>
        [DataMember]
        public DateTime? PlannedDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value specifying setting value part stored in the current object.
        /// </summary>
        [DataMember]
        public int PartIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets setting value.
        /// </summary>
        [DataMember]
        public string Value
        {
            get;
            set;
        }
    }
}
