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
            string escapedHandle = Uri.EscapeDataString(handle);
            DateTime startDownload = DateTime.Now;
            Exception? ex;
            //유저 기본 정보
            (var user, ex) = await ResolvedInfo.API.GetUserAsync(escapedHandle).ConfigureAwait(false);
            if (user == null)
                return ex ?? new Exception("Failed to download user information.");
            //추가 정보
            (var addition, ex) = await ResolvedInfo.API.GetUserAdditionalInfoAsync(escapedHandle).ConfigureAwait(false);
            if (addition == null)
                return ex ?? new Exception("Failed to download additional information.");
            //맞은 문제 (페이지네이션으로 전체 조회)
            List<int> accepted = await ResolvedInfo.SearchProblemIdsAsync($"s@{handle}").ConfigureAwait(false);
            //실패한 문제 (페이지네이션으로 전체 조회)
            List<int> failed = await ResolvedInfo.SearchProblemIdsAsync($"t@{handle} -s@{handle}").ConfigureAwait(false);
            //성공!
            this.User = user;
            this.Additional = addition;
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
