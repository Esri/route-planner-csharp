using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command that navigates to Setup->WhatsNew page.
    /// </summary>
    class WhatsNewCmd : NavigateToPageCmd
    {
        public const string COMMAND_NAME = "ArcLogistics.Commands.WhatsNew";

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("WhatsNewCommandTitle"); }
        }

        public override string TooltipText
        {
            get { return ""; }
            protected set { }
        }

        protected override string _PagePath
        {
            get
            {
                return @"Setup\WhatNew";
            }
        }
    }
}
