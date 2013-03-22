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
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace EditCommonPropertiesPlugin
{
    // Add Task to Routes View.
    [CommandPlugIn(new string[1] { "RouteRoutingCommands" })]
    public class EditCommonStopPropertiesCmd : AppCommands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
            if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Stop" == selector.SelectedItems[0].GetType().ToString())
            {
                foreach (ESRI.ArcLogistics.DomainObjects.Stop s in selector.SelectedItems)
                {
                    if (s.IsLocked == true)
                        s.IsLocked = false;
                    else
                        s.IsLocked = true;
                }
            }

        }

        public void Initialize(App app)
        {
            m_application = app;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);
        }

        void Current_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content.ToString() == "ESRI.ArcLogistics.App.Pages.OptimizeAndEditPage" && !initialized)
            {
                // Subscribe to the page's selection changed event
                ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
                ((INotifyCollectionChanged)selector.SelectedItems).CollectionChanged += new NotifyCollectionChangedEventHandler(selected_CollectionChanged);
                initialized = true;
            }
        }

        private void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.Navigated += new System.Windows.Navigation.NavigatedEventHandler(Current_Navigated);
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;

                // Notify about property change.
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
            }
        }

        public KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "EditCommonPropertiesPlugin.EditCommonStopPropertiescmd"; }
        }

        public string Title
        {
            get { return "Lock/Unlock Selected"; }
        }

        public string TooltipText
        {
            get { return "Lock/Unlock all selected rows at once"; }
        }

        #endregion

        private void selected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (App.Current.MainWindow.CurrentPage.ToString() == "ESRI.ArcLogistics.App.Pages.OptimizeAndEditPage")
            {
                ISupportSelection selector = (ISupportSelection)App.Current.MainWindow.CurrentPage;
                if (selector.SelectedItems.Count > 0 && "ESRI.ArcLogistics.DomainObjects.Stop" == selector.SelectedItems[0].GetType().ToString())
                {
                    Boolean isLocked = false;
                    Boolean isUnlocked = false;

                    foreach(ESRI.ArcLogistics.DomainObjects.Stop s in selector.SelectedItems)
                    {
                        if (s.IsLocked == true)
                            isLocked = true;
                        else
                            isUnlocked = true;
                    }

                    if ((isLocked && !isUnlocked) || (!isLocked && isUnlocked))
                        IsEnabled = true;
                    else
                        IsEnabled = false;
                }
                else
                    IsEnabled = false;
            }
            else
                IsEnabled = false;
        }

        App m_application = null;
       
        Boolean _isEnabled = false;
        Boolean initialized = false;
        public event PropertyChangedEventHandler PropertyChanged;


    }
}
