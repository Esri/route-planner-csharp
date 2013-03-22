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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml - shows info about ArcLogistics third-party controls
    /// </summary>
    internal partial class AboutDialog : Window
    {
        #region Constructors

        public AboutDialog()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AboutDialog_Loaded);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method loads all icon sizes from icon and sets icon with max size to image
        /// </summary>
        private void _SetMaxSizeIconToImage()
        {
            Uri iconUri = new Uri(ICON_PATH);

            // load icon
            StreamResourceInfo iconInfo = Application.GetResourceStream(iconUri);
            Stream iconStream = iconInfo.Stream;
            IconBitmapDecoder iconBitmapDecoder = new IconBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

            // if icon file contains more than 1 size - find frame with max size
            if (iconBitmapDecoder.Frames.Count > 1)
            {
                double maxHeight = 0;
                double maxWidth = 0;
                BitmapFrame sourceFrame = default(BitmapFrame);

                foreach (BitmapFrame bitmapFrame in iconBitmapDecoder.Frames)
                {
                    if (bitmapFrame.Height > maxHeight && bitmapFrame.Width > maxWidth)
                    {
                        sourceFrame = bitmapFrame; // save found frame in sourceFrame
                        maxHeight = bitmapFrame.Height;
                        maxWidth = bitmapFrame.Width;
                    }
                }

                image.Source = sourceFrame; // set max size frame as image source
            }
        }

        /// <summary>
        /// Sets version to the version text block on the window.
        /// </summary>
        private void _SetVersion()
        {
            Assembly appAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = appAssembly.GetName();
            versionTextBlock.Text = assemblyName.Version.ToString();
        }

        #endregion

        #region Event Handlers

        private void AboutDialog_Loaded(object sender, RoutedEventArgs e)
        {
            _SetMaxSizeIconToImage();
            _SetVersion();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Private Fields

        private const string ICON_PATH = @"pack://application:,,/Resources/ArcLogistics.ico";

        #endregion
    }
}
