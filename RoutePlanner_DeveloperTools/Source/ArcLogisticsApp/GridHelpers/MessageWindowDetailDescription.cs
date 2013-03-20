using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;

using Xceed.Wpf.DataGrid;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// MessageObjectContext struct
    /// </summary>
    internal struct MessageObjectContext
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageObjectContext(DataObject dataObject)
        {
            if (null == dataObject)
                throw new ArgumentException();

            _name = dataObject.ToString();
            _type = dataObject.GetType();
            _id = dataObject.Id;
        }

        #endregion // Constructors

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string Name
        {
            get { return _name; }
        }

        public Guid Id
        {
            get { return _id; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public string Hyperlink
        {
            get
            {
                if (null == App.Current)
                    return null;
                Project project = App.Current.Project;
                if (null == project)
                    return null;

                string link = null;
                DataObject obj = DataObjectHelper.GetPrescribedObject(_type, _id);
                if (null != obj)
                {
                    if (typeof(Barrier) == _type)
                        link = PagePaths.BarriersPagePath;

                    else if (typeof(Driver) == _type)
                        link = PagePaths.DriversPagePath;

                    else if ((typeof(DriverSpecialty) == _type) ||
                             (typeof(VehicleSpecialty) == _type))
                        link = PagePaths.SpecialtiesPagePath;

                    else if (typeof(FuelType) == _type)
                        link = PagePaths.FuelTypesPagePath;

                    else if (typeof(Location) == _type)
                        link = PagePaths.LocationsPagePath;

                    else if (typeof(MobileDevice) == _type)
                        link = PagePaths.MobileDevicesPagePath;

                    else if ((typeof(Order) == _type) ||
                             (typeof(Route) == _type) ||
                             (typeof(Schedule) == _type))
                    {
                        if (typeof(Route) == _type)
                        {
                            Route route = obj as Route;
                            bool isDefaultRoute = App.Current.Project.DefaultRoutes.Contains(route);
                            link = isDefaultRoute ?
                                        PagePaths.DefaultRoutesPagePath : PagePaths.SchedulePagePath;
                        }
                        else
                            link = PagePaths.SchedulePagePath;
                    }

                    else if (typeof(Vehicle) == _type)
                        link = PagePaths.VehiclesPagePath;

                    else if (typeof(Zone) == _type)
                        link = PagePaths.ZonesPagePath;

                    else
                    {
                        Debug.Assert(false); // NOTE: not supported
                    }
                }

                return link;
            }
        }

        #endregion // Properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name;
        private Type _type;
        private Guid _id;

        #endregion // Private members
    }

    /// <summary>
    /// MessageDescription struct
    /// </summary>
    internal struct MessageDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageDescription(string format, Link link, IList<MessageObjectContext> objects)
        {
            Debug.Assert(!string.IsNullOrEmpty(format));
            _text = format;
            _link = link;

            _objects = objects;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is simple message flag
        /// </summary>
        public bool IsSimpleMessage
        {
            get { return ((null == _objects) || (0 == _objects.Count)); }
        }

        /// <summary>
        /// Fromat message text
        /// </summary>
        /// <remarks>Return only if IsSimpleMessage is true</remarks>
        public string Format
        {
            get
            {
                if (IsSimpleMessage)
                    throw new InvalidOperationException();
                return _text;
            }
        }

        /// <summary>
        /// Relativ objects collection
        /// </summary>
        /// <remarks>Return only if IsSimpleMessage is true</remarks>
        public IList<MessageObjectContext> Objects
        {
            get
            {
                if (IsSimpleMessage)
                    throw new InvalidOperationException();
                return _objects;
            }
        }

        /// <summary>
        /// Message text
        /// </summary>
        /// <remarks>Return only if IsSimpleMessage is false</remarks>
        public string Text
        {
            get
            {
                if (!IsSimpleMessage)
                    throw new InvalidOperationException();
                return _text;
            }
        }

        /// <summary>
        /// Link
        /// </summary>
        public Link Link
        {
            get { return _link; }
        }

        #endregion // Public properties

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            string content = null;
            if (this.IsSimpleMessage)
                content = this.Text;
            else
            {
                string format = this.Format;
                try
                {
                    MatchCollection mc = Regex.Matches(format, @"({\d+})");
                    if (0 == mc.Count)
                        content = format;
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        int index = 0;
                        for (int i = 0; i < mc.Count; ++i)
                        {
                            // add text before objects
                            string stringObj = mc[i].Value;
                            int startIndex = format.IndexOf(stringObj, index);
                            if (0 < startIndex)
                                sb.Append(format.Substring(index, startIndex - index));
                            index = startIndex + stringObj.Length;

                            // add object name
                            MatchCollection mcNum = Regex.Matches(stringObj, @"(\d+)");
                            if (1 == mcNum.Count)
                            {
                                int objNum = Int32.Parse(mcNum[0].Value);
                                if (objNum < this.Objects.Count)
                                    sb.AppendFormat("'{0}'", this.Objects[objNum].Name);
                            }
                        }

                        // add text after all objects
                        if (index < format.Length)
                            sb.Append(format.Substring(index, format.Length - index));

                        content = sb.ToString();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    content = format;
                }
            }

            if ((null != this.Link) && !string.IsNullOrEmpty(this.Link.Text))
                content += string.Format(" {0}", this.Link.Text);

            return content.Trim();
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _text; // NOTE: message or format text
        private Link _link;
        private IList<MessageObjectContext> _objects;

        #endregion // Private members
    }

    /// <summary>
    /// MessageDetailDataWrap struct
    /// </summary>
    internal struct MessageDetailDataWrap
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageDetailDataWrap(MessageType type, MessageDescription description)
        {
            _type = type;
            _description = description;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageType Type
        {
            get { return _type; }
        }

        public MessageDescription Description
        {
            get { return _description; }
        }

        #endregion // Public methods

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return _description.ToString();
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MessageType _type;
        private MessageDescription _description;

        #endregion // Private members
    }

    /// <summary>
    /// MessageLinkHelper class
    /// </summary>
    internal static class MessageLinkHelper
    {
        /// <summary>
        /// Helper function - convert Link class to visual element Hyperlink.
        /// </summary>
        public static Hyperlink CreateHiperlink(Link link)
        {
            Hyperlink hyperlink = null;
            if (null != link && !string.IsNullOrEmpty(link.Text) && !string.IsNullOrEmpty(link.LinkRef))
            {
                hyperlink = new Hyperlink(new Run(link.Text));
                switch (link.Type)
                {
                    case LinkType.Page:
                        hyperlink.Command = new NavigationCommandSimple(link.LinkRef);
                        break;

                    case LinkType.Url:
                        hyperlink.Command = new HelpLinkCommand(link.LinkRef);
                        break;

                    default:
                        System.Diagnostics.Debug.Assert(false); // NOTE: not supported
                        break;
                }
            }

            return hyperlink;
        }
    }

    /// <summary>
    /// MessageDetail class
    /// </summary>
    internal class MessageWindowDetailDescription : DataGridDetailDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageWindowDetailDescription()
        {
            RelationName = "SubMessage";
        }

        #endregion // Constructors

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageWindowDataWrapper? ParentDataObject
        {
            get { return _parentDataObject; }
        }

        #endregion // Public members

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override IEnumerable GetDetailsForParentItem(DataGridCollectionViewBase parentCollectionView, object parentItem)
        {
            IEnumerable<MessageDetailDataWrap> details = new List<MessageDetailDataWrap>();
            if (null != parentItem)
            {
                _parentDataObject = (MessageWindowDataWrapper?)parentItem;
                if (_parentDataObject.HasValue)
                    details = _parentDataObject.Value.Details;
            }

            return details;
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MessageWindowDataWrapper? _parentDataObject = null;

        #endregion // Private members
    }
}
