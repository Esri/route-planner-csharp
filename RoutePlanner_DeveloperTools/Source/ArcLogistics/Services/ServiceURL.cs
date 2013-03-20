using System.ComponentModel;
using System.Xml.Serialization;

namespace ESRI.ArcLogistics.Services.Serialization
{
    /// <summary>
    /// Class for storing URL where configuration file can be found.
    /// </summary>
    [XmlRoot("ConfigurationFile")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ServiceURL
    {
        [XmlAttribute("URL")]
        /// <summary>
        /// URL to configuration file.
        /// </summary>
        public string URL { get; set; }
    }
}
