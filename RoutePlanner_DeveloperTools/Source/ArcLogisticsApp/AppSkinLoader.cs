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
