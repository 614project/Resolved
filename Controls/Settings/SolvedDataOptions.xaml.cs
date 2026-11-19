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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls.Settings
{
    public sealed partial class SolvedDataOptions : UserControl
    {
        public SolvedDataOptions()
        {
            this.InitializeComponent();
            this.Loaded += this.SolvedDataOptions_Loaded;
            this.Unloaded += this.SolvedDataOptions_Unloaded;
        }

        private void SolvedDataOptions_Loaded(object sender , RoutedEventArgs e)
        {
            ResolvedInfo.OnDataChanged += this.ResolvedInfo_OnDataChanged;
            UpdateStatus();
            LoadCredentials();
        }

        private void SolvedDataOptions_Unloaded(object sender , RoutedEventArgs e)
        {
            ResolvedInfo.OnDataChanged -= this.ResolvedInfo_OnDataChanged;
        }

        private void ResolvedInfo_OnDataChanged(object? sender , EventArgs e)
        {
            DispatcherQueue.TryEnqueue(UpdateStatus);
        }

        public void UpdateStatus()
        {
            LastWriteTimeCard.Content = LastWriteTime;
            CurrentUserCard.Content = CurrentUserText;
        }

        private void LoadCredentials()
        {
            ClientIdBox.Text = Configuration.Config.ClientId ?? string.Empty;
            ClientSecretBox.Password = Configuration.Config.ClientSecret ?? string.Empty;
            CredentialsStatus.Text = ResolvedInfo.HasCredentials ? string.Empty : "Not set";
        }

        private void SaveCredentials_Click(object sender , RoutedEventArgs e)
        {
            string id = ClientIdBox.Text.Trim();
            string secret = ClientSecretBox.Password.Trim();
            Configuration.Config.ClientId = id.Length == 0 ? null : id;
            Configuration.Config.ClientSecret = secret.Length == 0 ? null : secret;
            Configuration.Save();
            ResolvedInfo.ResetAPI();
            CredentialsStatus.Text = ResolvedInfo.HasCredentials ? "Saved" : "Not set";
        }

        private void OpenBackstage_Click(object sender , RoutedEventArgs e)
        {
            "https://backstage.solved.ac/developer".OpenToBrowser();
        }

        private string DataSavePath => JsonManager.SaveFolder;
        private string LastWriteTime => ResolvedInfo.GetLastWriteTime()?.ToString(@"yyyy\-MM\-dd HH\:mm\:ss") ?? "(No saved)";
        private string CurrentUserText => Configuration.Config.CurrentUser ?? "(None)";

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
