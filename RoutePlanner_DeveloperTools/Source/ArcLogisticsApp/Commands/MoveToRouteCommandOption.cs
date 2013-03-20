using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.App.Pages;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class implements logic for "MoveToBestRoute" option.
    /// </summary>
    internal class MoveToRouteCommandOption : MoveToCommandOptionBase
    {
        #region Constructors

        /// <summary>
        /// Crates new MoveToRouteCommandOption. Initializes all main class fields.
        /// </summary>
        /// <param name="groupId">Option group ID (to set in separate group in UI).</param>
        /// <param name="targetRoute">Processed route.</param>
        public MoveToRouteCommandOption(int groupId, Route targetRoute)
        {
            GroupID = groupId;
            _targetRoute = targetRoute;
            EnabledTooltip = null;
            DisabledTooltip = null;
        }

        #endregion

        #region CommandBase Members

        /// <summary>
        /// Gets command option title.
        /// </summary>
        public override string Title
        {
            get 
            {
                Debug.Assert(_targetRoute != null);
                return _targetRoute.Name; 
            }
        }

        /// <summary>
        /// Gets/sets tooltip.
        /// </summary>
        public override string TooltipText
        {
            get;
            protected set;
        }

        #endregion

        #region RoutingCommandOptionBase Members

        /// <summary>
        ///Checks whether option enabled.
        /// </summary>
        /// <returns></returns>
        protected override void _CheckEnabled(OptimizeAndEditPage schedulePage)
        {
            IsEnabled = !_targetRoute.IsLocked;
        }

        /// <summary>
        /// Starts operation process.
        /// </summary>
        /// <param name="args"></param>
        protected override void _Execute(params object[] args)
        {
            Debug.Assert(_targetRoute != null);
            ICollection<Route> targetRoutes = new Collection<Route>();
            targetRoutes.Add(_targetRoute);
            base._Execute(targetRoutes, _targetRoute.Name);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Target route.
        /// </summary>
        private Route _targetRoute = null;

        #endregion
    }
}
