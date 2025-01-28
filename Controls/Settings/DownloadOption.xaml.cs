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
    public sealed partial class DownloadOption : UserControl
    {
        public DownloadOption()
        {
            this.InitializeComponent();
            SolvedInfo.OnProgressChanged += this.Solved_OnProgressChanged;
            SolvedInfo.OnDownloadEnd += this.Solved_OnDownloadEnd;
        }

        private void Solved_OnDownloadEnd(object? sender , Exception? e)
        {
            DispatcherQueue.TryEnqueue(() => {
                if (e == null)
                    MyButton.Content = "Download success.";
                else
                    MyButton.Content = "Download Failed.";
                MyProgress.Visibility = Visibility.Collapsed;
                MyButton.IsEnabled = true;
                MyButton.MinWidth = 0;
                MainWindow.SelectorBar.SelectedItem = null;
            });
        }

        private void Solved_OnProgressChanged(object? sender , double e)
        {
            DispatcherQueue.TryEnqueue(() => {
                MyProgress.Value = e;
                MyButton.Content = $"{e:F2}% downloaded.";
            });
        }

        private void DownloadStart(object? sender, RoutedEventArgs args)
        {
            MyButton.MinWidth = 160;
            MyButton.IsEnabled = false;
            MyProgress.Visibility = Visibility.Visible;
            SolvedInfo.Download();
        }
    }
}
