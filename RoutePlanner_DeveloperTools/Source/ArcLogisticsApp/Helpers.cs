
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ESRI.ArcLogistics.App
{
    static class FileHelpers
    {
        public static bool IsSlash(char letter)
        {
            if (letter == '/' || letter == '\\')
                return true;
            else
                return false;
        }


        public static string FixSlash(string path, bool needSlash)
        {
            if (needSlash)
            {
                if (path.Length > 0 && !IsSlash(path[path.Length - 1]))
                {
                    path = path + "\\";
                }
            }
            else
            {
                if (path.Length > 0 && IsSlash(path[path.Length - 1]))
                    path.Remove(path.Length - 1, 1);
            }

            return path;
        }

        public static string MakePathToFile(string path, string fileName)
        {
            return FileHelpers.FixSlash(path, true) + fileName;
        }
            
        /// <summary>
        /// Gets whether the specified path is a valid absolute file path.
        /// </summary>
        /// <param name="path">Any path. OK if null or empty.</param>
        /// 
        public static bool ValidateFilepath(string path)
        {   
            if (path.Trim() == string.Empty)
            {
                return false;
            }

            string pathName = string.Empty;
            string fileName = string.Empty;

            bool result = true;
            try
            {
                fileName = System.IO.Path.GetFileName(path);
                pathName = System.IO.Path.GetPathRoot(path);
            }
            catch 
            {    
                result = false;   
            }

            if (!result)
                return false;
            else
            {
                if (fileName.Trim() == string.Empty)
                {
                    return false;
                }

                if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                {
                    return false;
                }

                if (pathName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                {
                    return false;
                }
            }
            return true;
        }

        //public static bool ValidateFilePath(string filePath)
        //{
        //    Regex r = new Regex( @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$" );
        //    return r.IsMatch(filePath);
        //}
    }
}
