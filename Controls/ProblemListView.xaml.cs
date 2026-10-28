using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Linq;
using Resolved.Collections;
using Windows.System;

namespace Resolved.Controls
{
    public sealed partial class ProblemListView : UserControl
    {
        public static readonly DependencyProperty ProblemSourceProperty = DependencyProperty.Register(
            "ProblemSource" , typeof(ResolvedProblem[]) , typeof(ProblemListView) , new PropertyMetadata(null , OnProblemSourceChanged)
        );
        public ResolvedProblem[]? ProblemSource {
            get => (ResolvedProblem[]?)GetValue(ProblemSourceProperty);
            set => SetValue(ProblemSourceProperty , value);
        }
        private static void OnProblemSourceChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            ((ProblemListView)d).ApplySearch();
        }

        int prevIndex = 0;
        bool ascending = true;
        public ProblemListView()
        {
            this.InitializeComponent();
            ReloadCurrentUser();
        }

        //현재 유저의 맞은/실패한 문제 (색칠용)
        string? loadedUserHandle = null;
        DateTime loadedUserDownloadTime = default;
        HashSet<int> acceptedProblems = [];
        HashSet<int> failedProblems = [];
        /// <returns>현재 유저 정보가 바뀌었으면 true</returns>
        private bool ReloadCurrentUser()
        {
            ResolvedUser? user = Configuration.Config.CurrentUser is string handle ? Database.Users?.FindById(handle) : null;
            DateTime downloadTime = user?.LastDownloadTime ?? default;
            if (user?.Handle == loadedUserHandle && downloadTime == loadedUserDownloadTime)
                return false;

            loadedUserHandle = user?.Handle;
            loadedUserDownloadTime = downloadTime;
            acceptedProblems = new(user?.AcceptProblems ?? []);
            failedProblems = new(user?.FailedProblems ?? []);
            return true;
        }

        private void ProblemListView_Loaded(object sender , RoutedEventArgs e)
        {
            //설정에서 현재 유저가 바뀌었거나 다시 다운로드했을 수 있다.
            if (ReloadCurrentUser())
            {
                ProblemsListView.ItemsSource = null;
                PushProblems(Cache);
            }
        }

        readonly Comparer<ResolvedProblem>[] SortOptions = [
            Comparer<ResolvedProblem>.Create((a , b) => a.ProblemId.CompareTo(b.ProblemId)),
            Comparer<ResolvedProblem>.Create((a,b) => a.Level.CompareTo(b.Level)),
            Comparer<ResolvedProblem>.Create((a , b) => a.GetTitle().CompareTo(b.GetTitle())),
            Comparer<ResolvedProblem>.Create((a, b) => a.AcceptedUserCount.CompareTo(b.AcceptedUserCount))
        ];

        private ResolvedProblem[] Cache = [];
        private void UpdateProblems(ResolvedProblem[] list)
        {
            Cache = list;
            if (SortBySelector.SelectedItem == null)
                return;
            if (SortBySelector.SelectedItem.TabIndex == 4)
                Random.Shared.Shuffle(list);
            else
                Array.Sort(list , SortOptions[SortBySelector.SelectedItem.TabIndex]);

            PushProblems(list);
        }
        private void PushProblems(ResolvedProblem[] list)
        {
            if (ProblemsListView == null)
                return;
            //수만 개를 하나씩 Add하면 그만큼 CollectionChanged가 발생하므로 목록을 통째로 교체한다.
            //같은 배열을 다시 넣으면 갱신되지 않으므로 항상 새 배열을 만든다.
            ProblemsListView.ItemsSource = ascending ? list.ToArray() : list.AsEnumerable().Reverse().ToArray();
        }
        private void SortBySelector_SelectionChanged(SelectorBar sender , SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == null)
                return;
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
            UpdateProblems(Cache);
        }

        readonly SolidColorBrush GreenColor = new("#009f6b".ToColor());
        readonly SolidColorBrush RedColor = new("#e74c3c".ToColor());
        readonly SolidColorBrush DefaultColor = new(Microsoft.UI.Colors.DarkSlateGray);

        private void ProblemIdTextBlock_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            if (args.NewValue is not ResolvedProblem info)
                return;

            //ListView는 항목 컨테이너를 재활용하므로 이전 색이 남지 않게 항상 다시 지정한다.
            var me = (TextBlock)sender;
            if (acceptedProblems.Contains(info.ProblemId))
                me.Foreground = GreenColor;
            else if (failedProblems.Contains(info.ProblemId))
                me.Foreground = RedColor;
            else
                me.Foreground = DefaultColor;
        }

        private void BookmarkButton_Click(object sender , RoutedEventArgs e)
        {
            var me = (ToggleButton)sender;
            if (ProblemsListView.SelectedItem == null)
                return;

            int id = ((ResolvedProblem)ProblemsListView.SelectedItem).ProblemId;
            if (Database.Bookmarks.FindById(id) != null)
            {
                me.IsChecked = false;
                Database.Bookmarks.Delete(id);
            } else
            {
                me.IsChecked = true;
                Database.Bookmarks.Upsert(new ResolvedBookmark(id));
            }
        }

        private void OpenButton_Click(object sender , RoutedEventArgs e)
        {
            if (ProblemsListView.SelectedItem == null) 
                return;

            _ = Launcher.LaunchUriAsync(new("https://noj.am/" + ((ResolvedProblem)ProblemsListView.SelectedItem).ProblemId));
        }

        private void ProblemsListView_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is not ResolvedProblem problem)
            {
                ProblemBookmarkButton.IsEnabled = false;
                ProblemOpenInOfflineButton.IsEnabled = false;
                ProblemOpenToBrowserButton.IsEnabled = false;

                ProblemDetailText.Text = "Select a problem.";
                return;
            }

            ProblemBookmarkButton.IsEnabled = true;
            ProblemOpenInOfflineButton.IsEnabled = true;
            ProblemOpenToBrowserButton.IsEnabled = true;

            ProblemBookmarkButton.IsChecked = Database.Bookmarks.FindById(problem.ProblemId) != null;

            ProblemDetailText.Text = $"Average tried count: {problem.AverageTries}, Accepted user count: {problem.AcceptedUserCount}";
        }

        private void ProblemSearchTextBox_TextChanged(object sender , TextChangedEventArgs e)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            if (ProblemSearchTextBox == null || ProblemsListView == null)
                return;
            ReloadCurrentUser();
            ResolvedProblem[] source = ProblemSource ?? [];
            string search = ProblemSearchTextBox.Text;
            //정렬이 원본 배열을 바꾸지 않도록 항상 복사본을 넘긴다.
            UpdateProblems(search.Length == 0 ? (ResolvedProblem[])source.Clone() : source.Where(p => p.IsMatching(search)).ToArray());
        }
    }
}
