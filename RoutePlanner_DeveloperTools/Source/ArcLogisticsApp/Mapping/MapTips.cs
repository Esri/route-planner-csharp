using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for work with map tips.
    /// </summary>
    class MapTips
    {
        #region public members

        /// <summary>
        /// Init maptip for layer if maptip enabled.
        /// </summary>
        /// <param name="layer">Parent layer for maptip.</param>
        /// <param name="mapTip">Inited maptip.</param>
        internal void CreateMapTipIfNeeded(ObjectLayer layer, FrameworkElement mapTip)
        {
            if (layer.MapTipEnabled)
            {
                layer.MapLayer.MapTip = mapTip;
                ContentControl contentControl = (ContentControl)layer.MapLayer.MapTip;
                contentControl.IsVisibleChanged += new DependencyPropertyChangedEventHandler(_MapTipVisibilityChanged);
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Fill maptip and set visibility.
        /// </summary>
        /// <param name="sender">Control for showing tips.</param>
        /// <param name="e">Property changed args.</param>
        private void _MapTipVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ContentControl contentControl = (ContentControl)sender;
            IDictionary<string, object> dic = contentControl.DataContext as IDictionary<string, object>;
            if ((bool)e.NewValue)
            {
                Border grid = (Border)contentControl.Content;
                Border border = (Border)grid.Child;
                TextBlock tb = (TextBlock)((StackPanel)border.Child).Children[0];

                border.Visibility = Visibility.Collapsed;

                if (dic != null)
                {
                    if (dic.ContainsKey(DataGraphicObject.DataKeyName))
                    {
                        object data = dic[DataGraphicObject.DataKeyName];
                        TipGenerator.FillTipText(tb, data);
                        border.Visibility = Visibility.Visible;
                    }
                    else if (dic.ContainsKey(ALClusterer.COUNT_PROPERTY_NAME))
                    {
                        int count = (int)dic[ALClusterer.COUNT_PROPERTY_NAME];
                        if (count <= ALClusterer.CLUSTER_COUNT_TO_EXPAND)
                        {
                            TipGenerator.FillClusterToolText(tb, dic);
                            border.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // Do nothing.
                        }
                    }
                    else
                    {
                        // Do nothing.
                    }
                }
                else
                {
                    // Do nothing.
                }
            }
        }

        #endregion
    }
}
