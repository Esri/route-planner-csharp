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
using System.Xml;
using System.Windows.Markup;
using System.Diagnostics;
using ESRI.ArcLogistics.App.OrderSymbology;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments.Controls
{
    /// <summary>
    /// Control container for Symbol ControlTemplate
    /// </summary>
    internal class SymbolControl : Control
    {
        #region constructors

        /// <summary>
        /// Instantiates new instance of <c>SymbolControl</c> class.
        /// </summary>
        /// <param name="template">Control template.</param>
        public SymbolControl(ControlTemplate template)
        {
            Template = template;

            _dataContext = new SymbologyContext();
            _dataContext.Attributes[SymbologyContext.SIZE_ATTRIBUTE_NAME] = SymbologyManager.DEFAULT_SIZE;
            _dataContext.Attributes[SymbologyContext.FULLSIZE_ATTRIBUTE_NAME] = SymbologyManager.DEFAULT_SIZE + SymbologyManager.DEFAULT_INDENT;
            DataContext = _dataContext;

            HorizontalContentAlignment = HorizontalAlignment.Center;
        }
        
        #endregion

        #region public members

        /// <summary>
        /// Color of Symbol.
        /// </summary>
        public SolidColorBrush Fill
        {
            get
            {
                return (SolidColorBrush)_dataContext.Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.FILL_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Sequence Number.
        /// </summary>
        public string SequenceNumber
        {
            get
            {
                return (string)_dataContext.Attributes[SymbologyContext.SEQUENCE_NUMBER_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.SEQUENCE_NUMBER_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Indicates whether stop is locked.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                return (bool)_dataContext.Attributes[SymbologyContext.IS_LOCKED_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.IS_LOCKED_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Indicates whether stop is violated.
        /// </summary>
        public bool IsViolated
        {
            get
            {
                return (bool)_dataContext.Attributes[SymbologyContext.IS_VIOLATED_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.IS_VIOLATED_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Size of the symbol.
        /// </summary>
        public double Size
        {
            get
            {
                return (double)_dataContext.Attributes[SymbologyContext.SIZE_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.SIZE_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Full size of the symbol including selection frame.
        /// </summary>
        public double FullSize
        {
            get
            {
                return (double)_dataContext.Attributes[SymbologyContext.FULLSIZE_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.FULLSIZE_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Rendering offset of the symbol by X axis.
        /// </summary>
        public int OffsetX
        {
            get
            {
                return (int)_dataContext.Attributes[SymbologyContext.OFFSETX_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.OFFSETX_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Rendering offset of the symbol by Y axis.
        /// </summary>
        public int OffsetY
        {
            get
            {
                return (int)_dataContext.Attributes[SymbologyContext.OFFSETY_ATTRIBUTE_NAME];
            }
            set
            {
                ControlTemplate ct = Template;
                Template = null;
                _dataContext.Attributes[SymbologyContext.OFFSETY_ATTRIBUTE_NAME] = value;
                Template = ct;
            }
        }

        /// <summary>
        /// Gets symbology context dictionary. When sets it just copies values from input dictionary
        /// to internal symbology context.
        /// </summary>
        public IDictionary<string, object> SymbologyContextDictionary
        {
            get
            {
                return _dataContext.Attributes;
            }
            set
            {
                Debug.Assert(value != null);

                // Copy values from input dictionary.
                foreach (var key in value.Keys)
                    _dataContext.Attributes[key] = value[key];
            }
        }

        #endregion

        #region private members

        /// <summary>
        /// Symbol's context that contains all the data.
        /// </summary>
        private SymbologyContext _dataContext;

        #endregion
    }
}