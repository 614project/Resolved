using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using static Resolved.Scripts.JsonManager;

namespace Resolved.Scripts;

class Configuration
{
    /// <summary>
    /// 0. none, 1. acrylic, 2. mica, 3. mica alt
    /// </summary>
    [JsonProperty("backdrop")]
    public int Backdrop { get; set; } = 3;
    [JsonProperty("currentUser")]
    public string? CurrentUser { get; set; } = null;

    public static Configuration Config => _conf;
    private static Configuration _conf = new();
    public static void Load()
    {
        TryRead(ref _conf , "config.json");
    }
    public static void Save()
    {
        try
        {
            Write(Config , "config.json");
        } catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save configuration: {ex}");
        }
    }

    public static void BackdropUpdate()
    {
        App.MainWindow.SystemBackdrop = Config.Backdrop switch {
            1 => new DesktopAcrylicBackdrop(),
            2 => new MicaBackdrop(),
            3 => new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt },
            _ => MainWindow.DefaultBackdrop
        };
    }
}
