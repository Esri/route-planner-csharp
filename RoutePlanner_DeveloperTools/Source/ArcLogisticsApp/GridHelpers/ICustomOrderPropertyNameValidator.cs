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
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Interface provides method for Custom order property name validation.
    /// </summary>
    internal interface ICustomOrderPropertyNameValidator
    {
        /// <summary>
        /// Performs validation of custom order property name.
        /// </summary>
        /// <param name="propertyName">Custom order property name.</param>
        /// <param name="errorMessage">Output error message.</param>
        /// <returns>True - if property name is valid, otherwise - false.</returns>
        bool Validate(string propertyName, out string errorMessage);
    }
}
