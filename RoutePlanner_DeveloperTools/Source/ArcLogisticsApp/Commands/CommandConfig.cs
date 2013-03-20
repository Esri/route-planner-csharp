using System.ComponentModel;
using System.Windows.Input;
using System.Xml.Serialization;

// APIREV: rename to ESRI.ArcLogistics.App.Commands.Serialization
// APIREV: use DataContractAttribute and DataContractSerializer and make these classes internal
namespace ESRI.ArcLogistics.App.Commands
{
    // Classes that used to read information about categories and commands from commands.xml.
    [XmlRoot("Categories")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CategoriesInfo
    {
        [XmlElement("Category")]
        public CategoryInfo[] Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        private CategoryInfo[] _categories;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CategoryInfo
    {
        [XmlAttribute("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [XmlAttribute("pagetype")]
        public string PageType
        {
            get { return _pageType; }
            set { _pageType = value; }
        }

        [XmlElement("Command")]
        public CommandInfo[] Commands
        {
            get { return _commands; }
            set { _commands = value; }
        }

        private string _name;
        private string _pageType;
        private CommandInfo[] _commands;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CommandInfo
    {
        [XmlAttribute("type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _type;
    }
}
