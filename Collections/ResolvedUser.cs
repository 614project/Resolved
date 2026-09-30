using AcNET.User;
using LiteDB;
using Resolved.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Resolved.Collections;

public class ResolvedUser : INotifyPropertyChanged
{
    public ResolvedUser()
    {
        User = new();
    }
    public ResolvedUser(SolvedSocialUser user) { this.User = user; }

    [BsonId]
    public string Handle { get => User.Handle; set => User.Handle = value; }

    public SolvedSocialUser User { get; set; }
    public SolvedAdditionalInformation? Additional { get; set; }
    public List<int> AcceptProblems { get; set; } = [];
    public List<int> FailedProblems { get; set; } = [];
    public DateTime LastDownloadTime { get; set; }

    private string _lastDownloadMessage = "no downloaded yet.";
    public string LastDownloadMessage {
        get => _lastDownloadMessage;
        set {
            if (_lastDownloadMessage == value)
                return;
            _lastDownloadMessage = value;
            PropertyChanged?.Invoke(this , new(nameof(LastDownloadMessage)));
        }
    }

    [BsonIgnore]
    public string Bio => User.Bio;
    [BsonIgnore]
    public string MaxStreakText => $"Max {User.MaxStreak} day streak";
    [BsonIgnore]
    public string ClassText => $"Class {User.Class}";
    [BsonIgnore]
    public string TierAndRatingText => $"{User.GetTierName} {User.Rating}";
    [BsonIgnore]
    public string GetTierColor => User.GetTierColor ?? "#000000";
    [BsonIgnore]
    public bool CanDownload => downloadTask == null;
    [BsonIgnore]
    public bool IsDownloaded => LastDownloadTime != DateTime.MinValue;
    [BsonIgnore]
    public string LastTimeText => LastDownloadTime.ToString(@"yyyy\-MM\-dd HH\:mm\:ss");

    public event PropertyChangedEventHandler? PropertyChanged = null;
    public event EventHandler<string>? OnDownloadStatusChanged = null;
    private void ChangeDownloadStatus(string message)
    {
        LastDownloadMessage = message;
        OnDownloadStatusChanged?.Invoke(this , message);
    }

    public async void StartDownload()
    {
        if (!CanDownload)
            return;
        Task<Exception?> task = downloadTask = DownloadAsync();
        ChangeDownloadStatus("downloading...");
        Exception? ret = await task;
        downloadTask = null;
        if (ret == null)
        {
            LastDownloadMessage = LastTimeText;
            //DB에서 이미 삭제된 유저라면 다시 추가하지 않는다.
            try
            {
                Database.Users.Update(this);
            } catch (Exception ex)
            {
                ret = ex;
            }
        }
        if (ret != null)
        {
            ChangeDownloadStatus("download failed.");
            Debug.WriteLine(ret.Message);
        }
        else
        {
            ChangeDownloadStatus(LastTimeText);
        }
    }

    private Task<Exception?>? downloadTask = null;
    private async Task<Exception?> DownloadAsync()
    {
        try
        {
            string handle = User.Handle;
            DateTime startDownload = DateTime.Now;
            Exception? ex;
            //추가 정보
            (var addition, ex) = await ResolvedInfo.API.GetUserAdditionalInfoAsync(handle);
            if (addition == null)
                return ex ?? new Exception("Failed to download additional information.");
            //맞은 문제
            (var query, ex) = await ResolvedInfo.API.GetSearchProblemAsync($"s@{handle}");
            if (query == null)
                return ex ?? new Exception("Failed to download accepted problems.");
            //실패한 문제
            (var query2, ex) = await ResolvedInfo.API.GetSearchProblemAsync($"t@{handle} -s@{handle}");
            if (query2 == null)
                return ex ?? new Exception("Failed to download failed problems.");
            //성공!
            this.Additional = addition;
            this.AcceptProblems = query.Items.Select(q => q.ProblemId).ToList();
            this.FailedProblems = query2.Items.Select(q => q.ProblemId).ToList();
            LastDownloadTime = startDownload;
            return null;
        } catch (Exception ex)
        {
            return ex;
        }
    }
}
