using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Collections;
using Resolved.Scripts;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

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
            //ProblemList.UpdateProblems(search.Length == 0 ? SolvedInfo.Problems.Values.ToArray() : SolvedInfo.Problems.Values.Where(p => Matching(p,search)).ToArray());
        }

        public ResolvedProblem[] ProblemSource => Database.Problems.FindAll().ToArray();
    }
}
