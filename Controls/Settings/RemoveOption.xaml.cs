using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Resolved.Scripts;
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
    public sealed partial class RemoveOption : UserControl
    {
        public RemoveOption()
        {
            this.InitializeComponent();
        }

        private async void RemoveProblemsButton_Click(object sender , RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog {
                XamlRoot = this.XamlRoot,
                //Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Confirm deleting problem data?",
                Content = "Can't recover after deleted.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                FontFamily = new("Outfit")
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SolvedInfo.RemoveProblems();
                MainWindow.SelectorBar.SelectedItem = null;
            }
        }

        private async void RemoveUsersButton_Click(object sender , RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog {
                XamlRoot = this.XamlRoot ,
                //Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style ,
                Title = "Confirm deleting user data?" ,
                Content = "Can't recover after deleted." ,
                PrimaryButtonText = "Delete" ,
                CloseButtonText = "Cancel" ,
                DefaultButton = ContentDialogButton.Primary ,
                FontFamily = new("Outfit")
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SolvedInfo.RemoveUsers();
                MainWindow.SelectorBar.SelectedItem = null;
            }
        }
    }
}
