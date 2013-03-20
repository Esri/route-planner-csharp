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
