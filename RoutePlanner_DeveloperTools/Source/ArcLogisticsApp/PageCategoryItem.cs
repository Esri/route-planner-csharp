using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class presents page category item - the logical part of UI Category Tab
    /// </summary>
    internal class PageCategoryItem: INotifyPropertyChanged
    {
        #region Public Constants

        public const string IS_ENABLED_PROPERTY_NAME = "IsEnabled";
        public const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        #endregion

        #region Constructors

        public PageCategoryItem() 
        { }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void _NotifyPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region Public Properties

        public virtual bool IsEnabled
        {
            get 
            {
                return _isEnabled;
            }
            set 
            {
                _isEnabled = value;
                _NotifyPropertyChanged(IS_ENABLED_PROPERTY_NAME);
            }
        }

        public string TooltipText
        {
            get
            {
                return _tooltipText;
            }
            set
            {
                _tooltipText = value;
                _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
            }
        }

        #endregion

        #region Private Fields

        private bool _isEnabled;
        private string _tooltipText = null;

        #endregion
    }
}
