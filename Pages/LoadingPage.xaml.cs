using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadingPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };
        TimeSpan time;
        public LoadingPage()
        {
            this.InitializeComponent();
            SolvedInfo.OnLodingEvent += this.Solved_OnLodingEvent;
            timer.Tick += this.Timer_Tick;
            timer.Start();
            time = DateTime.Now.TimeOfDay;
        }

        private void Solved_OnLodingEvent(object? sender , string e)
        {
            message = e;
        }

        private string message = string.Empty;
        private void Timer_Tick(object? sender , object e)
        {
            Status.Text = $"Please wait, {message}... ({(DateTime.Now.TimeOfDay - time).ToString(@"m\:ss")})";
        }

        private void Grid_Loaded(object sender , RoutedEventArgs e)
        {
            Task.Run(() => {
                Configuration.Load();
                SolvedInfo.Load();

                DispatcherQueue.TryEnqueue(() => {
                    Configuration.BackdropUpdate();
                    MainWindow.SelectorBar.SelectedItem = MainWindow.SelectorBar.Items.First();
                    MainWindow.SelectorBar.IsEnabled = true;
                    App.MainWindow.Activate();
                });
            });
        }
    }
}
