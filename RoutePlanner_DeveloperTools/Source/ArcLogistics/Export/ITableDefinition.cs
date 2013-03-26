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
using System.Collections.Generic;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Export table type specifies table data content.
    /// </summary>
    public enum TableType
    {
        /// <summary>
        /// Schedules data table.
        /// </summary>
        Schedules,
        /// <summary>
        /// Routes data table.
        /// </summary>
        Routes,
        /// <summary>
        /// Stops data table (all stops: locations, all orders, breaks).
        /// </summary>
        Stops,
        /// <summary>
        /// Orders data table (contains orders of schedules and unassigned orders).
        /// </summary>
        Orders,
        /// <summary>
        /// Exports schema for special fields table.
        /// </summary>
        Schema
    }

    /// <summary>
    /// Table definition interface.
    /// </summary>
    public interface ITableDefinition
    {
        /// <summary>
        /// Table type.
        /// </summary>
        TableType Type { get; }

        /// <summary>
        /// Table name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets collection of supported fields names.
        /// </summary>
        ICollection<string> SupportedFields { get; }

        /// <summary>
        /// Adds field to export field collection.
        /// </summary>
        /// <remarks>Name from <c>SupportedFields</c> collection.</remarks>
        void AddField(string name);

        /// <summary>
        /// Removes field from export field collection.
        /// </summary>
        /// <remarks>Name from <c>SupportedFields</c> collection.</remarks>
        void RemoveField(string name);

        /// <summary>
        /// Removes all fields from export fields collection.
        /// </summary>
        void ClearFields();

        /// <summary>
        /// Gets field collection that must be present in the export table.
        /// </summary>
        ICollection<string> Fields { get; }

        /// <summary>
        /// Gets field title by name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Returns localizable field name that corresponds to the <c>name</c> field.</returns>
        string GetFieldTitleByName(string name);

        /// <summary>
        /// Gets field name by title.
        /// </summary>
        /// <param name="faceName">Field title.</param>
        /// <returns>Returns field name that corresponds to the <c>faceName</c> title.</returns>
        string GetFieldNameByTitle(string faceName);

        /// <summary>
        /// Gets field description by name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Returns localizable field description.</returns>
        string GetDescriptionByName(string name);
    }
}
