
/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
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
