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
using ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for WhatNewPage.xaml
    /// </summary>
    public partial class WhatNewPage : PageBase
    {
        #region Constructors

        public WhatNewPage()
        {
            InitializeComponent();
            IsRequired = false;
            IsAllowed = true;
            this.Loaded += new RoutedEventHandler(WhatNewPage_Loaded);
        }

        void WhatNewPage_Loaded(object sender, RoutedEventArgs e)
        {
            // set void status bar content
            ((MainWindow)App.Current.MainWindow).StatusBar.SetStatus(this, "");
        }

        #endregion

        public override void SaveLayout()
        {
            
        }

        #region PageBase overrided members

        public override string QuickHelpText
        {
            get { return (string)App.Current.FindResource("WhatNewHelpString"); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        public override ReadOnlyCollection<NextStep> NextSteps
        {
            get
            {
                List<NextStep> nextSteps = new List<NextStep>();
                nextSteps.Add(new NextStep((string)App.Current.FindResource("LicenseCaption"), "Home\\License"));
                nextSteps.Add(new NextStep((string)App.Current.FindResource("SetupStepCaption"), "Setup\\Locations"));

                return nextSteps.AsReadOnly();
            }
        }

        #endregion

     }
}
