using acNET;
using acNET.User;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resolved.Scripts;

public class SolvedUser
{
    public static readonly SolvedUser Empty = new(new() { handle = "Empty" , bio = "no solved.ac user" });

    public RankedUser User { get; set; }
    public AdditionalInformation AdditionalInfo { get; set; } = new();
    public HashSet<int> AccpetProblems { get; set; } = [];
    public HashSet<int> FailedProblems { get; set; } = [];
    public DateTime LastDownloadTime { get; set; } = DateTime.MinValue;
    public string LastDownloadMessage { get; set; } = "no downloaded yet.";

    public SolvedUser(RankedUser user)
    {
        User = user;
    }

    [JsonIgnore]
    public bool IsDownloaded => LastDownloadTime != DateTime.MinValue;
    [JsonIgnore]
    public string Handle => User.handle;
    [JsonIgnore]
    public string Name => AdditionalInfo.name ?? string.Empty;
    [JsonIgnore]
    public string NativeName => AdditionalInfo.nameNative ?? string.Empty;
    [JsonIgnore]
    public string TierAndRating => $"{User.GetTierName} {User.rating}";
    [JsonIgnore]
    public string GetTierColor => User.GetTierColor ?? "#000000";
    [JsonIgnore]
    public string LastTimeText => LastDownloadTime.ToString(@"yyyy\-MM\-dd HH\:mm\:ss");
    [JsonIgnore]
    public string Bio => User.bio;
    [JsonIgnore]
    public bool CanDownload => downloadTask == null;
    public event EventHandler<string>? OnDownloadStatusChanged = null;
    [JsonIgnore]
    public string MaxStreakText => $"Max {User.maxStreak} day streak";
    [JsonIgnore]
    public string ClassText => $"Class {User.@class}";

    private Task<Exception?>? downloadTask = null;
    public async void StartDownload()
    {
        OnDownloadStatusChanged?.Invoke(this ,LastDownloadMessage = "downloading...");
        var ret = await DownloadAsync();
        if (ret != null)
        {
            OnDownloadStatusChanged?.Invoke(this , LastDownloadMessage = "download failed.");
            Debug.WriteLine(ret.Message);
        } else
        {
            OnDownloadStatusChanged?.Invoke(this , LastDownloadMessage = LastTimeText);
        }
    }

    private async Task<Exception?> DownloadAsync()
    {
        string handle = User.handle;
        DateTime startDownload = DateTime.Now;
        Exception? ex;
        //추가 정보
        (var addition, ex) = await SolvedInfo.API.GetUserAdditionalInfoAsync(handle);
        if (addition == null)
            return ex;
        this.AdditionalInfo = addition;
        //맞은 문제
        (var query, ex) = await SolvedInfo.API.GetSearchProblemAsync($"s@{handle}");
        if (query == null)
            return ex;
        this.AccpetProblems = new(query.items.Select(q => q.problemId));
        //실패한 문제
        (var query2, ex) = await SolvedInfo.API.GetSearchProblemAsync($"t@{handle} -s@{handle}");
        if (query2 == null)
            return ex;
        this.FailedProblems = new(query2.items.Select(q => q.problemId));
        //성공!
        LastDownloadTime = startDownload;
        return null;
    }
}
