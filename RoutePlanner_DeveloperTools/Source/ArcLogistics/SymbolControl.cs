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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Xaml;
using System.Xml;
using System.Windows.Markup;
using System.Diagnostics;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Control container for Symbol ControlTemplate
    /// </summary>
    internal class SymbolControl : Control
    {
        #region constructors

        public SymbolControl(ControlTemplate template)
        {
            Template = template;
            _dataContext = new SymbolControlContext();
            DataContext = _dataContext;
            HorizontalContentAlignment = HorizontalAlignment.Center;
        }
        
        #endregion

        #region public members

        /// <summary>
        /// Color of Symbol
        /// </summary>
        public SolidColorBrush Fill
        {
            get
            {
                return (SolidColorBrush)_dataContext.Attributes[SymbolControlContext.FILL_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbolControlContext.FILL_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Sequence Number
        /// </summary>
        public string SequenceNumber
        {
            get
            {
                return (string)_dataContext.Attributes[SymbolControlContext.SEQUENCE_NUMBER_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbolControlContext.SEQUENCE_NUMBER_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Route geometry
        /// </summary>
        public PathGeometry Geometry
        {
            get
            {
                return (PathGeometry)_dataContext.Attributes[SymbolControlContext.GEOMETRY_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbolControlContext.GEOMETRY_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        #endregion

        #region private members

        private SymbolControlContext _dataContext;

        #endregion
    }
}