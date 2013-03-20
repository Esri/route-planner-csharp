using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_ComboBox", Type = typeof(ComboBox))]

    // List-box cotrol with collection of objects which satisfy any conditions (not already use in current page and they "Day" property is correct for current date)
    internal class RouteNameSelector : Control
    {
        #region Constructors and override methods

        static RouteNameSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RouteNameSelector), new FrameworkPropertyMetadata(typeof(RouteNameSelector)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        }

        #endregion

        #region Public Properties

        public static readonly DependencyProperty DefaultRoutesProperty =
           DependencyProperty.Register("DefaultRoutes", typeof(IList<Route>), typeof(RouteNameSelector));

        /// <summary>
        /// Gets/sets list of default routes available in application.
        /// </summary>
        public IList<Route> DefaultRoutes
        {
            get { return (IList<Route>)GetValue(DefaultRoutesProperty); }
            set { SetValue(DefaultRoutesProperty, value); }
        }

        public static readonly DependencyProperty NewRouteProperty = DependencyProperty.Register(
            "NewRoute",
            typeof(object),
            typeof(RouteNameSelector));

        /// <summary>
        /// Gets/sets string value of new route name
        /// </summary>
        public object NewRoute
        {
            get { return (object)GetValue(NewRouteProperty); }
            set { SetValue(NewRouteProperty, value); }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Method finds all routes what not in current routes and satisfies current day
        /// </summary>
        private void _MakeAvailableRoutes()
        {
            _availableRoutes.Clear();

            Schedule currentSchedule = null;

            OptimizeAndEditPage _schedulePage = (OptimizeAndEditPage)(((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath));

            currentSchedule = _schedulePage.CurrentSchedule;

            if (currentSchedule != null)
            {
                _currentRoutes = currentSchedule.Routes;

                if (_currentRoutes != null && DefaultRoutes != null)
                {
                    foreach (Route defaultRoute in DefaultRoutes)
                    {
                        if (!_IsRouteFound(_currentRoutes, defaultRoute))
                        {
                            Route newRoute = (Route)defaultRoute.Clone();
                            _availableRoutes.Add(newRoute);
                        }
                    }
                }
            }

            _ComboBox.ItemsSource = null;
            _ComboBox.Items.Clear();
            _ComboBox.ItemsSource = _availableRoutes;
        }

        /// <summary>
        /// Inits part visual components.
        /// </summary>
        private void _InitComponents()
        {
            _ComboBox = this.GetTemplateChild("PART_ComboBox") as ComboBox;
            _ComboBox.ItemsSource = _availableRoutes;
        }

        /// <summary>
        /// Method inits handlers for all events
        /// </summary>
        private void _InitEventHandlers()
        {
            _ComboBox.DropDownOpened += new EventHandler(_ComboBox_DropDownOpened);
            _ComboBox.Loaded += new RoutedEventHandler(_ComboBox_Loaded);
        }

        void _ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            _ComboBox.Focus();
        }

        /// <summary>
        /// Method returns true if route found in collection and false otherwise
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        private bool _IsRouteFound(IList<Route> collection, Route defaultRoute)
        {
            bool isFound = false;
           
            foreach (Route collectionRoute in collection)
            {
                if (collectionRoute.DefaultRouteID.Equals(defaultRoute.Id))
                    isFound = true;
            }
            return isFound;
        }

        #endregion

        #region Event handlers

        void _ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            _MakeAvailableRoutes();
        }
                
        #endregion

        #region Private fields

        private ComboBox _ComboBox;

        // Routes available to show in list
        private List<Route> _availableRoutes = new List<Route>();

        private IList<Route> _currentRoutes;

        #endregion
    }
}
