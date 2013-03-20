using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    internal class RelationObjectCollectionEditor<T> : RelationObjectCollectionEditorBase
        where T : DataObject
    {
        protected override void _AddSelectedItem(object item)
        {
            ICollection<T> collection = SelectedItems as ICollection<T>;

            Debug.Assert(collection != null); // collection can't be null

                collection.Add(item as T);
        }

        protected override void _RemoveSelectedItem(object item)
        {
            ICollection<T> collection = SelectedItems as ICollection<T>;
            
            Debug.Assert(collection != null); // collection can't be null
            
            foreach (T elem in collection)
            {
                if (elem.ToString().Equals(item.ToString()))
                {
                    collection.Remove(elem);
                    break;
                }
            }
        }

        protected override int _GetCollectionSize()
        {
            int count = 0;
            ICollection<T> collection = SelectedItems as ICollection<T>;
            if (collection != null)
                count = collection.Count;
            return count;
        }

        protected override bool _CheckAllContent(object item)
        {
            bool isOK = false;

            ICollection<T> collection = AllItems as ICollection<T>;

            Debug.Assert(collection != null); // collection can't be null

            foreach (T elem in collection)
            {
                if (elem.ToString().Equals(item.ToString()))
                {
                    isOK = true;
                    break;
                }
            }
            return isOK;
        }

        protected override bool _CheckSelectedContent(object item)
        {
            bool isOK = false;
            ICollection<T> collection = SelectedItems as ICollection<T>;

            Debug.Assert(collection != null); // collection can't be null

            foreach (T elem in collection)
            {
                if (elem.ToString().Equals(item.ToString()))
                {
                    isOK = true;
                    break;
                }
            }
            return isOK;
        }

        protected override object _GetIndexItem(int index)
        {
            object item = null;
            ICollection<T> collection = SelectedItems as ICollection<T>;

            Debug.Assert(collection != null); // collection can't be null

            List<T> list = collection.ToList();
            item = list[index];
            return item;
        }

        protected override string _InitText()
        {
            string names = string.Empty;

            ICollection<T> collection = SelectedItems as ICollection<T>;

            Debug.Assert(collection != null); // collection can't be null

            List<T> list = collection.ToList();

            if (null != collection)
            {
                StringBuilder sb = new StringBuilder();
                foreach (T elem in collection)
                {
                    if (0 < sb.Length)
                        sb.Append(", ");

                    sb.Append(elem.ToString());
                }

                names = sb.ToString();
            }
            return names;
        }
    }

    #region Template specializations
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal class RelationObjectCollectionLocationsEditor : RelationObjectCollectionEditor<Location>
    {
    }

    internal class RelationObjectCollectionDriverSpecialtiesEditor : RelationObjectCollectionEditor<DriverSpecialty>
    {
    }

    internal class RelationObjectCollectionVehicleSpecialtiesEditor : RelationObjectCollectionEditor<VehicleSpecialty>
    {
    }

    internal class RelationObjectCollectionZonesEditor : RelationObjectCollectionEditor<Zone>
    {
    }
    #endregion Template specializations
}