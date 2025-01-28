using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System;
using System.IO;
using static Resolved.Scripts.JsonManager;

namespace Resolved.Scripts;

class Configuration
{
    /// <summary>
    /// 1. acrylic, 2. mica, 3. mica alt
    /// </summary>
    public int backdrop { get; set; } = 3;
    public string? currentUser { get; set; } = null;

    public static Configuration Config => _conf;
    private static Configuration _conf = new();
    public static void Load()
    {
        TryRead(ref _conf , "config.json");
    }
    public static void Save()
    {
        Write(Config , "config.json");
    }

    public static void BackdropUpdate()
    {
        App.MainWindow.SystemBackdrop = Config.backdrop switch {
            1 => new DesktopAcrylicBackdrop(),
            2 => new MicaBackdrop(),
            3 => new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt },
            _ => MainWindow.DefaultBackdrop
        };
    }
    public static SolvedUser? CurrentUser => _conf.currentUser == null ? null : SolvedInfo.Users[_conf.currentUser];
}