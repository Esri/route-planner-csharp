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
using System.Data.Objects;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that manages barriers of the project.
    /// </summary>
    public class BarrierManager : DataObjectManager<Barrier>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal BarrierManager(DataObjectContext context, string entityName, SpecFields specFields)
            : base(context, entityName, specFields)
        {
        }
        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns barriers available on specified date.
        /// </summary>
        /// <param name="date">Date used to query barriers.</param>
        /// <param name="asSynchronized">Indicates whether collection remains synchronized when
        /// barriers are added or deleted to the project database.</param>
        /// <returns>Collection of found barriers.</returns>
        public IDataObjectCollection<Barrier> Search(DateTime date,
            bool asSynchronized)
        {
            var filter = Functional.MakeExpression((DataModel.Barriers barrier) =>
                barrier.StartDate <= date && date <= barrier.FinishDate);
            var barriers = BARRIERS_QUERY(_Context, date);

            return this.Query(barriers, asSynchronized ? filter : null);
        }

        /// <summary>
        /// Returns barriers available on specified date.
        /// </summary>
        /// <param name="date">Date used to query barriers.</param>
        /// <returns>Non-synchronized collection of found barriers.</returns>
        public IDataObjectCollection<Barrier> Search(DateTime date)
        {
            return Search(date, false);
        }

        /// <summary>
        /// Returns all barriers available in the project.
        /// </summary>
        /// <returns>Collection of all project's barriers.</returns>
        public IDataObjectCollection<Barrier> SearchAll(bool asSynchronized)
        {
            return _SearchAll<DataModel.Barriers>(asSynchronized);
        }

        #endregion // Public methods

        #region private constants
        /// <summary>
        /// Compiled query for searching barriers for the specified date.
        /// </summary>
        private static readonly Func<DataModel.Entities, DateTime, IQueryable<DataModel.Barriers>>
            BARRIERS_QUERY = CompiledQuery.Compile<
                DataModel.Entities,
                DateTime,
                IQueryable<DataModel.Barriers>>((context, date) => context.Barriers
                    .Where(barrier => barrier.StartDate <= date && date <= barrier.FinishDate));
        #endregion
    }
}
