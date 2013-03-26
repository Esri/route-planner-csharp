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
