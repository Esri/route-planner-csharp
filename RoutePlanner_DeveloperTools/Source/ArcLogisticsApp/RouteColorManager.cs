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
using System.Drawing;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections;
using ESRI.ArcLogistics.App.Properties;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class keeps default colors set and return necessary color for immediate route (Singleton)
    /// </summary> 
    internal class RouteColorManager
    {
        #region Static Properties

        /// <summary>
        /// Gets singletone instance
        /// </summary>
        public static RouteColorManager Instance
        {
            get
            {
                if (_routeColorManager == null)
                    _routeColorManager = new RouteColorManager();
                return _routeColorManager;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default private constructor
        /// </summary>
        private RouteColorManager()
        {
            try
            {
                _InitColors();
                App.Current.Exit += new System.Windows.ExitEventHandler(Current_Exit);
            }
            catch
            {
                // NOTE: setting string "ColorsSet" is empty or has incorrect format
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets default application color set
        /// </summary>
        public ReadOnlyCollection<Color> ColorsSet
        {
            get
            {
                List<Color> colors = new List<Color>();

                if (_userColors.Count > 0)
                {
                    colors.AddRange(_applicationColors);
                    colors.AddRange(_userColors);
                }
                else
                    colors = _applicationColors;


                return colors.AsReadOnly();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method clears user color set
        /// </summary>
        public void ClearUserColors()
        {
            _userColors.Clear();
        }

        /// <summary>
        /// Gets next route color 
        /// </summary>
        public Color NextRouteColor(ICollection<Route> usedRoutes)
        {
            Dictionary<Color, int> colorsUsing = new Dictionary<Color, int>();

            // fill colors using dictionary by default values
            foreach (Color color in ColorsSet)
                colorsUsing.Add(color, 0);

            Debug.Assert(ColorsSet.Count >= 1);

            // set result color as first by default
            Color resultColor = ColorsSet[0];

            // define count of using each color
            foreach (Route route in usedRoutes)
            {
                if (colorsUsing.ContainsKey(route.Color))
                    colorsUsing[route.Color]++;
            }

            // Get 1-st min color occurence value
            int min = colorsUsing.Values.Min();

            // find according color
            foreach (KeyValuePair<Color, Int32> value in colorsUsing)
            {
                if (value.Value == min)
                {
                    resultColor = value.Key;
                    break;
                }
            }

            return resultColor;
        }

        /// <summary>
        /// Method adds color into user's colors collection if such color is absent there
        /// </summary>
        /// <param name="color"></param>
        public void AddUserColor(Color color)
        {
            Debug.Assert(_userColors != null);
            Debug.Assert(_applicationColors != null);

            if (_applicationColors.Contains(color) || _userColors.Contains(color))
                return;

            if (_userColors.Count == MAX_USER_COLORS_COUNT) // if all colors are filled - remove the last one
                _userColors.RemoveAt(MAX_USER_COLORS_COUNT - 1);

            _userColors.Insert(0, color);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method loads application routes colors set from user.config. If this resource is empty - loads default colors from Application.Settings 
        /// </summary>
        private void _InitColors()
        {
            Debug.Assert(!string.IsNullOrEmpty(Settings.Default.ColorsSet));

            _applicationColors = _LoadColors(Settings.Default.ColorsSet);

            // if string in user.config is not empty - load use defined colors set 
            if (!string.IsNullOrEmpty(Settings.Default.CustomColorsSet))
                _userColors = _LoadColors(Settings.Default.CustomColorsSet);
        }

        /// <summary>
        /// Method loads colors set from stated resource path. 
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        private List<Color> _LoadColors(string resourcePath)
        {
            List<Color> outputCollection = new List<Color>();

            XmlTextReader reader = null;

            int R, G, B;

            Debug.Assert(!string.IsNullOrEmpty(resourcePath));

            try
            {
                // Read resource string as XML
                reader = new XmlTextReader(new StringReader(resourcePath));

                while (reader.Read())
                {
                    if (reader.Name == COLOR_NODE)
                    {
                        // Get "red" value
                        reader.MoveToNextAttribute();
                        R = Math.Abs(Convert.ToInt32(reader.Value)); // get absolute value of color to exclude negative values
                        Debug.Assert(R <= MAX_COLOR_VALUE);
                        R = (R > MAX_COLOR_VALUE) ? MAX_COLOR_VALUE : R; // define correct value of color

                        //// Get "green" value
                        reader.MoveToNextAttribute();
                        G = Math.Abs(Convert.ToInt32(reader.Value)); // get absolute value of color to exclude negative values
                        Debug.Assert(G <= MAX_COLOR_VALUE);
                        G = (G > MAX_COLOR_VALUE) ? MAX_COLOR_VALUE : G; // define correct value of color

                        // Get "blue" value
                        reader.MoveToNextAttribute();
                        B = Math.Abs(Convert.ToInt32(reader.Value)); // get absolute value of color to exclude negative values
                        Debug.Assert(B <= MAX_COLOR_VALUE);
                        B = (B > MAX_COLOR_VALUE) ? MAX_COLOR_VALUE : B; // define correct value of color

                        outputCollection.Add(Color.FromArgb(MAX_COLOR_VALUE, R, G, B));
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                reader.Close();
            }

            return outputCollection;
        }

        /// <summary>
        /// Method saves changed colors set in user.config file
        /// </summary>
        private void _SaveUserColorsSet()
        {
            Debug.Assert(!string.IsNullOrEmpty(Settings.Default.ColorsSet));

            try
            {
                // create xml with the same structure as Application.Settings.Default.ColorsSet
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Settings.Default.ColorsSet); // get root element from Application.Settings.Default.ColorsSet
                XmlNode colors = doc.GetElementsByTagName(COLORS_NODE)[0]; // get first element with name "colors"
                colors.RemoveAll(); // clear all childe nodes of this element (for replace values)

                foreach (Color color in _userColors) // add new child nodes to "colors" element
                {
                    // create new element with R,G,B color values
                    XmlElement colorElement = doc.CreateElement(COLOR_NODE);
                    colorElement.SetAttribute(R_ATTRIBUTE, color.R.ToString());
                    colorElement.SetAttribute(G_ATTRIBUTE, color.G.ToString());
                    colorElement.SetAttribute(B_ATTRIBUTE, color.B.ToString());
                    colors.AppendChild(colorElement);
                }

                Settings.Default.CustomColorsSet = doc.InnerXml.ToString(); // replace CustomColorSet
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        #endregion

        #region Event Handlers

        private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            try
            {
                _SaveUserColorsSet();
            }
            catch
            {
                // NOTE : error in settings file (ColorsSet or CustomColorSet string)
            }
        }

        #endregion

        #region Private Fields

        private const int MAX_COLOR_VALUE = 255;
        private const string COLOR_NODE = "color";
        private const int MAX_USER_COLORS_COUNT = 6;

        private const string COLORS_NODE = "colors";
        private const string R_ATTRIBUTE = "R";
        private const string G_ATTRIBUTE = "G";
        private const string B_ATTRIBUTE = "B";

        private static RouteColorManager _routeColorManager;

        // default application colors set
        private List<Color> _applicationColors;

        // user defined colors
        private List<Color> _userColors = new List<Color>();

        #endregion
    }
}
