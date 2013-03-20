using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// LinkType enum
    /// </summary>
    public enum LinkType
    {
        /// <summary>
        /// Link call HelpLinkCommand
        /// </summary>
        Url,

        /// <summary>
        /// Link call NavigationCommandSimple
        /// </summary>
        Page
    }

    /// <summary>
    /// MessageType enum
    /// </summary>
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }

    /// <summary>
    /// Link class
    /// </summary>
    public class Link
    {
        public Link(string text, string link, LinkType type)
        {
            _text = text;
            _link = link;
            _type = type;
        }

        public string Text
        {
            get { return _text; }
        }

        public string LinkRef
        {
            get { return _link; }
        }

        internal LinkType Type
        {
            get { return _type; }
        }

        private readonly string _text;
        private readonly string _link;
        private readonly LinkType _type = LinkType.Url;
    }

    /// <summary>
    /// IMessenger interface
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Add message to output window
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message text</param>
        void AddMessage(MessageType type, string message);
        /// <summary>
        /// Add message to output window
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message text. Can contain sequence '{0}' for  designation of position of link.
        ///     Without instructions of place the link will be added after the message text at the end</param>
        /// <param name="link">Link description</param>
        void AddMessage(MessageType type, string message, Link link);
        /// <summary>
        /// Add message to output window with details
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        void AddMessage(MessageType type, string message, IEnumerable<MessageDetail> details);
        /// <summary>
        /// Add message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        /// <remarks>Type selected automatically by details type (set as high priority)</remarks>
        void AddMessage(string message, IEnumerable<MessageDetail> details);

        /// <summary>
        /// Add error message to output window
        /// </summary>
        /// <param name="message">Message text</param>
        void AddError(string message);
        /// <summary>
        /// Add error message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        void AddError(string message, IEnumerable<MessageDetail> details);

        /// <summary>
        /// Add warning message to output window
        /// </summary>
        /// <param name="message">Message text</param>
        void AddWarning(string message);
        /// <summary>
        /// Add warning message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        void AddWarning(string message, IEnumerable<MessageDetail> details);

        /// <summary>
        /// Add information message to output window
        /// </summary>
        /// <param name="message">Message text</param>
        void AddInfo(string message);
        /// <summary>
        /// Add information message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        void AddInfo(string message, IEnumerable<MessageDetail> details);

        /// <summary>
        /// Remove all messages
        /// </summary>
        void Clear();
    }
}
