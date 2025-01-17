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
using acNET.Problem;
using System.Collections.ObjectModel;
using Resolved.Scripts;
using System.Threading.Tasks;
using System.Threading;

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

        readonly Comparer<SolvedUser>[] SortOptions = [
            Comparer<SolvedUser>.Create((a , b) => a.User.rating.CompareTo(b.User.rating)),
            Comparer<SolvedUser>.Create((a,b) => a.User.solvedCount.CompareTo(b.User.solvedCount)),
            Comparer<SolvedUser>.Create((a , b) => a.User.handle.CompareTo(b.User.handle)),
            Comparer<SolvedUser>.Create((a, b) => a.User.@class.CompareTo(b.User.@class))
        ];

        private SolvedUser[] Cache = [];
        public void Update(SolvedUser[] list)
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
        private void PushProblems(SolvedUser[] list)
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

        private ObservableCollection<SolvedUser> UserCollection { get; set; } = [];

        public static readonly DependencyProperty SideProperty = DependencyProperty.Register(
            "Side" , typeof(object) , typeof(UserListView) , new PropertyMetadata(null)
        );
        public object Side {
            get => GetValue(SideProperty);
            set => SetValue(SideProperty , value);
        }

        public event EventHandler<SolvedUser?>? SelectUser = null;
        private void UsersListView_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            SelectUser?.Invoke(sender , UsersListView.SelectedItem as SolvedUser);
        }

        public int SelectedIndex {
            get => UsersListView.SelectedIndex;
            set => UsersListView.SelectedIndex = value;
        }

        private void DownloadStatusTextBlock_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            if (args.NewValue is not SolvedUser info)
                return;
            var me = (TextBlock)sender;

            me.Text = info.LastDownloadMessage;
            info.OnDownloadStatusChanged += (_ , msg) => {
                DispatcherQueue.TryEnqueue(() => me.Text = msg);
            };
        }
    }
}
