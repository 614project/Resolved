using acNET.User;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Resolved.Scripts;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserManagementPage : Page
    {
        static readonly SelectorBarItem mySelectBarItem = new() { Text = "User Management", FontFamily = new("Outfit") };
        public UserManagementPage()
        {
            this.InitializeComponent();
            this.MyUserListView.Update(Solved.Users.Values.ToArray());
            this.AddUser.MinWidth = this.AddUser.ActualWidth;
            this.Loaded += this.UserManagementPage_Loaded;
            this.Unloaded += this.UserManagementPage_Unloaded;
            this.debouncer.OnResult += this.Debouncer_OnResult;
            this.Search.KeyDown += this.Search_KeyDown;
            this.MyUserListView.SelectUser += this.MyUserListView_SelectUser;
        }

        SolvedUser? nowUser;
        private void MyUserListView_SelectUser(object? sender , SolvedUser? info)
        {
            //이전에 할당한거 제거
            if (nowUser != null)
            {
                nowUser.OnDownloadStatusChanged -= ActionButtonsUpdate;
            }
            //새롭게 할당
            if ((nowUser = info) == null)
                return;
            nowUser.OnDownloadStatusChanged += ActionButtonsUpdate;
            // 기존 컨트롤 초기화
            ActionStatusTextBlock.Text = info == null ? "Select a user." : "What would you like to do?";
            ActionButtonsSetup();
        }

        private void Search_KeyDown(object sender , KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            AddUser_Click(this, e);
            AddUser.StartBringIntoView();
        }

        private void Debouncer_OnResult(object? sender , RankedUser? user)
        {
            predictAddUser = user;
            bool alreadyExist = false;
            if (predictAddUser != null)
            {
                if (alreadyExist = Solved.Users.ContainsKey(predictAddUser.handle))
                    predictAddUser = null;
            }

            bool isEnable = predictAddUser != null;
            string message = isEnable ? string.Empty : (alreadyExist ? "Already added" : "No exist user");

            DispatcherQueue.TryEnqueue(() => {
                AddUser.IsEnabled = isEnable;
                SearchStatus.Text = message;
            });
        }

        private void UserManagementPage_Unloaded(object sender , RoutedEventArgs e)
        {
            MainWindow.SelectorBar.Items.Remove(mySelectBarItem);
            Solved.UsersSave();
        }

        private void UserManagementPage_Loaded(object sender , RoutedEventArgs e)
        {  
            MainWindow.SelectorBar.Items.Add(mySelectBarItem);
            mySelectBarItem.IsSelected = true;
        }

        private void MySelectorBar_SelectionChanged(SelectorBar sender , SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem.TabIndex == 0)
                MainWindow.Frame.Navigate(typeof(SettingPage));
        }

        Debouncer<string,RankedUser?> debouncer = new(
            name => Solved.API.GetUser(name).Result
        );
        RankedUser? predictAddUser = null;
        private void AddUser_Click(object sender , RoutedEventArgs e)
        {
            if (predictAddUser == null)
                return;
            SolvedUser solvedUser = new(predictAddUser);
            Solved.Users.Add(solvedUser.Handle, solvedUser);
            UpdateUserList(solvedUser.Handle);
            Debouncer_OnResult(null , predictAddUser);
            DispatcherQueue.TryEnqueue(solvedUser.StartDownload);
        }

        private void Search_TextChanged(object sender , TextChangedEventArgs e)
        {
            predictAddUser = null;
            AddUser.IsEnabled = false;

            string handle = Search.Text;
            if (handle.Length == 0)
            {
                SearchStatus.Text = string.Empty;
                SearchStatus.Visibility = Visibility.Collapsed;
                MyUserListView.Update(Solved.Users.Values.ToArray());
                return;
            }

            SearchStatus.Visibility = Visibility.Visible;
            SearchStatus.Text = "searching...";
            debouncer.Current = handle;
            UpdateUserList(handle);
        }
        private void UpdateUserList(string handle) => MyUserListView.Update(Solved.Users.Values.Where(user => user.Handle.Contains(handle)).ToArray());
        private void ActionButtonsSetup()
        {
            bool exist = nowUser != null;
            bool downloadable = nowUser?.CanDownload ?? false;

            //다운에 영향을 받음.
            DownloadButton.IsEnabled = downloadable;
            CurrentUserButton.IsEnabled = downloadable;

            //존재에 영향을 받음
            ActionStatusTextBlock.Text = exist ? "What would you like to do?" : "Select a user.";
            RemoveButton.IsEnabled = exist; //다운하든지 말든지 없애면 그만
            OpenBOJButton.IsEnabled = exist;
            OpenSolvedacButton.IsEnabled = exist;
        }
        private void DownloadButtonClick(object sender , RoutedEventArgs e)
        {
            if (nowUser?.CanDownload ?? false)
            {
                nowUser.StartDownload();
                ActionButtonsSetup();
            }
        }
        private void ActionButtonsUpdate(object? sender, string msg) => DispatcherQueue.TryEnqueue(ActionButtonsSetup);

        private void RemoveButton_Click(object sender , RoutedEventArgs e)
        {
            if (nowUser == null) return;
            Solved.Users.Remove(nowUser.Handle);
            if (Configuration.CurrentUser?.Handle == nowUser.Handle)
            {
                Configuration.Config.currentUser = null;
            }
            MyUserListView_SelectUser(null , null);
            UpdateUserList(Search.Text);
            ActionButtonsSetup();
        }

        private void CurrentUserButton_Click(object sender , RoutedEventArgs e)
        {
            if (nowUser == null)
                return;
            Configuration.Config.currentUser = nowUser.Handle;
        }

        private void OpenBOJButton_Click(object sender , RoutedEventArgs e)
        {
            if (nowUser == null)
                return;

            ("https://www.acmicpc.net/user/" + nowUser.Handle).OpenToBrowser();
        }

        private void OpenSolvedacButton_Click(object sender , RoutedEventArgs e)
        {
            if (nowUser == null)
                return;

            ("https://solved.ac/profile/" + nowUser.Handle).OpenToBrowser();
        }
    }

    
}
