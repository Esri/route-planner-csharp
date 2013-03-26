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
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Xml;
using System.Windows;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Symbols
{
    /// <summary>
    /// Custom order symbol class
    /// </summary>
    internal class CustomOrderSymbol : TemplateMarkerSymbol
    {
        public CustomOrderSymbol()
        {
            ControlTemplate = _template;
        }

        static ControlTemplate _template = _LoadTemplateFromResource("ESRI.ArcLogistics.App.Symbols.CustomOrderSymbol.xaml");
    }
}
