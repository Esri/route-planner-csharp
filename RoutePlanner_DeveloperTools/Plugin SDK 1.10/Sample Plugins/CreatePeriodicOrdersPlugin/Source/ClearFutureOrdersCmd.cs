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
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;


using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace CreatePeriodicOrdersPlugin
{
    [CommandPlugIn(new string[1] { "PeriodicOrdersTaskWidgetCommands" })]
    public class ClearFutureOrdersCmd : AppCommands.ICommand
    {
        #region ICommand Members

        public void Execute(params object[] args)
        {
            bool success = true;
            string statusMessage = "";

            List<DateTime> days = m_application.Project.Orders.SearchDaysWithOrders(m_application.CurrentDate, m_application.CurrentDate.AddDays(PeriodicOrdersPage.numDays -1)).ToList();

            foreach (DateTime day in days)
            {

                List<Order> daysOrders = m_application.Project.Orders.Search(day).ToList();
                foreach (Order O in daysOrders)
                {
                    try
                    {
                        m_application.Project.Orders.Remove(O);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                }
            }

            if (success)
		{
		    statusMessage = "Deleted all orders between " + m_application.CurrentDate.ToShortDateString() + " and " + m_application.CurrentDate.AddDays(PeriodicOrdersPage.numDays - 1).ToShortDateString();
		    m_application.Messenger.AddInfo(statusMessage);
		}
		else
		{
		    statusMessage = "Deleted orders between " + m_application.CurrentDate.ToShortDateString() + " and " + m_application.CurrentDate.AddDays(PeriodicOrdersPage.numDays - 1).ToShortDateString();
		    m_application.Messenger.AddInfo(statusMessage);
		    statusMessage = "Unable to delete some orders because they are/were assigned to routes. Please delete them manually.";
		    m_application.Messenger.AddWarning(statusMessage);
		}
            m_application.Project.Save();
        }

        public void Initialize(App app)
        {
            m_application = app;
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "CreatePeriodicOrdersPlugin.ClearFutureOrdersCmd"; }
        }

        public string Title
        {
            get { return "Delete Future Orders"; }
        }

        public string TooltipText
        {
            get { return "Delete all future orders for the selected period"; }
        }

        public App m_application = null;

        #endregion
    }
}