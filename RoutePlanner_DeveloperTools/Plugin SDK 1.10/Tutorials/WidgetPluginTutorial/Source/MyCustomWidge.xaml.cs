using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.App;
using System.Xml;

namespace WidgetPluginTutorial
{
    [WidgetPlugIn(new string[1] { @"Schedule\OptimizeAndEdit" })]
    public partial class MyCustomWidge : PageWidget
    {
        public MyCustomWidge()
        {
            InitializeComponent();
        }
        public override void Initialize(ESRI.ArcLogistics.App.Pages.Page page)
        {
            label1.Content = "Data unavailable.";
            try
            {
                string URLString = @"http://www.weather.gov/xml/current_obs/KRAL.xml";
                XmlTextReader reader = new XmlTextReader(URLString);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "temperature_string")
                    {
                        if (reader.Read())
                            label1.Content = "Riverside, CA: " + reader.Value;

                        reader.Close();
                        break;
                    }
                }
            }

            finally
            {

            }
        }

        public override string Title
        {
            get { return "Temperature"; }
        }
    }
}
