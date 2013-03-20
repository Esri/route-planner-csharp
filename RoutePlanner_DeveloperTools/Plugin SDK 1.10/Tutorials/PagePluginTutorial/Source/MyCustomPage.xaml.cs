using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Help;

namespace PagePluginTutorial
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [PagePlugInAttribute("Schedule")]
    public partial class MyCustomPage : PageBase
    {
        public MyCustomPage()
        {
            _helpTopic = new HelpTopic(null, "This is the custom QuickHelp for My Custom Page");
            IsAllowed = true;
            IsRequired = true;
            InitializeComponent();
        }

        public override ESRI.ArcLogistics.App.Help.HelpTopic HelpTopic
        {
            get { return _helpTopic; }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get { return null; }
        }

        public override string Name
        {
            get { return "PagePluginTutorial.MyCustomPage"; }
        }

        public override string Title
        {
            get { return "My Custom Page"; }
        }

        private HelpTopic _helpTopic;
    }
}
