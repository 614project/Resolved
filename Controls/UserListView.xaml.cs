using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Resolved.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls
{
    public sealed partial class UserListView : UserControl
    {
        int prevIndex = 0;
        bool ascending = true;
        public UserListView()
        {
            this.InitializeComponent();
        }

        readonly Comparer<ResolvedUser>[] SortOptions = [
            Comparer<ResolvedUser>.Create((a , b) => a.User.Rating.CompareTo(b.User.Rating)),
            Comparer<ResolvedUser>.Create((a,b) => a.User.SolvedCount.CompareTo(b.User.SolvedCount)),
            Comparer<ResolvedUser>.Create((a , b) => a.User.Handle.CompareTo(b.User.Handle)),
            Comparer<ResolvedUser>.Create((a, b) => a.User.Class.CompareTo(b.User.Class))
        ];

        private ResolvedUser[] Cache = [];
        public void Update(ResolvedUser[] list)
        {
            Cache = list;
            if (SortBySelector.SelectedItem == null)
                return;
            UserCollection.Clear();
            if (SortBySelector.SelectedItem.TabIndex == 4)
                Random.Shared.Shuffle(list);
            else
                Array.Sort(list , SortOptions[SortBySelector.SelectedItem.TabIndex]);

            PushProblems(list);
        }
        private void PushProblems(ResolvedUser[] list)
        {
            UserCollection.Clear();
            if (ascending)
            {
                for (int i = 0 ; i < list.Length ; i++)
                    UserCollection.Add(list[i]);
            }
            else
            {
                for (int i = list.Length - 1 ; i >= 0 ; i--)
                    UserCollection.Add(list[i]);
            }
        }
        private void SortBySelector_SelectionChanged(SelectorBar sender , SelectorBarSelectionChangedEventArgs args)
        {
            int now = sender.SelectedItem.TabIndex;
            if (now > 4)
            {
                SortBySelector.SelectedItem.Text = (ascending = !ascending) ? "Ascending" : "Descending";
                sender.SelectedItem = sender.Items[prevIndex];
                return;
            }
            if (now == prevIndex)
            {
                PushProblems(Cache);
                return;
            }
            prevIndex = now;
            Update(Cache);
        }

        private ObservableCollection<ResolvedUser> UserCollection { get; set; } = [];

        public static readonly DependencyProperty SideProperty = DependencyProperty.Register(
            "Side" , typeof(object) , typeof(UserListView) , new PropertyMetadata(null)
        );
        public object Side {
            get => GetValue(SideProperty);
            set => SetValue(SideProperty , value);
        }

        public event EventHandler<ResolvedUser?>? SelectUser = null;
        private void UsersListView_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            SelectUser?.Invoke(sender , UsersListView.SelectedItem as ResolvedUser);
        }

        public int SelectedIndex {
            get => UsersListView.SelectedIndex;
            set => UsersListView.SelectedIndex = value;
        }

        private void DownloadStatusTextBlock_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            if (args.NewValue is not ResolvedUser info)
                return;
            var me = (TextBlock)sender;

            me.Text = info.LastDownloadMessage;
            info.OnDownloadStatusChanged += (_ , msg) => {
                DispatcherQueue.TryEnqueue(() => me.Text = msg);
            };
        }
    }
}
