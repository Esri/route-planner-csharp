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
using System.Windows.Forms;


namespace EditCommonPropertiesPlugin
{
    public partial class EditCommonPropertiesPopup : Form
    {
        object _owner = null;

        public EditCommonPropertiesPopup(List<string> items, EditCommonOrderPropertiesCmd owner)
        {
            _owner = owner;
            initialize(items);
        }

        public EditCommonPropertiesPopup(List<string> items, EditCommonVehiclePropertiesCmd owner)
        {
            _owner = owner;
            initialize(items);
        }

        public EditCommonPropertiesPopup(List<string> items, EditCommonDriverPropertiesCmd owner)
        {
            _owner = owner;
            initialize(items);
        }

        public EditCommonPropertiesPopup(List<string> items, EditCommonDefaultRoutePropertiesCmd owner)
        {
            _owner = owner;
            initialize(items);
        }

        public EditCommonPropertiesPopup(List<string> items, EditCommonDailyRoutePropertiesCmd owner)
        {
            _owner = owner;
            initialize(items);
        }

        public void initialize(List<string> items)
        {
            InitializeComponent();
            items.Sort();
            foreach (string s in items)
                cbox.Items.Add(s);

            applyButton.Enabled = false;
            OKButton.Enabled = false;
        }


        private void makeEdit(string field, string value)
        {
            string ownerType = _owner.GetType().ToString();
            switch (ownerType)
            {
                case "EditCommonPropertiesPlugin.EditCommonOrderPropertiesCmd": EditCommonOrderPropertiesCmd.makeEdit(field, value); break;
                case "EditCommonPropertiesPlugin.EditCommonVehiclePropertiesCmd": EditCommonVehiclePropertiesCmd.makeEdit(field, value); break;
                case "EditCommonPropertiesPlugin.EditCommonDriverPropertiesCmd": EditCommonDriverPropertiesCmd.makeEdit(field, value); break;
                case "EditCommonPropertiesPlugin.EditCommonDefaultRoutePropertiesCmd": EditCommonDefaultRoutePropertiesCmd.makeEdit(field, value); break;
                case "EditCommonPropertiesPlugin.EditCommonDailyRoutePropertiesCmd": EditCommonDailyRoutePropertiesCmd.makeEdit(field, value); break;
            }
        }



        #region Form Event Handlers

        private void OKButton_Click_1(object sender, EventArgs e)
        {
            if (cbox.SelectedItem != null)
            {
                if(applyButton.Enabled==true)
                    makeEdit(cbox.SelectedItem.ToString(), textBox1.Text);
                this.Close();
            }
        }
        
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (cbox.SelectedItem != null)
            {
                makeEdit(cbox.SelectedItem.ToString(), textBox1.Text);
                applyButton.Enabled = false;
            }
        }

        private void cbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void cbox_TextChanged(object sender, EventArgs e)
        {
            if (cbox.Items.Contains(cbox.Text))
            {
                applyButton.Enabled = true;
                OKButton.Enabled = true;
            }
            else
            {
                applyButton.Enabled = false;
                OKButton.Enabled = false;
            }
        }

        #endregion Form Event Handlers 


    }
}
