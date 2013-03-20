using System;
using System.Diagnostics;
using System.Net;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Server authentication type enumeration.
    /// </summary>
    internal enum AgsServerAuthenticationType
    {
        Yes,
        No,
        UseApplicationLicenseCredentials
    }

    /// <summary>
    /// Class contains ArcGIS Server Connection parameters. 
    /// </summary>
    internal class AgsServerConnectionParameters
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal AgsServerConnectionParameters(ServerInfoWrap serverInfo)
        {
            this.SoapUrl = serverInfo.Url;

            if (serverInfo.Authentication != null)
                this.AuthenticationType = _StrToAuthType(serverInfo.Authentication);
            else
                this.AuthenticationType = AgsServerAuthenticationType.No;

            if (serverInfo.Credentials != null)
            {
                this.Credentials = new NetworkCredential(
                    serverInfo.Credentials.UserName,
                    serverInfo.Credentials.Password);
            }
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string SoapUrl { get; internal set; }

        public NetworkCredential Credentials { internal get; set; }

        public AgsServerAuthenticationType AuthenticationType { get; internal set; }

        #endregion public properties

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static AgsServerAuthenticationType _StrToAuthType(string typeStr)
        {
            Debug.Assert(typeStr != null);

            AgsServerAuthenticationType type = AgsServerAuthenticationType.No; // default value
            try
            {
                type = (AgsServerAuthenticationType)Enum.Parse(
                    typeof(AgsServerAuthenticationType),
                    typeStr,
                    true);
            }
            catch { }

            return type;
        }

        #endregion private methods
    }
}
