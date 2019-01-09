/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Windows;

using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics;
using ESRI.ArcLogistics.DomainObjects;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// Class for sending mail messages
    /// </summary>
    class Mailer : IDisposable
    {
        #region constructors

        /// <summary>
        /// Mailer constructor
        /// </summary>
        /// <param name="mailerSettingsConfig">Mailer settings confuration</param>
        /// <param name="resources">Plugin resources</param>
        public Mailer(GrfExporterSettingsConfig grfExporterSettingsConfig)
        {
            _CreateSMTPClient(grfExporterSettingsConfig);

            if (!grfExporterSettingsConfig.RememberPassword)
            {
                bool passwordEntered = MailAuthorisationDlg.Execute(_client, grfExporterSettingsConfig);
                if (!passwordEntered)
                    throw new InvalidOperationException(Properties.Resources.CanceledByUser);
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(_ValidateServerCertificate);
        }

        #endregion

        #region public members

        /// <summary>
        /// Send mail async
        /// </summary>
        /// <param name="emailTo">EMail To address</param>
        /// <param name="emailFrom">EMail From address</param>
        /// <param name="subject">EMail message subject</param>
        /// <param name="body">EMail message body</param>
        /// <param name="attachments">EMail message attachment filenames</param>
        /// <param name="token">EMail message token</param>
        public void Send(string emailTo, string emailFrom, string subject, string body,
            string[] attachments, string token)
        {
            MailMessage message = new MailMessage(emailFrom, emailTo, subject, body);

            foreach (string fileName in attachments)
            {
                Attachment attachment = new Attachment(fileName);
                message.Attachments.Add(attachment);
            }

            _client.Send(message);
        }

        #endregion

        #region IDisposable interface members

        /// <summary>
        /// Dispose internal smtp client
        /// </summary>
        public void Dispose()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        #endregion

        #region private members

        /// <summary>
        /// Create internal smtp client
        /// </summary>
        private void _CreateSMTPClient(GrfExporterSettingsConfig grfExporterSettingsConfig)
        {
            if (string.IsNullOrEmpty(grfExporterSettingsConfig.ServerAddress))
            {
                throw new MailerSettingsException(Properties.Resources.MailerSettingsEmpty);
            }
            else
            {
                _client = new SmtpClient();
                _client.Host = grfExporterSettingsConfig.ServerAddress;
                _client.Port = grfExporterSettingsConfig.ServerPort;
                _client.EnableSsl = grfExporterSettingsConfig.EncryptedConnectionRequires;

                _client.UseDefaultCredentials = false;

                if (grfExporterSettingsConfig.AutenticationRequired && !string.IsNullOrEmpty(grfExporterSettingsConfig.UserName) 
                    && grfExporterSettingsConfig.Password != null)
                    _client.Credentials = new NetworkCredential(grfExporterSettingsConfig.UserName, grfExporterSettingsConfig.Password);
            }
        }

        /// <summary>
        /// Callback for certificate validation
        /// </summary>
        /// <returns>Always returns true</returns>
        private bool _ValidateServerCertificate(object sender, 
            X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion

        #region private members

        private static SmtpClient _client = null;

        #endregion
    }
}