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
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Mapping
{
    class EditingMarker
    {
        #region constructors

        public EditingMarker(int multipleIndex, object obj)
        {
            _multipleIndex = multipleIndex;
            _editingObject = obj;
        }

        #endregion 

        #region public members

        public int MultipleIndex
        {
            get { return _multipleIndex; }
        }

        #endregion public members

        #region private methods

        public object EditingObject
        {
            get { return _editingObject; }
        }

        #endregion

        #region private members

        private int _multipleIndex;
        private object _editingObject;

        #endregion private members
    }
}
