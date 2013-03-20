using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Project properties interface.
    /// </summary>
    public interface IProjectProperties
    {
        // REV: rename to GetPropertyNames
        /// <summary>
        /// Collection of presented properties name
        /// </summary>
        /// <remarks>Read only collection</remarks>
        ICollection<string> GetPropertiesName();

        /// <summary>
        /// Properties indexer
        /// </summary>
        string this[string name] { get; }

        /// <summary>
        /// Get property by name
        /// </summary>
        string GetPropertyByName(string name);

        /// <summary>
        /// Update property value
        /// </summary>
        /// <remarks>If not present, automatical adding to properties</remarks>
        void UpdateProperty(string name, string value);

        /// <summary>
        /// Add property to properties
        /// </summary>
        /// <remarks>Name must be unique</remarks>
        void AddProperty(string name, string value);

        /// <summary>
        /// Remove property from properties
        /// </summary>
        void RemoveProperty(string name);
    }
}
