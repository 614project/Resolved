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
    public ResolvedUser(SolvedUser user) { this.User = user; }

    [BsonId]
    public string Handle { get => User.Handle; set => User.Handle = value; }

    public SolvedUser User { get; set; }
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
    public string Bio => User.Bio ?? string.Empty;
    [BsonIgnore]
    public string MaxStreakText => $"Max {User.MaxStreak} day streak";
    [BsonIgnore]
    public string ClassText => $"Class {User.Class}";
    [BsonIgnore]
    public string TierAndRatingText => $"{User.GetTierName} {User.Rating}";
    [BsonIgnore]
    public string GetTierColor => User.GetTierColor;
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
        PropertyChanged?.Invoke(this , new(nameof(CanDownload)));
        OnDownloadStatusChanged?.Invoke(this , message);
    }

    private void NotifyUserPropertiesChanged()
    {
        PropertyChanged?.Invoke(this , new(nameof(TierAndRatingText)));
        PropertyChanged?.Invoke(this , new(nameof(GetTierColor)));
        PropertyChanged?.Invoke(this , new(nameof(Bio)));
        PropertyChanged?.Invoke(this , new(nameof(ClassText)));
        PropertyChanged?.Invoke(this , new(nameof(MaxStreakText)));
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
            NotifyUserPropertiesChanged();
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
            //유저 정보 (API v4에서는 추가 정보가 Metadata로 함께 온다)
            SolvedUser user = await ResolvedInfo.CallAsync(api => api.GetUserAsync(handle)).ConfigureAwait(false);
            //맞은 문제 (페이지네이션으로 전체 조회)
            List<int> accepted = await ResolvedInfo.SearchProblemIdsAsync($"s@{handle}").ConfigureAwait(false);
            //실패한 문제 (페이지네이션으로 전체 조회)
            List<int> failed = await ResolvedInfo.SearchProblemIdsAsync($"t@{handle} -s@{handle}").ConfigureAwait(false);
            //성공!
            this.User = user;
            this.AcceptProblems = accepted;
            this.FailedProblems = failed;
            LastDownloadTime = startDownload;
            return null;
        } catch (Exception ex)
        {
            return ex;
        }
    }
}
