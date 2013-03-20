using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using System.Collections;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Controls
{
    internal class ComboBoxWithNullRow : ComboBoxBasedEditor
    {
        static ComboBoxWithNullRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxWithNullRow), new FrameworkPropertyMetadata(typeof(ComboBoxWithNullRow)));
        }

        #region Protected Override Methods

        /// <summary>
        /// Overrided method insert null item to the top of items list
        /// </summary>
        protected override void _InsertNullItem()
        {
            string nullItem = string.Empty;

            if (AvailableCollection.Count == 0 || (AvailableCollection[0] != null && AvailableCollection[0] != nullItem))
                AvailableCollection.Insert(0, nullItem);
        }

        #endregion
    }
}
