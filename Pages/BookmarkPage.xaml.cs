using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved;
using Resolved.Collections;
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

        public ResolvedProblem[] ProblemSource => Database.Bookmarks.FindAll().Select(bookmark => bookmark.Problem).ToArray();
    }
}
