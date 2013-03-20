using System;
using System.Xml;
using System.Text;
using System.Globalization;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// ConfigDataSerializer class.
    /// </summary>
    internal class ConfigDataSerializer
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string NODE_CAPACITIES = "Capacities";
        private const string NODE_CAPACITY = "Capacity";
        private const string NODE_ORDERCUSTOMPROP = "CustomOrderProperties";
        private const string NODE_PROPERTY = "Property";
        private const string ATTR_NAME = "Name";
        private const string ATTR_DESCRIPTION = "Description";
        private const string ATTR_TYPE = "Type";
        private const string ATTR_DISPLAYUNITUS = "DisplayUnitUS";
        private const string ATTR_DISPLAYUNITMETRIC = "DisplayUnitMetric";
        private const string ATTR_MAXLENGTH = "MaxLength";
        private const string ATTR_VERSION = "Version";
        private const string ATTR_ORDERPAIRKEY = "OrderPairKey";

        private const double ORDER_CUST_PROP_VERSION_NUMBER_1_1 = 1.1;
        private const double ORDER_CUST_PROP_VERSION_NUMBER_1_2 = 1.2;
        private const double ORDER_CUST_PROP_VERSION_NUMBER_1_3 = 1.3;
        private const double ORDER_CUST_PROP_CURRENT_VERSION = ORDER_CUST_PROP_VERSION_NUMBER_1_3;

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses CapacitiesInfo.
        /// </summary>
        public static CapacitiesInfo ParseCapacitiesInfo(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            CapacitiesInfo info = null;
            XmlNode root = xmlDoc.SelectSingleNode(NODE_CAPACITIES);
            if (root != null)
            {
                info = new CapacitiesInfo();
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue; // skip comments and other non element nodes

                    if (node.Name.Equals(NODE_CAPACITY, StringComparison.OrdinalIgnoreCase))
                    {
                        XmlAttributeCollection attributes = node.Attributes;
                        string name = attributes[ATTR_NAME].Value;
                        Unit unitUS = (Unit)Enum.Parse(typeof(Unit), attributes[ATTR_DISPLAYUNITUS].Value);
                        Unit unitMetric = (Unit)Enum.Parse(typeof(Unit), attributes[ATTR_DISPLAYUNITMETRIC].Value);
                        info.Add(new CapacityInfo(name, unitUS, unitMetric));
                    }
                    else
                        throw new FormatException();
                }
            }
            else
                throw new FormatException();

            return info;
        }

        /// <summary>
        /// Parses OrderCustomPropertiesInfo.
        /// </summary>
        public static OrderCustomPropertiesInfo ParseOrderCustomPropertiesInfo(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            OrderCustomPropertiesInfo info = null;
            XmlNode root = xmlDoc.SelectSingleNode(NODE_ORDERCUSTOMPROP);
            if (root != null)
            {
                info = new OrderCustomPropertiesInfo();
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue; // skip comments and other non element nodes

                    if (node.Name.Equals(NODE_PROPERTY, StringComparison.OrdinalIgnoreCase))
                    {
                        XmlAttributeCollection attributes = node.Attributes;

                        string name = attributes[ATTR_NAME].Value;
                        string description = null;
                        if (null != attributes[ATTR_DESCRIPTION])
                            description = attributes[ATTR_DESCRIPTION].Value;
                        int length = int.Parse(attributes[ATTR_MAXLENGTH].Value);

                        OrderCustomPropertyType type = OrderCustomPropertyType.Text; // NOTE: for default used text
                        XmlAttribute typeAttribute = attributes[ATTR_TYPE];
                        if (null != typeAttribute)
                            type = (OrderCustomPropertyType)Enum.Parse(typeof(OrderCustomPropertyType), typeAttribute.Value);

                        bool orderPairKey = false;  // default
                        XmlAttribute orderPairKeyAttribute = attributes[ATTR_ORDERPAIRKEY];
                        if (null != orderPairKeyAttribute)
                            orderPairKey = bool.Parse(orderPairKeyAttribute.Value);

                        info.Add(new OrderCustomProperty(name, type, length, description, orderPairKey));
                    }
                    else
                        throw new FormatException();
                }
            }
            else
                throw new FormatException();

            return info;
        }

        /// <summary>
        /// Serializes CapacitiesInfo object.
        /// </summary>
        public static string SerializeCapacitiesInfo(CapacitiesInfo capacitiesInfo)
        {
            string xml = null;
            XmlWriter writer = null;
            try
            {
                StringBuilder sb = new StringBuilder();
                writer = XmlWriter.Create(sb);

                writer.WriteStartElement(NODE_CAPACITIES);
                for (int index = 0; index < capacitiesInfo.Count; index++)
                {
                    writer.WriteStartElement(NODE_CAPACITY);
                    writer.WriteAttributeString(ATTR_NAME, capacitiesInfo[index].Name);
                    writer.WriteAttributeString(ATTR_DISPLAYUNITUS, capacitiesInfo[index].DisplayUnitUS.ToString());
                    writer.WriteAttributeString(ATTR_DISPLAYUNITMETRIC, capacitiesInfo[index].DisplayUnitMetric.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();

                xml = sb.ToString();
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return xml;
        }

        /// <summary>
        /// Serializes OrderCustomPropertiesInfo object.
        /// </summary>
        public static string SerializeOrderCustomPropertiesInfo(OrderCustomPropertiesInfo orderCustomPropertiesInfo)
        {
            string xml = null;
            XmlWriter writer = null;
            try
            {
                StringBuilder sb = new StringBuilder();
                writer = XmlWriter.Create(sb);

                writer.WriteStartElement(NODE_ORDERCUSTOMPROP);
                writer.WriteAttributeString(ATTR_VERSION, ORDER_CUST_PROP_CURRENT_VERSION.ToString(CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE)));
                for (int index = 0; index < orderCustomPropertiesInfo.Count; index++)
                {
                    OrderCustomProperty property = orderCustomPropertiesInfo[index];
                    writer.WriteStartElement(NODE_PROPERTY);
                    writer.WriteAttributeString(ATTR_NAME, property.Name);
                    writer.WriteAttributeString(ATTR_TYPE, property.Type.ToString());
                    writer.WriteAttributeString(ATTR_MAXLENGTH, property.Length.ToString());
                    writer.WriteAttributeString(ATTR_DESCRIPTION, property.Description);
                    writer.WriteAttributeString(ATTR_ORDERPAIRKEY, property.OrderPairKey.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();

                xml = sb.ToString();
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return xml;
        }

        #endregion public methods
    }
}
