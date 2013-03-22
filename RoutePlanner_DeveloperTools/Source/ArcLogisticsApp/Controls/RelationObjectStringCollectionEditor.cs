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
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections;

namespace ESRI.ArcLogistics.App.Controls
{
    internal class RelationObjectCollectionEditor : RelationObjectCollectionEditorBase
    {
        protected override void _AddSelectedItem(object item)
        {
            IList collection = SelectedItems as IList;
            if (collection != null)
                collection.Add(item);
        }

        protected override void _RemoveSelectedItem(object item)
        {
            IList collection = SelectedItems as IList;
            if (collection != null)
            {
                foreach (object elem in collection)
                {
                    if (elem.ToString().Equals(item.ToString()))
                    {
                        collection.Remove(elem);
                        break;
                    }
                }
            }
        }

        protected override int _GetCollectionSize()
        {
            int count = 0;
            IList collection = SelectedItems as IList;
            if (collection != null)
                count = collection.Count;

            return count;
        }

        protected override bool _CheckAllContent(object item)
        {
            bool isOK = false;
            
            //ICollection<object> collection = AllItems as ICollection<object>;
            if (AllItems != null)
            {
                foreach (object elem in AllItems)
                {
                    if (elem.ToString() == item.ToString())
                    {
                        isOK = true;
                        break;
                    }
                }
            }
            return isOK;
        }

        protected override bool _CheckSelectedContent(object item)
        {
            bool isOK = false;
            IList collection = SelectedItems as IList;
            if (collection != null)
            {
                foreach (object elem in collection)
                {
                    if (elem.ToString() == item.ToString())
                    {
                        isOK = true;
                        break;
                    }
                }
            }

            return isOK;
        }

        protected override object _GetIndexItem(int index)
        {
            object item = null;
            IList collection = SelectedItems as IList;
            if (collection != null)
            {
                item = collection[index];
            }
            return item;
        }

        protected override string _InitText()
        {
            string names = string.Empty;

            IList collection = SelectedItems as IList;
            if (collection != null)
            {
                if (null != collection)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (object elem in collection)
                    {
                        if (0 < sb.Length)
                            sb.Append(", ");

                        sb.Append(elem.ToString());
                    }

                    names = sb.ToString();
                }
            }

            return names;
        }
    }

    #region Template specializations
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal class RelationObjectCollectionStringEditor : RelationObjectCollectionEditor
    {
    }
    #endregion Template specializations
}