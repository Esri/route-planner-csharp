using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// An extended message box with lot of customizing capabilities.
    /// </summary>
    internal class MessageBoxEx
    {
        #region Public static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxText">Text to display.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        public static MessageBoxExButtonType Show(Window owner, string messageBoxText, string caption,
                                                  MessageBoxButtons button, MessageBoxImage icon)
        {
            bool checkBoxState = false; // NOTE: ignored
            MessageBoxEx msbBox = new MessageBoxEx();
            return msbBox._Show(owner, messageBoxText, caption, button, icon, null, ref checkBoxState);
        }

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxText">Text to display.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <param name="checkBoxText">Response check box caption.</param>
        /// <param name="checkBoxState">Response check box default\return state.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        public static MessageBoxExButtonType Show(Window owner, string messageBoxText, string caption,
                                                  MessageBoxButtons button, MessageBoxImage icon,
                                                  string checkBoxText, ref bool checkBoxState)
        {
            MessageBoxEx msbBox = new MessageBoxEx();
            return msbBox._Show(owner, messageBoxText, caption, button, icon, checkBoxText, ref checkBoxState);
        }

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxTextFormat">Text format to display.</param>
        /// <param name="links">List of links on text.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        public static MessageBoxExButtonType Show(Window owner, string messageBoxTextFormat,
                                                  IList<Hyperlink> links, string caption,
                                                  MessageBoxButtons button, MessageBoxImage icon)
        {
            bool checkBoxState = false; // NOTE: ignored
            MessageBoxEx msbBox = new MessageBoxEx();
            return msbBox._Show(owner, messageBoxTextFormat, links, caption, button, icon,
                                null, ref checkBoxState);
        }

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxTextFormat">Format for text to display.</param>
        /// <param name="links">Link list to inject in to message.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <param name="checkBoxText">Response check box caption.</param>
        /// <param name="checkBoxState">Response check box default\return state.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        public static MessageBoxExButtonType Show(Window owner, string messageBoxTextFormat,
                                                  IList<Hyperlink> links, string caption,
                                                  MessageBoxButtons button, MessageBoxImage icon,
                                                  string checkBoxText, ref bool checkBoxState)
        {
            MessageBoxEx msbBox = new MessageBoxEx();
            return msbBox._Show(owner, messageBoxTextFormat, links, caption, button, icon,
                                checkBoxText, ref checkBoxState);
        }

        #endregion // Public static methods

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MessageBoxEx()
        {
        }

        #endregion // Constructors

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates message content composition.
        /// </summary>
        /// <param name="messageTextFormat">Message format.</param>
        /// <param name="links">Inserted list of links.</param>
        /// <returns>Content composition.</returns>
        private IList<Inline> _CreateMessageText(string messageTextFormat, IList<Hyperlink> links)
        {
            List<Inline> inlines = new List<Inline>();

            MatchCollection mc = Regex.Matches(messageTextFormat, @"({\d+})");
            if ((0 == mc.Count) || (null == links))
                inlines.Add(new Run(messageTextFormat));
            else
            {
                int index = 0;
                for (int i = 0; i < mc.Count; ++i)
                {
                    // add text before link
                    string stringObj = mc[i].Value;
                    int startIndex = messageTextFormat.IndexOf(stringObj, index);
                    if (0 < startIndex)
                        inlines.Add(new Run(messageTextFormat.Substring(index, startIndex - index)));
                    index = startIndex + stringObj.Length;

                    // add link
                    MatchCollection mcNum = Regex.Matches(stringObj, @"(\d+)");
                    if (1 == mcNum.Count)
                    {
                        int objNum = Int32.Parse(mcNum[0].Value);
                        if (objNum < links.Count)
                            inlines.Add(links[objNum]);
                    }
                }

                // add text after all links
                if (index < messageTextFormat.Length)
                    inlines.Add(new Run(messageTextFormat.Substring(index, messageTextFormat.Length - index)));
            }

            return inlines;
        }


        /// <summary>
        /// Add standard buttons to the message box.
        /// </summary>
        /// <param name="buttons">The standard buttons to add.</param>
        private void _AddButtons(MessageBoxButtons buttons)
        {
            switch(buttons)
            {
                case MessageBoxButtons.OK:
                    _AddButton(MessageBoxExButtonType.Ok, true);
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    _AddButton(MessageBoxExButtonType.Abort, false);
                    _AddButton(MessageBoxExButtonType.Retry, false);
                    _AddButton(MessageBoxExButtonType.Ignore, true);
                    break;

                case MessageBoxButtons.OKCancel:
                    _AddButton(MessageBoxExButtonType.Ok, false);
                    _AddButton(MessageBoxExButtonType.Cancel, true);
                    break;

                case MessageBoxButtons.RetryCancel:
                    _AddButton(MessageBoxExButtonType.Retry, false);
                    _AddButton(MessageBoxExButtonType.Cancel, true);
                    break;

                case MessageBoxButtons.YesNo:
                    _AddButton(MessageBoxExButtonType.Yes, false);
                    _AddButton(MessageBoxExButtonType.No, true);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    _AddButton(MessageBoxExButtonType.Yes, false);
                    _AddButton(MessageBoxExButtonType.No, false);
                    _AddButton(MessageBoxExButtonType.Cancel, true);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
        }

        /// <summary>
        /// Add a standard button to the message box.
        /// </summary>
        /// <param name="button">The standard button to add.</param>
        /// <param name="isCancelResult">The return value for this button in case if dialog 
        /// closes without pressing any button.</param>
        private void _AddButton(MessageBoxExButtonType buttonType, bool isCancelResult)
        {
            // The text of the button.
            string caption = _GetButtonCaption(buttonType);

            // Text must be not null.
            Debug.Assert(!string.IsNullOrEmpty(caption));

            // Create button.
            MessageBoxExButton button = new MessageBoxExButton();
            button.Caption = caption;

            // The return value in case this button is clicked.
            button.Value = buttonType;

            if (isCancelResult)
                button.IsCancelButton = true;

            // Add a custom button to the message box.
            _msgBox.Buttons.Add(button);
        }

        /// <summary>
        /// Gets button localized caption by button type.
        /// </summary>
        /// <param name="button">Button type.</param>
        /// <returns>Caption text from resources.</returns>
        private string _GetButtonCaption(MessageBoxExButtonType button)
        {
            string resourceName = null;
            switch(button)
            {
                case MessageBoxExButtonType.Ok:
                    resourceName = "ButtonHeaderOk";
                    break;

                case MessageBoxExButtonType.Cancel:
                    resourceName = "ButtonHeaderCancel";
                    break;

                case MessageBoxExButtonType.Yes:
                    resourceName = "ButtonHeaderYes";
                    break;

                case MessageBoxExButtonType.No:
                    resourceName = "ButtonHeaderNo";
                    break;

                case MessageBoxExButtonType.Abort:
                    resourceName = "ButtonHeaderAbort";
                    break;

                case MessageBoxExButtonType.Retry:
                    resourceName = "ButtonHeaderRetry";
                    break;

                case MessageBoxExButtonType.Ignore:
                    resourceName = "ButtonHeaderIgnore";
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            Debug.Assert(null != resourceName);
            return (string)App.Current.FindResource(resourceName);
        }

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxTextComposition">Text composition to display.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <param name="checkBoxText">Response check box caption.</param>
        /// <param name="checkBoxState">Response check box default\return state.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        private MessageBoxExButtonType _Show(Window owner, IList<Inline> messageBoxTextComposition, string caption,
                                             MessageBoxButtons button, MessageBoxImage icon,
                                             string checkBoxText, ref bool checkBoxState)
        {
            // initialize components of message box
            _msgBox = new MessageBoxExDlg();
            _msgBox.Caption = caption;
            _msgBox.StandardIcon = icon;

            _msgBox._textBlockQuestion.Inlines.Clear();
            foreach (Inline inline in messageBoxTextComposition)
                _msgBox._textBlockQuestion.Inlines.Add(inline);

            if (string.IsNullOrEmpty(checkBoxText))
                _msgBox.ResponseText = null;
            else
            {
                _msgBox.ResponseText = checkBoxText;
                _msgBox.Response = checkBoxState;
            }

            _AddButtons(button);

            if (null != owner)
                _msgBox.Owner = owner;

            using (MouseHelper.OverrideCursor(null))
            {
                // populate dialog
                _msgBox.ShowDialog(); // NOTE: ignore result
            }

            // get results
            if (!string.IsNullOrEmpty(checkBoxText))
                checkBoxState = _msgBox.Response;
            return _msgBox.Result;
        }

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxTextFormat">Text format to display.</param>
        /// <param name="links">List of links on text.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <param name="checkBoxText">Response check box caption.</param>
        /// <param name="checkBoxState">Response check box default\return state.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        public MessageBoxExButtonType _Show(Window owner, string messageBoxTextFormat,
                                            IList<Hyperlink> links, string caption,
                                            MessageBoxButtons button, MessageBoxImage icon,
                                            string checkBoxText, ref bool checkBoxState)
        {
            IList<Inline> messageBoxTextComposition = _CreateMessageText(messageBoxTextFormat, links);
            return _Show(owner, messageBoxTextComposition, caption, button, icon, checkBoxText,
                         ref checkBoxState);
        }

        /// <summary>
        /// Displays a customized message box in front of the specified window.
        /// </summary>
        /// <param name="owner">Owner window of the message box.</param>
        /// <param name="messageBoxText">Text to display.</param>
        /// <param name="caption">Title bar caption to display.</param>
        /// <param name="button">A value that specifies which button or buttons to display.</param>
        /// <param name="icon">Icon to display.</param>
        /// <param name="checkBoxText">Response check box caption.</param>
        /// <param name="checkBoxState">Response check box default\return state.</param>
        /// <returns>Button value which message box is clicked by the user.</returns>
        private MessageBoxExButtonType _Show(Window owner, string messageBoxText, string caption,
                                             MessageBoxButtons button, MessageBoxImage icon,
                                             string checkBoxText, ref bool checkBoxState)
        {
            IList<Inline> messageBoxTextComposition = _CreateMessageText(messageBoxText, null);
            return _Show(owner, messageBoxTextComposition, caption, button, icon, checkBoxText,
                         ref checkBoxState);
        }

        #endregion // Private methods

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Customized message box instance.
        /// </summary>
        private MessageBoxExDlg _msgBox = null;

        #endregion // Private fields
    }
}
