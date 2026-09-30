using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Resolved.Scripts;
using System.Linq;

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

        //이 페이지는 캐시되므로, 문제 데이터가 바뀌었을 때만 다시 읽는다. (다운로드, 삭제 등)
        private int loadedProblemsVersion = -1;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (loadedProblemsVersion == ResolvedInfo.ProblemsVersion)
                return;
            loadedProblemsVersion = ResolvedInfo.ProblemsVersion;
            ProblemList.ProblemSource = Database.Problems.FindAll().ToArray();
        }
    }
}
