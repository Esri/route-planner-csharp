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
using System.Security.Principal;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ESRI.ArcLogistics
{
    static internal class FileHelpers
    {
        #region public static methods

        public static bool IsAbsolutPath(string path)
        {
            bool result = false;

            if (char.IsLetter(path[0]) && path[1] == ':' && IsSlash(path[2]))
            {
                result = true;
            }

            return result;
        }

        private static bool IsSlash(char letter)
        {
            if (letter == '/' || letter == '\\')
                return true;
            else
                return false;
        }

        static public bool IsFileNameCorrect(string fileName)
        {
            bool bOk = false;
            try
            {
                new FileInfo(fileName);
                bOk = true;
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (NotSupportedException)
            {
            }
            return bOk;
        }

        /// <summary>
        /// Gets whether the specified path is a valid absolute file path.
        /// </summary>
        /// <param name="path">Any path. OK if null or empty.</param>
        public static bool ValidateFilepath(string path)
        {
            if (path == null || string.IsNullOrEmpty(path.Trim()))
                return false;

            string pathName = string.Empty;
            string fileName = string.Empty;
            string fileExt = string.Empty;

            bool result = true;
            try
            {
                fileName = Path.GetFileNameWithoutExtension(path);
                fileExt = Path.GetExtension(path);
                pathName = Path.GetDirectoryName(path);
            }
            catch
            {
                result = false;
            }
            if (!result)
                return false;

            if (string.IsNullOrEmpty(fileName.Trim()))
                return false;

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                return false;

            if (!string.IsNullOrEmpty(fileExt.Trim()) && fileExt.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                return false;

            if (!string.IsNullOrEmpty(pathName.Trim()) && pathName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                return false;

             return true;
        }

        /// <summary>
        /// Check writing access to path
        /// </summary>
        static public bool CheckWriteAccess(string path)
        {
            bool result = true;
            try
            {
                FileSecurity security = File.GetAccessControl(path,
                    AccessControlSections.Access | AccessControlSections.Owner);
                NTAccount owner = (NTAccount)security.GetOwner(typeof(NTAccount));

                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(NTAccount)))
                {
                    if (rule.AccessControlType != AccessControlType.Allow)
                    {
                        result = false;
                        break; // NOTE: all must Allow - first breakdown - exit
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Check is file SHP-file
        /// </summary>
        static public bool IsShapeFile(string filePath)
        {
            bool isShapeFile = false;
            if (ValidateFilepath(filePath))
                isShapeFile = Path.GetExtension(filePath).Equals(SHAPE_FILE_EXT, StringComparison.OrdinalIgnoreCase);

            return isShapeFile;
        }

        /// <summary>
        /// Deletes file silently.
        /// </summary>
        public static void DeleteFileSilently(string path)
        {
            Debug.Assert(path != null);

            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion

        private const string SHAPE_FILE_EXT = ".shp";
    }
}
