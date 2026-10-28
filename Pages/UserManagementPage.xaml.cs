using AcNET;
using AcNET.User;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Resolved.Collections;
using Resolved.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserManagementPage : Page
    {
        readonly SelectorBarItem mySelectBarItem = new() { Text = "User Management", FontFamily = new("Outfit") };
        //DB에서 한 번만 읽어 같은 인스턴스를 계속 쓴다. (다운로드 중인 유저와 화면의 항목이 같은 객체가 되도록)
        readonly Dictionary<string , ResolvedUser> users = new(StringComparer.OrdinalIgnoreCase);
        public UserManagementPage()
        {
            this.InitializeComponent();
            foreach (ResolvedUser user in Database.Users.FindAll())
                users[user.Handle] = user;
            this.MyUserListView.Update(users.Values.ToArray());
            this.Loaded += this.UserManagementPage_Loaded;
            this.Unloaded += this.UserManagementPage_Unloaded;
            this.debouncer.OnResult += this.DebouncerOnResult;
            this.debouncer.OnError += this.DebouncerOnError;
            this.Search.KeyDown += this.Search_KeyDown;
            this.MyUserListView.SelectUser += this.MyUserListView_SelectUser;
        }

        ResolvedUser? nowUser;
        private void MyUserListView_SelectUser(object? sender , ResolvedUser? info)
        {
            //이전에 할당한거 제거
            if (nowUser != null)
            {
                nowUser.OnDownloadStatusChanged -= ActionButtonsUpdate;
            }
            //새롭게 할당
            nowUser = info;
            if (nowUser != null)
            {
                nowUser.OnDownloadStatusChanged += ActionButtonsUpdate;
            }
            ActionButtonsSetup();
        }

        private void Search_KeyDown(object sender , KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            AddUser_Click(this, e);
            AddUser.StartBringIntoView();
        }

        private void DebouncerOnResult(object? sender , SolvedSocialUser? user)
        {
            bool alreadyExist = user != null && users.ContainsKey(user.Handle);
            predictAddUser = alreadyExist ? null : user;

            bool isEnable = predictAddUser != null;
            AddUser.IsEnabled = isEnable;
            SearchStatus.Text = isEnable ? string.Empty : (alreadyExist ? "Already added" : "No exist user");
        }

        private void DebouncerOnError(object? sender , Exception ex)
        {
            Debug.WriteLine(ex.Message);
            predictAddUser = null;
            AddUser.IsEnabled = false;
            SearchStatus.Text = "Search failed";
        }

        private void UserManagementPage_Unloaded(object sender , RoutedEventArgs e)
        {
            debouncer.Cancel();
            MainWindow.SelectorBar.Items.Remove(mySelectBarItem);
        }

        private void UserManagementPage_Loaded(object sender , RoutedEventArgs e)
        {  
            MainWindow.SelectorBar.Items.Add(mySelectBarItem);
            mySelectBarItem.IsSelected = true;
        }

        readonly Debouncer<string,SolvedSocialUser?> debouncer = new(
            async (name , _) => {
                (SolvedSocialUser? user, Exception? ex) = await ResolvedInfo.API.GetUserAsync(Uri.EscapeDataString(name));
                if (ex is not null and not SolvedAPIException { StatusCode: HttpStatusCode.NotFound })
                    throw ex;
                return user;
            },
            TimeSpan.FromMilliseconds(300)
        );
        SolvedSocialUser? predictAddUser = null;
        private void AddUser_Click(object sender , RoutedEventArgs e)
        {
            if (predictAddUser == null)
                return;
            ResolvedUser resolvedUser = new(predictAddUser);
            Database.Users.Upsert(resolvedUser);
            users[resolvedUser.Handle] = resolvedUser;
            UpdateUserList(Search.Text);
            DebouncerOnResult(null , predictAddUser);
            DispatcherQueue.TryEnqueue(resolvedUser.StartDownload);
        }

        private void Search_TextChanged(object sender , TextChangedEventArgs e)
        {
            predictAddUser = null;
            AddUser.IsEnabled = false;

            string handle = Search.Text;
            if (handle.Length == 0)
            {
                debouncer.Cancel();
                SearchStatus.Text = string.Empty;
                SearchStatus.Visibility = Visibility.Collapsed;
                MyUserListView.Update(users.Values.ToArray());
                return;
            }

            SearchStatus.Visibility = Visibility.Visible;
            SearchStatus.Text = "searching...";
            debouncer.Push(handle);
            UpdateUserList(handle);
        }
        private void UpdateUserList(string handle) => MyUserListView.Update(users.Values.Where(user => user.Handle.Contains(handle , StringComparison.OrdinalIgnoreCase)).ToArray());
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
            Database.Users.Delete(nowUser.Handle);
            users.Remove(nowUser.Handle);
            if (string.Equals(Configuration.Config.CurrentUser , nowUser.Handle , StringComparison.OrdinalIgnoreCase))
            {
                Configuration.Config.CurrentUser = null;
                Configuration.Save();
            }
            MyUserListView_SelectUser(null , null);
            UpdateUserList(Search.Text);
            ActionButtonsSetup();
            if (Search.Text.Length > 0)
                debouncer.Push(Search.Text);
        }

        private void CurrentUserButton_Click(object sender , RoutedEventArgs e)
        {
            if (nowUser == null)
                return;
            Configuration.Config.CurrentUser = nowUser.Handle;
            Configuration.Save();
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
