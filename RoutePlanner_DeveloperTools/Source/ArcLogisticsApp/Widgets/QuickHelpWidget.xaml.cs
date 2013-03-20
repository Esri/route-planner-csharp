using System;
using System.Windows.Documents;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// Quick help widget.
    /// </summary>
    internal partial class QuickHelpWidget : PageWidgetBase
    {
        #region constructors 

        public QuickHelpWidget()
        {
            InitializeComponent();
        }

        #endregion

        #region Public properties and methods

        public override void Initialize(ESRI.ArcLogistics.App.Pages.Page page)
        {
            base.Initialize(page);
            _InitQuickHelpText();
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("QuickHelpWidgetCaption"); }
        }

        public bool ShowTopic()
        {
            bool result = false;
            if (null != _command)
            {
                _command.Execute(null);
                result = true;
            }

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initialize control with help text.
        /// </summary>
        private void _InitQuickHelpText()
        {
            HelpTopic topic = ((PageBase)this._Page).HelpTopic;
            if (null != topic)
            {
                QuickHelpText.Inlines.Clear();

                List<Inline> inlines = new List<Inline>();
                if (!string.IsNullOrEmpty(topic.QuickHelpText))
                    QuickHelpText.Inlines.Add(topic.QuickHelpText.Trim());

                if (!string.IsNullOrEmpty(topic.Key) || !string.IsNullOrEmpty(topic.Path))
                {
                    if (!string.IsNullOrEmpty(topic.QuickHelpText))
                        QuickHelpText.Inlines.Add(" ");

                    Hyperlink helpLink = new Hyperlink(new Run((string)App.Current.FindResource("QuickHelpLinkCaption")));
                    _command = new HelpLinkCommand(topic.Path, topic.Key);
                    helpLink.Command = _command;
                    QuickHelpText.Inlines.Add(helpLink);
                }
            }
        }

        #endregion

        #region Private members

        private HelpLinkCommand _command = null;

        #endregion
    }
}
