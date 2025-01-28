using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using System.Collections.ObjectModel;
using acNET.Problem;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Diagnostics;
using System.Linq;

namespace Resolved.Controls
{
    public sealed partial class ProblemListView : UserControl
    {
        public static readonly DependencyProperty ProblemSourceProperty = DependencyProperty.Register(
            "ProblemSource" , typeof(TaggedProblem[]) , typeof(ProblemListView) , new PropertyMetadata("ProblemSource")
        );
        public TaggedProblem[] ProblemSource {
            get => (TaggedProblem[])GetValue(ProblemSourceProperty);
            set => SetValue(ProblemSourceProperty , value);
        }

        SolvedUser? CurrentUser;

        int prevIndex = 0;
        bool ascending = true;
        public ProblemListView()
        {
            this.InitializeComponent();
            this.CurrentUser = Configuration.CurrentUser;
        }

        readonly Comparer<TaggedProblem>[] SortOptions = [
            Comparer<TaggedProblem>.Create((a , b) => a.problemId.CompareTo(b.problemId)),
            Comparer<TaggedProblem>.Create((a,b) => a.level.CompareTo(b.level)),
            Comparer<TaggedProblem>.Create((a , b) => a.titleKo.CompareTo(b.titleKo)),
            Comparer<TaggedProblem>.Create((a, b) => a.acceptedUserCount.CompareTo(b.acceptedUserCount))
        ];

        private TaggedProblem[] Cache = [];
        private void UpdateProblems(TaggedProblem[] list)
        {
            Cache = list;
            if (SortBySelector.SelectedItem == null)
                return;
            ProblemCollection.Clear();
            if (SortBySelector.SelectedItem.TabIndex == 4)
                Random.Shared.Shuffle(list);
            else
                Array.Sort(list , SortOptions[SortBySelector.SelectedItem.TabIndex]);

            PushProblems(list);
        }
        private void PushProblems(TaggedProblem[] list)
        {
            ProblemCollection.Clear();
            if (ascending)
            {
                for (int i = 0 ; i < list.Length ; i++)
                    ProblemCollection.Add(list[i]);
            }
            else
            {
                for (int i = list.Length - 1 ; i >= 0 ; i--)
                    ProblemCollection.Add(list[i]);
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
            UpdateProblems(Cache);
        }

        private ObservableCollection<TaggedProblem> ProblemCollection { get; set; } = [];

        readonly SolidColorBrush GreenColor = new("#009f6b".ToColor());
        readonly SolidColorBrush RedColor = new("#e74c3c".ToColor());

        private void ProblemIdTextBlock_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            if (CurrentUser == null)
                return;

            var me = (TextBlock)sender;
            var info = me.DataContext as TaggedProblem;
            if (info == null)
            {
                return;
            }

            if (CurrentUser.AccpetProblems.Contains(info.problemId))
            {
                me.Foreground = GreenColor;
            }
            else if (CurrentUser.FailedProblems.Contains(info.problemId))
            {
                me.Foreground = RedColor;
            }
        }

        private void BookmarkButton_Click(object sender , RoutedEventArgs e)
        {
            var me = (ToggleButton)sender;
            if (ProblemsListView.SelectedItem == null)
                return;

            int id = ((TaggedProblem)ProblemsListView.SelectedItem).problemId;
            if (SolvedInfo.Bookmarks.Contains(id))
            {
                me.IsChecked = false;
                SolvedInfo.Bookmarks.Remove(id);
            } else
            {
                me.IsChecked = true;
                SolvedInfo.Bookmarks.Add(id);
            }
        }

        private void OpenButton_Click(object sender , RoutedEventArgs e)
        {
            //var me = (Button)sender;
            if (ProblemsListView.SelectedItem == null) 
                return;

            ("https://noj.am/" + ((TaggedProblem)ProblemsListView.SelectedItem).problemId).OpenToBrowser();
        }

        private void BookmarkButton_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            var me = (ToggleButton)sender;
            if (ProblemsListView.SelectedItem is not TaggedProblem problem)
            {
                me.IsEnabled = false;
                return;
            }

            me.IsEnabled = true;
            bool include = SolvedInfo.Bookmarks.Contains(problem.problemId);
            me.IsChecked = include;
            //if (include)
            //{
            //    me.Content = "In Bookmarks";
            //} else
            //{
            //    me.Content = "Add Bookmarks";
            //}
        }

        private void OpenButton_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            var me = (Button)sender;
            if (ProblemsListView.SelectedItem is not TaggedProblem problem)
            {
                me.IsEnabled = false;
                return;
            }

            me.IsEnabled = true;
        }

        private void ProblemsListView_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            var me = (ListView)sender;
            Debug.WriteLine($"selection changed: {e.RemovedItems.FirstOrDefault("null").GetType()} -> {e.AddedItems.FirstOrDefault("null").GetType()}");

            if (e.AddedItems.FirstOrDefault() is not TaggedProblem problem)
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

            ProblemBookmarkButton.IsChecked = SolvedInfo.Bookmarks.Contains(problem.problemId);

            ProblemDetailText.Text = $"Average tried count: {problem.averageTries}, Accepted user count: {problem.acceptedUserCount}";
        }

        private void ProblemSearchTextBox_TextChanged(object sender , TextChangedEventArgs e)
        {
            string search = ProblemSearchTextBox.Text;
            var filter = ProblemSource.Where(p => p.Matching(search)).ToArray();
            UpdateProblems(filter);
        }
    }
}
