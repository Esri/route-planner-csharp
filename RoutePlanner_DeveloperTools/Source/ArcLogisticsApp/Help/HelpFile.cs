using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ESRI.ArcLogistics.App.Help
{
    /// <summary>
    /// Application help settings
    /// </summary>
    internal static class HelpFile
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize HelpFile instance from file
        /// </summary>
        public static HelpTopics Load(string fileName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName));

            if (!File.Exists(fileName))
                return null;

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            return _LoadContent(doc.DocumentElement);
        }

        #endregion // Public methods

        #region Private static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static void _LoadTopic(string path, XmlNode nodeTopic, Dictionary<string, HelpTopic> topics)
        {
            string name = nodeTopic.Attributes[ATTRIBUTE_NAME_NAME].Value;
            string key = nodeTopic.Attributes[ATTRIBUTE_NAME_KEY].Value;
            string quickHelp = null;
            foreach (XmlNode node in nodeTopic.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_QUICKHELP, StringComparison.OrdinalIgnoreCase))
                {
                    if (null != node.FirstChild)
                        quickHelp = node.FirstChild.Value;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(name) &&
                (!string.IsNullOrEmpty(path) || !string.IsNullOrEmpty(key) || !string.IsNullOrEmpty(quickHelp)))
            {
                HelpTopic topic = new HelpTopic(path, key, quickHelp);
                topics.Add(name, topic);
            }
        }

        private static HelpTopics _LoadContent(XmlElement nodeProfiles)
        {
            string path = null;
            LinkType type = LinkType.Html;
            Dictionary<string, HelpTopic> topics = new Dictionary<string,HelpTopic> ();
            try
            {
                foreach (XmlNode node in nodeProfiles.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue; // skip comments and other non element nodes

                    if (node.Name.Equals(NODE_NAME_SOURCE, StringComparison.OrdinalIgnoreCase))
                    {
                        type = (node.Attributes[ATTRIBUTE_NAME_TYPE].Value.Equals("Html", StringComparison.OrdinalIgnoreCase))?
                                LinkType.Html : LinkType.Chm;
                        path = node.Attributes[ATTRIBUTE_NAME_PATH].Value;
                        if (!string.IsNullOrEmpty(path))
                            if (!FileHelpers.IsAbsolutPath(path) && !CommonHelpers.IsSourceHttpLink(path))
                                path = Path.Combine(DataFolder.Path, path);
                    }
                    else if (node.Name.Equals(NODE_NAME_TOPICS, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlNode topicNode in node.ChildNodes)
                        {
                            if (topicNode.NodeType != XmlNodeType.Element)
                               continue; // skip comments and other non element nodes

                            if (topicNode.Name.Equals(NODE_NAME_TOPIC, StringComparison.OrdinalIgnoreCase))
                                _LoadTopic(path, topicNode, topics);
                        }
                    }
                    else
                        throw new NotSupportedException();
                }
            }
            catch(Exception e)
            {
                path = null;
                Logger.Error(e);
            }

            return new HelpTopics(type, path, topics);
        }

        #endregion // Private static methods

        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string NODE_NAME_SOURCE = "Source";
        private const string NODE_NAME_TOPICS = "Topics";
        private const string NODE_NAME_TOPIC = "Topic";
        private const string NODE_NAME_QUICKHELP = "QuickHelp";

        private const string ATTRIBUTE_NAME_TYPE = "Type";
        private const string ATTRIBUTE_NAME_PATH = "Path";
        private const string ATTRIBUTE_NAME_NAME = "Name";
        private const string ATTRIBUTE_NAME_KEY = "Key";

        #endregion // Constants
    }
}
