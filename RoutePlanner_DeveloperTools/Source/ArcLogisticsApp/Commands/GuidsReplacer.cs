using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class which replace Guids in string with corresponding domain objects names.
    /// </summary>
    internal static class GuidsReplacer
    {
        #region Public methods

        /// <summary>
        /// Replace guids in string.
        /// </summary>
        /// <param name="message">Message with guids.</param>
        /// <param name="project">Current project.</param>
        /// <returns>Message with guids replaced with names.</returns>
        internal static string ReplaceGuids(string message, Project project)
        {
            string newMessage = message;

            var guids = _GetGUIDS(newMessage);

            // For each found guid replace it in string.
            foreach (var guid in guids)
            {
                var name = _GetName(guid.Value, project);
                newMessage = newMessage.Replace(guid.Key, name);
            }

            return newMessage;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Get guids from string.
        /// </summary>
        /// <param name="message">String with guids.</param>
        /// <returns>Collection of key value pair. 
        /// Key is guid string and value is corresponding guid.</returns>
        private static IEnumerable<KeyValuePair<string, Guid>> _GetGUIDS(string message)
        {
            var result = new List<KeyValuePair<string, Guid>>();

            // Split string by quotes.
            var splitByQuotes = message.Split(QUOTE);

            Guid guid;
            foreach (var guidString in splitByQuotes)
            {
                // Check that splitted string is guid.
                if (Guid.TryParse(guidString, out guid))
                {
                    // If it is - add corresponding key value pair to result.
                    result.Add(new KeyValuePair<string, Guid>(guidString, guid));
                }
            }
            return result;
        }

        /// <summary>
        /// Get name of object with specified guid.
        /// </summary>
        /// <param name="id">Id to search.</param>
        /// <param name="project">Current project.</param>
        /// <returns>Name of object which id was specifed. 
        /// If such object hasn't been found - return guid string representation.</returns>
        private static string _GetName(Guid id, Project project)
        {
            ISupportName obj;
            // Check that location with such id exist.
            obj = project.Locations.FirstOrDefault(x => x.Id == id) as ISupportName;
            // If object hasn't been found - check routes.
            if (obj == null)
                obj = project.Schedules.SearchRoute(id) as ISupportName;
            // If object hasn't been found - check orders.
            if (obj == null)
                obj = project.Orders.SearchById(id) as ISupportName;
            // If object hasn't been found - check barriers.
            if (obj == null)
                obj = project.Barriers.SearchById(id) as ISupportName;

            // If object has been found - return its name.
            if (obj != null)
                return obj.Name;
            // If it hasn't - return guid's string representation.
            else
            {
                Debug.Assert(false);
                return id.ToString();
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Quotes char.
        /// </summary>
        private const char QUOTE = '\"';

        #endregion
    }
}
