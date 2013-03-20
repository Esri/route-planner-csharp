using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// TextBlock derived control that allows to bind Inlines property.
    /// </summary>
    internal class BindableTextBlock : TextBlock
    {
        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.Register("BindableInlines", typeof(ICollection<Inline>), typeof(BindableTextBlock),
            new UIPropertyMetadata(new PropertyChangedCallback(OnBindableInlinesChanged)));

        public ICollection<Inline> BindableInlines
        {
            get { return (ICollection<Inline>)GetValue(BindableInlinesProperty); }
            set { SetValue(BindableInlinesProperty, value); }
        }

        private static void OnBindableInlinesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((BindableTextBlock)obj)._OnBindableInlinesChanged();
        }

        private void _OnBindableInlinesChanged()
        {
            Inlines.Clear();
            if (BindableInlines != null)
                foreach (Inline inline in BindableInlines)
                    Inlines.Add(inline);
        }
    }
}
