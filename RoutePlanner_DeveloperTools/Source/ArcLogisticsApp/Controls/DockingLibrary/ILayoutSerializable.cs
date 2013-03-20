using System.Xml;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Delegate used by serializer to get user defined dockable contents
    /// </summary>
    /// <param name="type">Type class</param>
    internal delegate DockableContent GetContentFromTypeString(string type);

    /// <summary>
    /// Interface layout serializable
    /// </summary>
    interface ILayoutSerializable
    {
        /// <summary>
        /// Serialize layout
        /// </summary>
        /// <param name="doc">Document to save</param>
        /// <param name="nodeParent">Parent node</param>
        void Serialize(XmlDocument doc, XmlNode nodeParent);
        /// <summary>
        /// Deserialize layout
        /// </summary>
        /// <param name="manager">Dock manager for initing objects</param>
        /// <param name="node">Node to parse</param>
        /// <param name="handlerObject">Delegate used to get user defined dockable contents</param>
        void Deserialize(DockManager manager, XmlNode node, GetContentFromTypeString handlerObject);
    }
}
