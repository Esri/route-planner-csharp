/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
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
