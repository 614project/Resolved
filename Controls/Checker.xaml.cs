using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Networking.NetworkOperators;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls
{
    public sealed partial class Checker : UserControl
    {
        public Checker()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title" , typeof(string) , typeof(Checker) , new PropertyMetadata("Title")
        );
        public string Title {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty , value);
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked" , typeof(bool) , typeof(Checker) , new PropertyMetadata(false)
        );
        public bool IsChecked {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty , value);
        }

        public event EventHandler? OnChecked = null;
        private void MyCheck_Checked(object sender , RoutedEventArgs e)
        {
            OnChecked?.Invoke(null , EventArgs.Empty);
        }
        public event EventHandler? OnUnchecked = null;
        private void MyCheck_Unchecked(object sender , RoutedEventArgs e)
        {
            OnUnchecked?.Invoke(null , EventArgs.Empty);
        }
    }
}
