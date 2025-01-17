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
using Resolved.Scripts;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls.Settings
{
    public sealed partial class BackdropOption : UserControl
    {
        public BackdropOption()
        {
            this.InitializeComponent();
        }

        private void BackdropsLoaded(object sender,RoutedEventArgs args)
        {
            //var back = App.MainWindow.SystemBackdrop;
            //if (back is DesktopAcrylicBackdrop)
            //    Backdrops.SelectedIndex = 1;
            //else if (back is MicaBackdrop mica)
            //    if (mica.Kind == Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt)
            //        Backdrops.SelectedIndex = 3;
            //    else
            //        Backdrops.SelectedIndex = 2;
            //else
            //    Backdrops.SelectedIndex = 0;
            Backdrops.SelectedIndex = Configuration.Config.backdrop;
        }

        bool first = true;
        private void BackdropsChanged(object sender,SelectionChangedEventArgs args)
        {
            if (first)
            {
                first = false;
                return;
            }
            Configuration.Config.backdrop = Backdrops.SelectedIndex;
            Configuration.BackdropUpdate();
        }
    }
}