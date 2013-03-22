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
using System.ComponentModel;
using ESRI.ArcLogistics.App.Controls;
using System.Drawing;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.OrderSymbology
{
    /// <summary>
    /// Base class of Symbology records
    /// </summary>
    [DataContract]
    internal abstract class SymbologyRecord : INotifyPropertyChanged
    {
        #region Constants

        // property names
        public const string PROP_NAME_SymbolFilename = "SymbolFilename";
        public const string PROP_NAME_Size = "Size";
        public const string PROP_NAME_Angle = "Angle";
        public const string PROP_NAME_Color = "Color";
        public const string PROP_NAME_UseRouteColor = "UseRouteColor";
        public const string PROP_NAME_DefaultValue = "DefaultValue";

        private const int DEFAULT_SIZE = 16;
        private const int MAX_SIZE = 150;
        private const int DEFAULT_ANGLE = 0;
        private const bool DEFAULT_USE_ROUTE_COLOR = true;

        #endregion // Constants

        #region constructors

        public SymbologyRecord()
        {
            if (Symbol == null)
                Symbol = new SymbolBox();
            Symbol.ParentRecord = this;

            Color = Color.Black;
            Size = DEFAULT_SIZE;
            Angle = DEFAULT_ANGLE;
            UseRouteColor = DEFAULT_USE_ROUTE_COLOR;
        }

        #endregion

        #region public members

        /// <summary>
        /// Default symbology value
        /// </summary>
        [DataMember]
        public bool DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                _defaultValue = value;
                _NotifyPropertyChanged(PROP_NAME_SymbolFilename);
            }
        }

        /// <summary>
        /// FileName of controltemplate
        /// </summary>
        [DataMember]
        public string SymbolFilename
        {
            get 
            {
                return Symbol.TemplateFileName; 
            }
            set
            {
                if (Symbol == null)
                    Symbol = new SymbolBox();

                Symbol.TemplateFileName = value;
                _NotifyPropertyChanged(PROP_NAME_SymbolFilename);
            }
        }

        /// <summary>
        /// Symbol size
        /// </summary>
        [DataMember]
        public int Size
        {
            get { return _size; }
            set
            {
                if (Symbol == null)
                    Symbol = new SymbolBox();

                _size = value;
                if (_size < 1)
                    _size = DEFAULT_SIZE;
                if (_size > MAX_SIZE)
                    _size = MAX_SIZE;
                Symbol.Size = _size;
                _NotifyPropertyChanged(PROP_NAME_Size);
            }
        }

        /// <summary>
        /// Symbol Angle
        /// </summary>
        [DataMember]
        public int Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                _NotifyPropertyChanged(PROP_NAME_Angle);
            }
        }

        /// <summary>
        /// Symbol color
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                if (Symbol == null)
                    Symbol = new SymbolBox();

                _color = value;
                System.Windows.Media.Color mediaColor = 
                    System.Windows.Media.Color.FromArgb(_color.A, _color.R, _color.G, _color.B);
                Symbol.Fill = new System.Windows.Media.SolidColorBrush(mediaColor);
                _NotifyPropertyChanged(PROP_NAME_Color);
            }
        }

        /// <summary>
        /// Use route color flag
        /// </summary>
        [DataMember]
        public bool UseRouteColor
        {
            get { return _useRouteColor; }
            set
            {
                _useRouteColor= value;
                _NotifyPropertyChanged(PROP_NAME_UseRouteColor);
            }
        }

        /// <summary>
        /// Serializable presentation of color
        /// </summary>
        [DataMember]
        public Int32 ARGB
        {
            get
            {
                return _color.ToArgb();
            }
            set
            {
                Color = Color.FromArgb(value);
            }
        }

        /// <summary>
        /// Symbol
        /// </summary>
        internal SymbolBox Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void _NotifyPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region private members

        private bool _defaultValue;
        private int _size;
        private int _angle;
        private Color _color;
        private bool _useRouteColor;
        private SymbolBox _symbol;

        #endregion
    }
}
