using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Resolved.Scripts;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages;

public sealed partial class LoadingPage : Page
{
    DispatcherTimer timer = new DispatcherTimer {
        Interval = TimeSpan.FromSeconds(1)
    };

    TimeSpan startTime;
    //백그라운드 스레드에서 쓰고 UI 스레드에서 읽는다.
    private volatile string _message = "Starting...";

    public LoadingPage()
    {
        this.InitializeComponent();
        timer.Tick += Timer_Tick;
        timer.Start();
        startTime = DateTime.Now.TimeOfDay;
        this.Unloaded += (_ , _) => timer.Stop();
    }

    private void Timer_Tick(object? sender , object e)
    {
        var elapsed = (DateTime.Now.TimeOfDay - startTime).ToString(@"m\:ss");
        StatusTextBox.Text = $"Please wait, {_message}... ({elapsed})";
    }

    private void UpdateMessage(string newMessage)
    {
        Debug.WriteLine(_message = newMessage);
    }

    private void Grid_Loaded(object sender , RoutedEventArgs e)
    {
        _ = Task.Run(() => {
            try
            {
                UpdateMessage($"Loading database from '{Database.SaveFilePath}'");
                Database.Open();

                UpdateMessage("Loading configuration");
                Configuration.Load();
                ResolvedInfo.Load();

                UpdateMessage("Almost done");
            } catch (Exception ex)
            {
                //예외를 삼키면 로딩 화면에서 멈춘 채로 남으므로 화면에 알린다.
                Debug.WriteLine(ex);
                DispatcherQueue.TryEnqueue(() => {
                    timer.Stop();
                    StatusTextBox.Text = $"Failed to start: {ex.Message}";
                });
                return;
            }

            DispatcherQueue.TryEnqueue(() => {
                Configuration.BackdropUpdate();
                MainWindow.SelectorBar.SelectedItem = MainWindow.SelectorBar.Items.First();
                MainWindow.SelectorBar.IsEnabled = true;
                App.MainWindow.Activate();
            });
        });
    }
}
