using acNET.Problem;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Resolved.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender , AutoSuggestBoxTextChangedEventArgs args)
        {
            string[] search = [sender.Text];
            ProblemList.Update(search.Length == 0 ? Solved.Problems.ToArray() : Solved.Problems.Where(p => Matching(p,search)).ToArray());
        }

        private void ProblemList_Loaded(object sender , RoutedEventArgs e)
        {
            ProblemList.Update(Solved.Problems.ToArray());
        }

        private bool Matching(TaggedProblem problem , string[] keywords)
        {
            for (int i = 0 ; i < keywords.Length ; i++)
            {
                if (problem.titleKo.Contains(keywords[i]) || problem.titles.Any(t => t.languageDisplayName.Contains(keywords[i])) || problem.problemId.ToString() == keywords[i])
                    return true;
            }
            return false;
        }
    }
}
