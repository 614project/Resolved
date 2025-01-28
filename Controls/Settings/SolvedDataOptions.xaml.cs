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
using Windows.System;
using Resolved.Pages;
using Microsoft.UI.Composition.Scenes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls.Settings
{
    public sealed partial class SolvedDataOptions : UserControl
    {
        public SolvedDataOptions()
        {
            this.InitializeComponent();
        }

        public void UpdateStatus()
        {
            LastWrtieTimeCard.Content = LastWriteTime;
            CurrentUserCard.Content = CurrentUesr;
        }

        private string DataSavePath => JsonManager.SaveFolder;
        private string LastWriteTime => SolvedInfo.GetLastWriteTime()?.ToString(@"yyyy\-MM\-dd HH\:mm\:ss") ?? "(No saved)";
        private string CurrentUesr => Configuration.Config.currentUser ?? "(None)";

        private async void OpenSaveFolderWithFileExplorer(object sender , RoutedEventArgs e)
        {
            await Launcher.LaunchFolderPathAsync(DataSavePath);
        }

        private void UserDownloadButton_Click(object sender , RoutedEventArgs e)
        {
            MainWindow.Frame.Navigate(typeof(UserManagementPage));
        }
    }
}
