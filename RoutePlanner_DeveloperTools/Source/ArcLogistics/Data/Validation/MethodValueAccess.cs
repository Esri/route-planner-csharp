//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Validation Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

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
using System.Globalization;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Validation.Properties;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace ESRI.ArcLogistics.Data.Validation
{
    /// <summary>
    /// Represents the logic to access values from a method.
    /// </summary>
    /// <seealso cref="ValueAccess"/>
    internal sealed class MethodValueAccess : ValueAccess
    {
        private const string ErrorValueAccessNull = "The value for \"{0}\" could not be accessed from null.";
        private const string ErrorValueAccessInvalidType = "The value for \"{0}\" could not be accessed from an instance of \"{1}\".";

        private MethodInfo methodInfo;

        public MethodValueAccess(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public override bool GetValue(object source, out object value, out string valueAccessFailureMessage)
        {
            value = null;
            valueAccessFailureMessage = null;

            if (null == source)
            {
                valueAccessFailureMessage 
                    = string.Format(
                        CultureInfo.CurrentCulture,
                        ErrorValueAccessNull,
                        this.Key);
                return false;
            }
            if (!this.methodInfo.DeclaringType.IsAssignableFrom(source.GetType()))
            {
                valueAccessFailureMessage 
                    = string.Format(
                        CultureInfo.CurrentCulture,
                        ErrorValueAccessInvalidType,
                        this.Key,
                        source.GetType().FullName);
                return false;
            }
            
            value = this.methodInfo.Invoke(source, null);
            return true;
        }

        public override string Key
        {
            get { return this.methodInfo.Name; }
        }

        #region test only properties

        internal MethodInfo MethodInfo
        {
            get { return this.methodInfo; }
        }

        #endregion
    }
}
