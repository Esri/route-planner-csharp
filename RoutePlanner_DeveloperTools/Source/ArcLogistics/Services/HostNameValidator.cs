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
using System.Net;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// The default implementation of the <see cref="IHostNameValidator"/> interface.
    /// </summary>
    internal sealed class HostNameValidator : IHostNameValidator
    {
        /// <summary>
        /// Validates the specified host name and returns appropriate validation result object.
        /// </summary>
        /// <param name="hostname">The host name to be validated.</param>
        /// <returns>A <see cref="ValidationResult"/> object describing the outcome of
        /// validation.</returns>
        public ValidationResult Validate(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                return new ValidationResult(false, Properties.Messages.Error_HostNameIsEmpty);
            }

            try
            {
                new WebProxy(hostname);
            }
            catch (UriFormatException e)
            {
                return new ValidationResult(false, e.Message);
            }

            return ValidationResult.ValidResult;
        }
    }
}
