using AcNET.User;
using LiteDB;
using Resolved.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Resolved.Collections;

public class ResolvedUser : INotifyPropertyChanged
{
    public ResolvedUser()
    {
        User = new();
        Debug.WriteLine("Created ResolvedUser instance without parameters.");
    }
    public ResolvedUser(SolvedSocialUser user) { this.User = user; }

    [BsonId]
    public string Handle { get => User.Handle; set => User.Handle = value; }

    public SolvedSocialUser User { get; set; }
    public SolvedAdditionalInformation Additional { get; set; } = null!;
    public List<int> AcceptProblems { get; set; } = [];
    public List<int> FailedProblems { get; set; } = [];
    public DateTime LastDownloadTime { get; set; }

    public string LastDownloadMessage
    {
        get => _lastDownMsg; 
        set 
        { 
            if (_lastDownMsg == value) return;
            _lastDownMsg = value;
            PropertyChanged?.Invoke(this, new(nameof(LastDownloadMessage))); 
            PropertyChanged?.Invoke(this, new(nameof(CanDownload))); 
        }

    }
    private string _lastDownMsg = "no downloaded yet.";


    public string Bio => User.Bio;
    public string MaxStreakText => $"Max {User.MaxStreak} day streak";
    public string ClassText => $"Class {User.Class}";
    public string TierAndRatingText => $"{User.GetTierName} {User.Rating}";
    public string GetTierColor => User.GetTierColor ?? "#000000";
    public bool CanDownload => downloadTask == null;
    public bool IsDownloaded => LastDownloadTime != DateTime.MinValue;
    public string LastTimeText => LastDownloadTime.ToString(@"yyyy\-MM\-dd HH\:mm\:ss");

    public async void StartDownload()
    {
        LastDownloadMessage = "downloading...";
        var ret = await DownloadAsync();
        if (ret != null)
        {
            LastDownloadMessage = "download failed.";
            Debug.WriteLine(ret.Message);
        }
        else
        {
            LastDownloadMessage = LastTimeText;
        }
    }

    private Task<Exception?>? downloadTask = null;
    private async Task<Exception?> DownloadAsync()
    {
        string handle = User.Handle;
        DateTime startDownload = DateTime.Now;
        Exception? ex;
        //추가 정보
        (var addition, ex) = await ResolvedInfo.API.GetUserAdditionalInfoAsync(handle);
        if (addition == null)
            return ex;
        this.Additional = addition;
        //맞은 문제
        (var query, ex) = await ResolvedInfo.API.GetSearchProblemAsync($"s@{handle}");
        if (query == null)
            return ex;
        this.AcceptProblems = query.Items.Select(q => q.ProblemId).ToList();
        //실패한 문제
        (var query2, ex) = await ResolvedInfo.API.GetSearchProblemAsync($"t@{handle} -s@{handle}");
        if (query2 == null)
            return ex;
        this.FailedProblems = query2.Items.Select(q => q.ProblemId).ToList();
        //성공!
        LastDownloadTime = startDownload;
        return null;
    }

#region Support INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged = null;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
#endregion

    public static readonly ResolvedUser Empty = new() { User = new() { Handle = "(Empty)" , Bio = "No solved.ac user" } };
}