using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using System;
using System.Threading.Tasks;

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
                Content = "Can't recover after deleted.\n(Bookmark and class data will also be deleted.)" ,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                FontFamily = new("Outfit")
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ResolvedInfo.RemoveProblems();
                MainWindow.SelectorBar.SelectedItem = null;
            }
        }

        private async void RemoveUsersButton_Click(object sender , RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog {
                XamlRoot = this.XamlRoot ,
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
                ResolvedInfo.RemoveUsers();
                MainWindow.SelectorBar.SelectedItem = null;
            }
        }

        private async void RemoveBookmarksButton_Click(object sender , RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog {
                XamlRoot = this.XamlRoot,
                Title = "Confirm deleting bookmark data?" ,
                Content = "Can't recover after deleted." ,
                PrimaryButtonText = "Delete" ,
                CloseButtonText = "Cancel" ,
                DefaultButton = ContentDialogButton.Primary ,
                FontFamily = new("Outfit")
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ResolvedInfo.RemoveBookmarks();
                MainWindow.SelectorBar.SelectedItem = null;
            }
        }
    }
}
