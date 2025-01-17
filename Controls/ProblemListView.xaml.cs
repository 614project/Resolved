using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using System.Collections.ObjectModel;
using acNET.Problem;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.WinUI.Helpers;

namespace Resolved.Controls
{
    public sealed partial class ProblemListView : UserControl
    {
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
        public void Update(TaggedProblem[] list)
        {
            Cache = list;
            if (SortBySelector.SelectedItem == null)
                return;
            Problems.Clear();
            if (SortBySelector.SelectedItem.TabIndex == 4)
                Random.Shared.Shuffle(list);
            else
                Array.Sort(list , SortOptions[SortBySelector.SelectedItem.TabIndex]);

            PushProblems(list);
        }
        private void PushProblems(TaggedProblem[] list)
        {
            Problems.Clear();
            if (ascending)
            {
                for (int i = 0 ; i < list.Length ; i++)
                    Problems.Add(list[i]);
            }
            else
            {
                for (int i = list.Length - 1 ; i >= 0 ; i--)
                    Problems.Add(list[i]);
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

        private ObservableCollection<TaggedProblem> Problems { get; set; } = [];

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
    }
}
