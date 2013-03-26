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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using System.ServiceModel;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class contains helper method for application simplifying routine.
    /// </summary>
    internal sealed class CommonHelpers
    {
        #region Public definitions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string XML_SETTINGS_INDENT_CHARS = "    ";

        //
        // custom serialization conts (into the project database).
        //

        /// <summary>
        /// Storage culture name.
        /// </summary>
        public const string STORAGE_CULTURE = "en-US";

        /// <summary>
        /// Application separator.
        /// </summary>
        public const char SEPARATOR = ',';
        /// Old version of separator (NOTE: is obsolete - now need use SEPARATOR).
        /// </summary>
        public const char SEPARATOR_OLD = ';';
        /// <summary>
        /// Application separator for data with ',' (double or composite).
        /// </summary>
        public const string SEPARATOR_ALIAS = "&comma";
        /// <summary>
        /// Part separator (used wiht versionen info).
        /// </summary>
        public const char SEPARATOR_OF_PART = '\\';

        #endregion // Public definitions

        #region Public helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets solver settings.
        /// </summary>
        /// <param name="solver">The reference to the VRP solver.</param>
        /// <returns>Solver settings or NULL.</returns>
        internal static SolverSettings GetSolverSettings(IVrpSolver solver)
        {
            Debug.Assert(solver != null);

            SolverSettings settings = null;
            try
            {
                settings = solver.SolverSettings;
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException ||
                    e is AuthenticationException ||
                    e is CommunicationException ||
                    e is FaultException)
                {
                    Logger.Info(e);
                }
                else
                {
                    throw; // exception
                }
            }

            return settings;
        }

        /// <summary>
        /// Get list of assembly file in directory.
        /// </summary>
        /// <param name="directoryPath">Directory full path</param>
        /// <returns>List of assembly file in directory</returns>
        /// <remarks>Not supported subfolder.</remarks>
        public static ICollection<string> GetAssembliesFiles(string directoryPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(directoryPath));

            List<string> list = new List<string>();
            if (!Directory.Exists(directoryPath))
                return list;

            try
            {
                //do through all the files in the plugin directory
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    FileInfo file = new FileInfo(filePath);

                    if (!file.Extension.Equals(".dll"))
                        continue; // NOTE: preliminary check, must be ".dll"

                    try
                    {
                        Assembly pluginAssembly = Assembly.LoadFrom(filePath);
                        list.Add(filePath);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return list.AsReadOnly();
        }

        /// <summary>
        /// Normalizes text.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Text in normalized state.</returns>
        public static string NormalizeText(string text)
        {
            Debug.Assert(!string.IsNullOrEmpty(text.Trim()));

            string normText = text.Trim().ToLower();
            return normText.Replace(SPACE, ""); // remove spaces
        }

        /// <summary>
        /// Checks is string value present in list.
        /// </summary>
        /// <param name="value">Value to cheking.</param>
        /// <param name="values">Value list.</param>
        /// <returns>TRUE if input value present in supported list.</returns>
        public static bool IsValuePresentInList(string value, string[] values)
        {
            Debug.Assert(!string.IsNullOrEmpty(value.Trim()));

            bool result = false;
            if (null != values)
            {
                for (int index = 0; index < values.Length; ++index)
                {
                    string curValue = NormalizeText(values[index]);
                    if (value == curValue)
                    {
                        result = true;
                        break; // NOTE: result founded
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sorts Stop objects respecting sequence number.
        /// </summary>
        /// <param name="stops">Stops to sorting.</param>
        public static void SortBySequence(List<Stop> stops)
        {
            stops.Sort(delegate(Stop s1, Stop s2)
            {
                return s1.SequenceNumber.CompareTo(s2.SequenceNumber);
            });
        }

        /// <summary>
        /// Gets route stops sorted by sequence number.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>Route stops sorted by sequence number.</returns>
        public static IList<Stop> GetSortedStops(Route route)
        {
            var routeStops = new List<Stop>(route.Stops);
            SortBySequence(routeStops);

            return routeStops;
        }

        #endregion // Public helpers

        /// <summary>
        /// Symbol SPACE (as string).
        /// </summary>
        private const string SPACE = " ";
    }
}
