/*
COPYRIGHT 1995-2010 ESRI
TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
Unpublished material - all rights reserved under the 
Copyright Laws of the United States.
For additional information, contact:
Environmental Systems Research Institute, Inc.
Attn: Contracts Dept
380 New York Street
Redlands, California, USA 92373
email: contracts@esri.com
*/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Navigation;

using Microsoft.Windows.Controls;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Export;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Widgets;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Pages
{
    // NOT USED: ARCLOGISTICS-1784

    /// <summary>
    /// Interaction logic for ExportPageOld.xaml
    /// </summary>
    internal partial class ExportPageOld : PageBase
    {
        public const string PAGE_NAME = "Export";

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ExportPageOld()
        {
            InitializeComponent();

            IsRequired = true;
            IsAllowed = true;
            this.Loaded += new RoutedEventHandler(_Page_Loaded);
            this.Unloaded += new RoutedEventHandler(_Page_Unloaded);
        }

        #endregion // Constructors

        #region Page overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("ExportPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("ExportProfilesBrush"); }
        }

        #endregion // Page overrided members

        #region PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.ExportPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        protected override void _CreateWidgets()
        {
            base._CreateWidgets();

            DateRangeCalendarWidget calendarWidget = new DateRangeCalendarWidget("CalendarWidgetCaption");
            calendarWidget.Initialize(this);
            this._EditableWidgetCollection.Insert(0, calendarWidget);
        }

        #endregion // PageBase overrided members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void ComboBoxProfile_DropDownClosed(object sender, EventArgs e)
        {
            _UpdateGUI();
        }

        private void ButtonCreateProfile_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Navigate(PagePaths.ExportProfilesPagePath);
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(0 < _exportedSchedule.Count);

            buttonExport.IsEnabled = false;
            string profileName = (string)comboboxProfile.SelectedItem;

            ICollection<Profile> profiles = App.Current.Exporter.Profiles;
            foreach (Profile profile in profiles)
            {
                if (profileName == profile.Name)
                {   // use selected profile
                    _DoExport(profile, _exportedSchedule);
                    break;
                }
            }

            buttonExport.IsEnabled = true;
        }

        private void buttonExport_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            buttonExport.ToolTip = (string)App.Current.FindResource(buttonExport.IsEnabled ?
                                                                    "ExportCommandEnabledTooltip" : "ExportCommandDisabledTooltip");
        }

        private void _Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, "");

            _UpdateScheduleList();
            _UpdateProfiles();
            _UpdateGUI();

            _GetCalendarWidget().SelectedDatesChanged += new EventHandler(_calendarWidget_SelectedDatesChanged);
        }

        private void _Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _GetCalendarWidget().SelectedDatesChanged -= _calendarWidget_SelectedDatesChanged;
        }

        private void _calendarWidget_SelectedDatesChanged(object sender, EventArgs e)
        {
            _UpdateScheduleList();
            _UpdateGUI();
        }

        #endregion // Event handlers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DateRangeCalendarWidget _GetCalendarWidget()
        {
            Debug.Assert(this._EditableWidgetCollection[0] is DateRangeCalendarWidget);
            return this._EditableWidgetCollection[0] as DateRangeCalendarWidget;
        }

        private void _UpdateGUI()
        {
            string message = "";
            if (0 == _exportedSchedule.Count)
            {
                DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
                message = string.Format((string)App.Current.FindResource("ExportNotFoundScheduleMessageFormat"),
                                         calendarWidget.StartDate.ToShortDateString(), calendarWidget.EndDate.ToShortDateString());
            }
            App.Current.MainWindow.StatusBar.SetStatus(this, message);

            buttonExport.IsEnabled = ((0 < _exportedSchedule.Count) && (-1 != comboboxProfile.SelectedIndex));
        }

        private void _UpdateProfiles()
        {
            StringCollection profilesNameList = new StringCollection();
            foreach (Profile profile in App.Current.Exporter.Profiles)
                profilesNameList.Add(profile.Name);

            comboboxProfile.ItemsSource = profilesNameList;
            comboboxProfile.SelectedIndex = -1;

            // hide users tooltip message
            Visibility visibility = (0 < profilesNameList.Count)? Visibility.Hidden : Visibility.Visible;
            buttonCreateProfile.Visibility = visibility;
            textBlockUserTooltip.Visibility = visibility;
        }

        private void _UpdateScheduleList()
        {
            // create list of schedule in date range
            DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
            _exportedSchedule =  ScheduleHelper.GetCurrentSchedulesByDates(calendarWidget.StartDate, calendarWidget.EndDate, true);
        }

        private void _DoExport(Profile profile, ICollection<Schedule> exportedSchedule)
        {
            string statusMessage = (string)App.Current.FindResource("ExportStartMessage");
            WorkingStatusHelper.SetBusy(statusMessage);

            try
            {
                MapLayer currentMapLayer = null;
                foreach (MapLayer layer in App.Current.Map.Layers)
                {
                    if (layer.IsVisible && layer.IsBaseMap)
                    {
                        currentMapLayer = layer;
                        break;
                    }
                }

                App.Current.Exporter.DoExport(profile, exportedSchedule, currentMapLayer);

                string format = (string)App.Current.FindResource("ExportMessageFormatSucceded");
                statusMessage = string.Format(format, ExportProfilesEditPage.GetTypeFaceName(profile.Type), profile.FilePath);
                App.Current.Messenger.AddInfo(statusMessage);
            }
            catch (Exception ex)
            {
                string format = (string)App.Current.FindResource("ExportMessageFormatFailed");
                statusMessage = string.Format(format, ExportProfilesEditPage.GetTypeFaceName(profile.Type), profile.FilePath);

                if (ex is AuthenticationException || ex is CommunicationException)
                {
                    string service = (string)App.Current.FindResource("ServiceNameMap");
                    CommonHelpers.AddServiceMessageWithDetail(statusMessage, service, ex);
                    Logger.Error(ex);
                }
                else
                {
                    string message = string.Format("{0} {1}", statusMessage, ex.Message);
                    App.Current.Messenger.AddError(message);
                    Logger.Critical(ex);
                }
            }

            WorkingStatusHelper.SetReleased();
            App.Current.MainWindow.StatusBar.SetStatus(this, statusMessage);
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ICollection<Schedule> _exportedSchedule = null;

        #endregion // Private members
    }
}
