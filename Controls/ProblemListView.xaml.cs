using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using System.Collections.ObjectModel;
using AcNET.Problem;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Diagnostics;
using System.Linq;
using Resolved.Collections;
using Windows.System;

namespace Resolved.Controls
{
    public sealed partial class ProblemListView : UserControl
    {
        public static readonly DependencyProperty ProblemSourceProperty = DependencyProperty.Register(
            "ProblemSource" , typeof(ResolvedProblem[]) , typeof(ProblemListView) , new PropertyMetadata("ProblemSource")
        );
        public ResolvedProblem[] ProblemSource {
            get => (ResolvedProblem[])GetValue(ProblemSourceProperty);
            set => SetValue(ProblemSourceProperty , value);
        }

        ResolvedUser? CurrentUser = null;

        int prevIndex = 0;
        bool ascending = true;
        public ProblemListView()
        {
            this.InitializeComponent();
            if (Configuration.CurrentUser is string handle)
            {
                this.CurrentUser = Database.Users.FindById(handle);
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
            ProblemCollection.Clear();
            if (SortBySelector.SelectedItem.TabIndex == 4)
                Random.Shared.Shuffle(list);
            else
                Array.Sort(list , SortOptions[SortBySelector.SelectedItem.TabIndex]);

            PushProblems(list);
        }
        private void PushProblems(ResolvedProblem[] list)
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

        private ObservableCollection<ResolvedProblem> ProblemCollection { get; set; } = [];

        readonly SolidColorBrush GreenColor = new("#009f6b".ToColor());
        readonly SolidColorBrush RedColor = new("#e74c3c".ToColor());

        private void ProblemIdTextBlock_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        {
            if (CurrentUser == null)
                return;

            var me = (TextBlock)sender;
            var info = me.DataContext as ResolvedProblem;
            if (info == null)
            {
                return;
            }

            if (CurrentUser.AcceptProblems.Contains(info.ProblemId))
            {
                me.Foreground = GreenColor;
            }
            else if (CurrentUser.FailedProblems.Contains(info.ProblemId))
            {
                me.Foreground = RedColor;
            }
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

        //private void BookmarkButton_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        //{
        //    var me = (ToggleButton)sender;
        //    if (ProblemsListView.SelectedItem is not ResolvedProblem problem)
        //    {
        //        me.IsEnabled = false;
        //        return;
        //    }

        //    me.IsEnabled = true;
        //    bool include = Database.Bookmarks.FindById(problem.ProblemId) != null;
        //    me.IsChecked = include;
        //    //if (include)
        //    //{
        //    //    me.Content = "In Bookmarks";
        //    //} else
        //    //{
        //    //    me.Content = "Add Bookmarks";
        //    //}
        //}

        //private void OpenButton_DataContextChanged(FrameworkElement sender , DataContextChangedEventArgs args)
        //{
        //    var me = (Button)sender;
        //    if (ProblemsListView.SelectedItem is not ResolvedProblem problem)
        //    {
        //        me.IsEnabled = false;
        //        return;
        //    }

        //    me.IsEnabled = true;
        //}

        private void ProblemsListView_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            var me = (ListView)sender;
            Debug.WriteLine($"selection changed: {e.RemovedItems.FirstOrDefault("null").GetType()} -> {e.AddedItems.FirstOrDefault("null").GetType()}");

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
            string search = ProblemSearchTextBox.Text;
            var filter = ProblemSource.Where(p => p.IsMatching(search)).ToArray();
            UpdateProblems(filter);
        }

        private void ProblemListViewLoading(FrameworkElement sender , object args)
        {
            Cache = ProblemSource;
        }
    }
}
