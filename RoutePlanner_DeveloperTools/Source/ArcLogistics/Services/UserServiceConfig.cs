using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Xml.Serialization;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services.Serialization
{
    /// <summary>
    /// UserServicesInfo class.
    /// </summary>
    [XmlRoot("services")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserServicesInfo
    {
        [XmlElement("map")]
        public UserMapInfo MapInfo { get; set; }

        [XmlElement("solve")]
        public UserSolveInfo SolveInfo { get; set; }

        [XmlElement("servers")]
        public UserServersInfo ServersInfo { get; set; }
    }

    #region map configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// UserMapInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserMapInfo
    {
        [XmlElement("service")]
        public List<UserMapServiceInfo> Services { get; set; }
    }

    /// <summary>
    /// UserMapServiceInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserMapServiceInfo
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("visible")]
        public bool IsVisible { get; set; }

        [XmlElement("opacity")]
        public double Opacity { get; set; }
    }

    #endregion map configuration

    #region routing configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// UserSolveInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserSolveInfo
    {
        [XmlElement("solversettings")]
        public UserSolverSettingsInfo SolverSettingsInfo { get; set; }
    }

    /// <summary>
    /// UserRestrictionsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserRestrictionsInfo
    {
        [XmlElement("restriction")]
        public List<RestrictionInfo> Restrictions { get; set; }
    }

    /// <summary>
    /// UserRouteAttrsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserRouteAttrsInfo
    {
        [XmlElement("param")]
        public List<RouteAttrInfo> AttributeParams { get; set; }
    }

    /// <summary>
    /// UserSolverSettingsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserSolverSettingsInfo
    {
        [XmlElement("uturnatintersections")]
        public bool? UTurnAtIntersections { get; set; }

        [XmlElement("uturnatdeadends")]
        public bool? UTurnAtDeadEnds { get; set; }

        [XmlElement("uturnatstops")]
        public bool? UTurnAtStops { get; set; }

        [XmlElement("stoponorderside")]
        public bool? StopOnOrderSide { get; set; }

        [XmlElement("restrictions")]
        public UserRestrictionsInfo RestrictionsInfo { get; set; }

        [XmlElement("attributeparams")]
        public UserRouteAttrsInfo AttributeParamsInfo { get; set; }

        [XmlElement("arrivedepartdelay")]
        public int? ArriveDepartDelay { get; set; }
    }

    #endregion routing configuration

    #region servers configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// UserServersInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserServersInfo
    {
        [XmlElement("server")]
        public List<UserServerInfo> Servers { get; set; }
    }

    /// <summary>
    /// UserServerInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UserServerInfo
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("credentials")]
        public CredentialsInfo Credentials { get; set; }
    }

    /// <summary>
    /// CredentialsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class  CredentialsInfo
    {
        [XmlAttribute("username")]
        public string UserName { get; set; }

        [XmlIgnore()]
        public string Password { get; set; }

        [XmlAttribute("password")]
        public string XML_Password
        {
            get
            {
                string password = String.Empty;
                if (this.Password != null)
                {
                    try
                    {
                        password = StringProcessor.TransformData(this.Password);
                    }
                    catch (CryptographicException) { }
                }
                return password;
            }
            set
            {
                if (value != null)
                {
                    var password = string.Empty;
                    StringProcessor.TryTransformDataBack(value, out password);
                    this.Password = password;
                }
            }
        }
    }

    #endregion servers configuration
}
