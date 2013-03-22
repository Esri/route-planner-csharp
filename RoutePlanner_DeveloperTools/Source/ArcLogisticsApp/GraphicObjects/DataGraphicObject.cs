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
using ESRI.ArcGIS.Client;
using System.Diagnostics;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.Mapping;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Class for graphics, associated with data object.
    /// </summary>
    internal abstract class DataGraphicObject : Graphic
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">Associated data object.</param>
        protected DataGraphicObject(object data)
        {
            _data = data;
            Attributes.Add(DATA_KEY_NAME, data);
        }

        #endregion

        #region Public static properties

        /// <summary>
        /// Data attribute name.
        /// </summary>
        public static string DataKeyName
        {
            get
            {
                return DATA_KEY_NAME;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Overrided geometry property. Used because of visibility supported.
        /// </summary>
        public new ESRI.ArcGIS.Client.Geometry.Geometry Geometry
        {
            get
            {
                if (_isVisible)
                {
                    return base.Geometry;
                }
                else
                {
                    return _tempGeometry;
                }
            }
            set
            {
                if (_isVisible)
                {
                    base.Geometry = value;
                }
                else
                {
                    _tempGeometry = value;
                }
            }
        }

        /// <summary>
        /// Source location for this graphic object
        /// </summary>
        public object Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Object, depending on whose properties graphics changes their view 
        /// </summary>
        public virtual object ObjectContext
        {
            get
            {
                Debug.Assert(false);
                return null;
            }
            set
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Layer-parent of graphic
        /// </summary>
        public ObjectLayer ParentLayer
        {
            get
            {
                return _parentLayer;
            }
            set
            {
                Debug.Assert(_parentLayer == null);

                _parentLayer = value;

                ProjectGeometry();
            }
        }

        /// <summary>
        /// Is barrier visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (value != _isVisible)
                {
                    if (value)
                    {
                        if (base.Geometry == null)
                            base.Geometry = _tempGeometry;
                    }
                    else
                    {
                        if (base.Geometry != null)
                            _tempGeometry = base.Geometry;
                        base.Geometry = null;
                    }

                    _isVisible = value;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public abstract void UnsubscribeOnChange();

        /// <summary>
        /// Project geometry to map spatial reference
        /// </summary>
        public abstract void ProjectGeometry();

        #endregion

        #region Constants

        /// <summary>
        /// Data attribute name.
        /// </summary>
        private const string DATA_KEY_NAME = "Data";

        #endregion

        #region Private members

        /// <summary>
        /// Associated data object.
        /// </summary>
        private object _data;

        /// <summary>
        /// Parent layer.
        /// </summary>
        private ObjectLayer _parentLayer;

        /// <summary>
        /// Stored geometry for hided objects.
        /// </summary>
        private ESRI.ArcGIS.Client.Geometry.Geometry _tempGeometry;

        /// <summary>
        /// Is graphic visible.
        /// </summary>
        private bool _isVisible = true;

        #endregion
    }
}
