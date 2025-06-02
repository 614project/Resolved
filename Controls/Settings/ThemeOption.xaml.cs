using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls.Settings
{
    public sealed partial class ThemeOption : UserControl
    {
        public ThemeOption()
        {
            InitializeComponent();
        }

         ApplicationTheme Default { get; set; }
        private void ThemeComboBoxLoaded(object sender , RoutedEventArgs e)
        {
            Default = App.Current.RequestedTheme;
            //ThemeComboBox.SelectedIndex = App.Current.RequestedTheme switch {
            //    ApplicationTheme.Light => 1,
            //    ApplicationTheme.Dark => 2,
            //    _ => 0
            //};
        }

        private void ThemeComboBoxSelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            App.Current.RequestedTheme = ThemeComboBox.SelectedIndex switch {
                1 => ApplicationTheme.Light,
                2 => ApplicationTheme.Dark,
                _ => Default,
            };
        }
    }
}
