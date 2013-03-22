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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Markup;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows;

namespace ESRI.ArcLogistics.App.Symbols
{
    internal class TemplateMarkerSymbol : MarkerSymbol
    {
        /// <summary>
        /// Load template from embedded resource by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Resource</returns>
        protected static ControlTemplate _LoadTemplateFromResource(string key)
        {
            ControlTemplate controlTemplate = null;
            try
            {
                Stream stream = Application.Current.GetType().Assembly.GetManifestResourceStream(key);
                string template = new StreamReader(stream).ReadToEnd();
                StringReader stringReader = new StringReader(template);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                controlTemplate = XamlReader.Load(xmlReader) as ControlTemplate;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            return controlTemplate;
        }
    }
}
