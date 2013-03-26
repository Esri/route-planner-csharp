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
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Reflection;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// ObjectInitData class.
    /// </summary>
    internal class ObjectInitData
    {
        public CapacitiesInfo CapacitiesInfo { get; set; }
        public OrderCustomPropertiesInfo OrderCustomPropertiesInfo { get; set; }
    }

    /// <summary>
    /// DataObjectFactory class.
    /// </summary>
    internal class DataObjectFactory
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public DataObjectFactory(ObjectInitData initData)
        {
            Debug.Assert(initData != null);
            _initData = initData;
        }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates data object with specified entity object.
        /// </summary>
        /// <param name="entity">entity object.</param>
        /// <returns>Generic data object.</returns>
        public T CreateObject<T>(EntityObject entity)
            where T : DataObject
        {
            // create object
            T obj = _CreateObject<T>(entity);

            // make post-initialization
            _InitObject(obj);

            return obj;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _InitObject(DataObject obj)
        {
            // CapacitiesInfo
            ICapacitiesInit capacityInit = obj as ICapacitiesInit;
            if (capacityInit != null)
                capacityInit.CapacitiesInfo = _initData.CapacitiesInfo;

            // OrderCustomPropertiesInfo
            IOrderPropertiesInit orderPropInit = obj as IOrderPropertiesInit;
            if (orderPropInit != null)
                orderPropInit.OrderCustomPropertiesInfo = _initData.OrderCustomPropertiesInfo;
        }

        private static T _CreateObject<T>(EntityObject entity)
            where T : DataObject
        {
            Debug.Assert(entity != null);

            var constructorFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.CreateInstance;
            var obj = (T)Activator.CreateInstance(
                typeof(T),
                constructorFlags,
                null,
                new object[] { entity },
                null);

            return obj;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ObjectInitData _initData;

        #endregion private fields
    }
}
