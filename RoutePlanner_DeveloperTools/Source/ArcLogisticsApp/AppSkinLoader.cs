using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App
{
    internal class AppSkinLoader : Control
    {
        public AppSkinLoader()
        { }
        
        /// <summary>
        /// applays skin & raise apply skin event
        /// </summary>
        public void ApplySkin()
        {
            if (!string.IsNullOrEmpty(SkinName))
            {
                _SkinSource = string.Format("pack://application:,,,/ESRI.ArcLogistics.App;Component/Skins/{0}.xaml", SkinName);
                Uri uri = new Uri(_SkinSource, UriKind.Absolute);
                ResourceDictionary skin = new ResourceDictionary();
                skin.Source = uri;
                Application.Current.Resources.Remove(skin);
                Application.Current.Resources.MergedDictionaries.Add(skin);
                this.RaiseEvent(new RoutedEventArgs(AppSkinLoader.SkinChangeEvent));
            }
        }

        /// <summary>
        /// adds/removes routed event handler for click event 
        /// </summary>
        public event RoutedEventHandler SkinChange
        {
            add { AddHandler(AppSkinLoader.SkinChangeEvent, value); }
            remove { RemoveHandler(AppSkinLoader.SkinChangeEvent, value); }
        }

        /// <summary>
        /// gets/sets skin name string
        /// </summary>
        public string SkinName
        {
            get { return _SkinName; }
            set { _SkinName = value; }
        }

        /// <summary>
        /// skin name string
        /// </summary>
        private string _SkinName;
       
        /// <summary>
        /// skin source string
        /// </summary>
        private string _SkinSource;

        public static readonly RoutedEvent SkinChangeEvent = EventManager.RegisterRoutedEvent("SkinChange",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AppSkinLoader));

    }
}
