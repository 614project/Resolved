using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved;
using Resolved.Scripts;
using System.Diagnostics;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BookmarkPage : Page
    {
        public BookmarkPage()
        {
            this.InitializeComponent();
        }

        private void ProblemList_Loaded(object sender , RoutedEventArgs e)
        {
            ProblemList.ProblemSource = SolvedInfo.Bookmarks.Select(x => SolvedInfo.Problems[x]).ToArray();
            //ProblemList.UpdateProblems(SolvedInfo.Bookmarks.Select(x => SolvedInfo.Problems[x]).ToArray());
        }
    }
}
