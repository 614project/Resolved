using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Resolved.Pages;
using Resolved.Scripts;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static SystemBackdrop DefaultBackdrop { get; private set; } = null!;
        public static Frame Frame { get; private set; } = null!;
        public static SelectorBar SelectorBar { get; private set; } = null!;

        public MainWindow()
        {
            this.InitializeComponent();
            Frame = this.MainFrame;
            SelectorBar = this.MainSelectorBar;
            this.ExtendsContentIntoTitleBar = true;
            DefaultBackdrop = this.SystemBackdrop;
            this.SetTitleBar(this.TitleBarBorder);
            MainFrame.Navigate(typeof(LoadingPage));

            this.Closed += this.MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender , WindowEventArgs args)
        {
            Configuration.Save();
        }

        private void MainSelectorBar_SelectionChanged(SelectorBar sender , SelectorBarSelectionChangedEventArgs args)
        {
            int? index = sender.SelectedItem?.TabIndex;
            if (index == null || index > 6) return;
            MainFrame.Navigate(index switch {
                0 => typeof(SearchPage),
                //2 => typeof(TestPage),
                5 => typeof(BookmarkPage),
                6 => typeof(SettingPage),
                _ => typeof(NoImplementedPage)
            });
        }
    }
}
