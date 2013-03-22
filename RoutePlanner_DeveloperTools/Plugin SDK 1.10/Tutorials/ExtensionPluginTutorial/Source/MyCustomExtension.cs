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
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.Routing;

namespace ExtensionPluginTutorial
{
    public class MyCustomExtension : IExtension
    {
        public string Description
        {
            get { return "This extensions shows an alert when a Build Routes operation is completed."; }
        }

        public void Initialize(App app)
        {
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);            
        }

        public string Name
        {
            get { return "ExtensionPluginTutorial.MyCustomExtension"; }
        }

        private void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Solver_AsyncSolveCompleted);
        }

        private void Solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            App.Current.Messenger.AddInfo("Solve completed.");
        }
    }
}
