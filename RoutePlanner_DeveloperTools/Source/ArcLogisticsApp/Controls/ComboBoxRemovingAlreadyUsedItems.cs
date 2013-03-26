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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Combo box that removes vehicles or drivers that are already used in current schedule routes.
    /// </summary>
    internal class ComboBoxRemovingAlreadyUsedItems : ComboBoxBasedEditor
    {
        static ComboBoxRemovingAlreadyUsedItems()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxRemovingAlreadyUsedItems), new FrameworkPropertyMetadata(typeof(ComboBoxRemovingAlreadyUsedItems)));
        }

        #region Public Properties

        public static readonly DependencyProperty ItemTypeProperty =
           DependencyProperty.Register("ItemType", typeof(Type), typeof(ComboBoxRemovingAlreadyUsedItems));

        /// <summary>
        /// gets/sets type of items
        /// </summary>
        public Type ItemType
        {
            get { return (Type)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }

        #endregion

        #region Protected Override Methods

        protected override void _BuildAvailableCollection()
        {
            base._BuildAvailableCollection();

            Collection<ESRI.ArcLogistics.Data.DataObject> usedItems = new Collection<ESRI.ArcLogistics.Data.DataObject>();

            if (ItemType != null && ItemType.Equals(typeof(ArcLogistics.DomainObjects.Vehicle)))
                usedItems = RoutesHelper.CreateUsedVehiclesCollection();
            else if (ItemType != null && ItemType.Equals(typeof(ArcLogistics.DomainObjects.Driver)))
                usedItems = RoutesHelper.CreateUsedDriversCollection();

            foreach (ESRI.ArcLogistics.Data.DataObject obj in usedItems)
            {
                if (obj != SelectedItem)
                    AvailableCollection.Remove(obj);
            }
        }

        /// <summary>
        /// Overrided method is empty because it adds null row to the top of items list but this control don't need null row
        /// </summary>
        protected override void _InsertNullItem()
        {}

        #endregion
    }
}
