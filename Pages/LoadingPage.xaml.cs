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
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Pages;

//public sealed partial class LoadingPage : Page
//{
//    DispatcherTimer timer = new DispatcherTimer {
//        Interval = TimeSpan.FromSeconds(1)
//    };
//    TimeSpan time;
//    public LoadingPage()
//    {
//        this.InitializeComponent();
//        ResolvedInfo.OnLoadingEvent += this.Solved_OnLoadingEvent;
//        timer.Tick += this.Timer_Tick;
//        timer.Start();
//        time = DateTime.Now.TimeOfDay;
//    }

//    private void Solved_OnLoadingEvent(object? sender , string e)
//    {
//        message = e;

//        DispatcherQueue.TryEnqueue(() => {
//            StatusTextBox.Text = $"Please wait, {message}... ({(DateTime.Now.TimeOfDay - time).ToString(@"m\:ss")})";
//        });
//    }


//    private string message = string.Empty;
//    private void Timer_Tick(object? sender , object e)
//    {
//        if (StatusTextBox.Visibility != Visibility.Visible)
//        {
//            timer.Stop();
//            return;
//        }
//        StatusTextBox.Text = $"Please wait, {message}... ({(DateTime.Now.TimeOfDay - time).ToString(@"m\:ss")})";
//    }

//    private void Grid_Loaded(object sender , RoutedEventArgs e)
//    {
//        Task.Run(() => {
//            message = "Loading database";
//            Database.Problems.FindAll();

//            message = "Loading configuration";
//            Configuration.Load();
//            ResolvedInfo.Load();

//            message = "Almost done";
//            DispatcherQueue.TryEnqueue(() => {
//                Configuration.BackdropUpdate();
//                MainWindow.SelectorBar.SelectedItem = MainWindow.SelectorBar.Items.First();
//                MainWindow.SelectorBar.IsEnabled = true;
//                App.MainWindow.Activate();
//            });
//        });
//    }
//}

public sealed partial class LoadingPage : Page
{
    DispatcherTimer timer = new DispatcherTimer {
        Interval = TimeSpan.FromSeconds(1)
    };

    TimeSpan startTime;
    private string _message = "Starting...";
    private readonly object _lock = new();

    public LoadingPage()
    {
        this.InitializeComponent();
        timer.Tick += Timer_Tick;
        timer.Start();
        startTime = DateTime.Now.TimeOfDay;
    }

    private void Timer_Tick(object? sender , object e)
    {
        if (StatusTextBox.Visibility != Visibility.Visible)
        {
            timer.Stop();
            return;
        }

        string messageSnapshot;
        lock (_lock)
        {
            messageSnapshot = _message;
        }

        var elapsed = (DateTime.Now.TimeOfDay - startTime).ToString(@"m\:ss");
        StatusTextBox.Text = $"Please wait, {messageSnapshot}... ({elapsed})";
    }

    private void UpdateMessage(string newMessage)
    {
        Debug.WriteLine(_message = newMessage);
    }

    private void Grid_Loaded(object sender , RoutedEventArgs e)
    {
        _ = Task.Run(() => {
            UpdateMessage("Loading database");
            UpdateMessage($"Loading database from '{Path.GetRelativePath("ms-appx:///", Database.SaveFilePath)}'");

            UpdateMessage("Loading configuration");
            Configuration.Load();
            ResolvedInfo.Load();

            UpdateMessage("Almost done");

            DispatcherQueue.TryEnqueue(() => {
                Configuration.BackdropUpdate();
                MainWindow.SelectorBar.SelectedItem = MainWindow.SelectorBar.Items.First();
                MainWindow.SelectorBar.IsEnabled = true;
                App.MainWindow.Activate();
            });
        });
    }
}