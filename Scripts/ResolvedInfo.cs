using AcNET;
using AcNET.Problem;
using AcNET.Site;
using Resolved.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Resolved.Scripts.JsonManager;

namespace Resolved.Scripts;

static class ResolvedInfo
{
    /// <summary>
    /// solved.ac API v4 클라이언트입니다. 설정(또는 환경 변수)에 클라이언트 ID/시크릿이 없으면
    /// <see cref="MissingCredentialsException"/>을 던집니다.
    /// </summary>
    public static SolvedAPI API {
        get {
            lock (apiLock)
            {
                if (api != null)
                    return api;
                (string? id , string? secret) = GetCredentials();
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret))
                    throw new MissingCredentialsException();
                return api = new SolvedAPI(id , secret);
            }
        }
    }
    private static SolvedAPI? api;
    private static readonly object apiLock = new();

    /// <summary>
    /// solved.ac API 자격 증명이 준비되어 있는지 확인합니다.
    /// </summary>
    public static bool HasCredentials {
        get {
            (string? id , string? secret) = GetCredentials();
            return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(secret);
        }
    }

    /// <summary>
    /// 설정의 자격 증명이 우선이며, 비어 있으면 환경 변수(SOLVEDAC_CLIENT_ID, SOLVEDAC_CLIENT_SECRET)를 사용합니다.
    /// </summary>
    private static (string? id, string? secret) GetCredentials()
    {
        string? id = Configuration.Config.ClientId;
        string? secret = Configuration.Config.ClientSecret;
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret))
        {
            id = Environment.GetEnvironmentVariable("SOLVEDAC_CLIENT_ID");
            secret = Environment.GetEnvironmentVariable("SOLVEDAC_CLIENT_SECRET");
        }
        return (id?.Trim(), secret?.Trim());
    }

    /// <summary>
    /// 자격 증명이 바뀌었을 때 호출하면 다음 요청부터 새 자격 증명을 사용합니다.
    /// </summary>
    public static void ResetAPI()
    {
        lock (apiLock)
        {
            api?.Dispose();
            api = null;
        }
    }

    private const int MaxRateLimitRetries = 10;
    /// <summary>
    /// API를 호출하고, 요청 한도(429)에 걸리면 Retry-After 만큼 기다린 뒤 다시 시도합니다.
    /// (solved.ac API v4는 토큰 버킷 방식이라 문제를 모두 받는 동안 한도에 걸릴 수 있다)
    /// </summary>
    public static async Task<T> CallAsync<T>(Func<SolvedAPI , Task<SolvedResult<T>>> request)
    {
        for (int attempt = 0 ; ; attempt++)
        {
            SolvedResult<T> result = await request(API).ConfigureAwait(false);
            if (result.APIException is { IsRateLimited: true } e && attempt < MaxRateLimitRetries)
            {
                TimeSpan wait = e.RetryAfter ?? TimeSpan.FromSeconds(5);
                if (wait < TimeSpan.FromSeconds(1))
                    wait = TimeSpan.FromSeconds(1);
                await Task.Delay(wait).ConfigureAwait(false);
                continue;
            }
            return result.GetResultOrThrow();
        }
    }

    public static SolvedStats Stats = new();

    private const string StatsFile = "stats.json";

    /// <summary>
    /// 문제 데이터가 바뀔 때마다 증가합니다. 캐시된 페이지가 문제 목록을 다시 읽어야 하는지 판단할 때 씁니다.
    /// </summary>
    public static int ProblemsVersion => Volatile.Read(ref _problemsVersion);
    private static int _problemsVersion = 0;

    /// <summary>
    /// 마지막으로 문제 다운로드에 성공한 시각입니다. (stats.json은 다운로드에 성공했을 때만 저장된다)
    /// </summary>
    public static DateTime? GetLastWriteTime()
    {
        string file = Path.Combine(SaveFolder , StatsFile);
        if (File.Exists(file))
            return File.GetLastWriteTime(file);
        return null;
    }

    public static void Load()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            return;
        }
        TryRead(ref Stats , StatsFile);
    }
    public static void SaveStats()
    {
        Write(Stats , StatsFile);
    }
    public static bool IsDownloading { get; private set; } = false;
    public static event EventHandler<double>? OnProgressChanged = null;
    public static event EventHandler<Exception?>? OnDownloadEnd = null;
    public static event EventHandler? OnDataChanged = null;
    public async static void Download()
    {
        if (IsDownloading)
            return;
        IsDownloading = true;
        Exception? error = null;
        try
        {
            //준비
            OnProgressChanged?.Invoke(null , 0);
            SolvedStats stat = await CallAsync(api => api.GetSiteStatsAsync()).ConfigureAwait(false);
            //비동기
            //문제 검색은 요청 비용이 커서 요청 한도를 나눠 쓰지 않도록 차례대로 받는다.
            await DownloadProblems().ConfigureAwait(false);
            await DownloadClasses().ConfigureAwait(false);
            //저장
            Stats = stat;
            SaveStats();
        } catch (Exception ex)
        {
            error = ex;
        }
        IsDownloading = false;
        //실패했더라도 일부는 이미 저장됐을 수 있다.
        Interlocked.Increment(ref _problemsVersion);
        OnDownloadEnd?.Invoke(null , error);
        OnDataChanged?.Invoke(null , EventArgs.Empty);
    }

    /// <summary>
    /// solved.ac 문제 검색 API는 페이지당 최대 50개만 반환하므로,
    /// 모든 페이지를 순회해 조건에 맞는 전체 문제 ID 목록을 가져옵니다.
    /// </summary>
    public static async Task<List<int>> SearchProblemIdsAsync(string query)
    {
        List<int> ids = [];
        for (int page = 1 ; ; page++)
        {
            var result = await CallAsync(api => api.GetSearchProblemAsync(query , page)).ConfigureAwait(false);
            if (result.Problems.Count == 0)
                break;
            ids.AddRange(result.Problems.Select(p => p.ProblemId));
            if (ids.Count >= result.Total)
                break;
        }
        return ids;
    }

    /// <summary>
    /// solved.ac API v4에는 여러 문제를 한 번에 조회하는 API가 없으므로,
    /// 조건 없는 문제 검색(번호 순, 페이지당 50개)을 끝까지 넘기며 모든 문제를 받습니다.
    /// </summary>
    private static async Task DownloadProblems()
    {
        int counting = 0;
        for (int page = 1 ; ; page++)
        {
            var result = await CallAsync(api => api.GetSearchProblemAsync(null , page)).ConfigureAwait(false);
            if (result.Problems.Count == 0)
                break;
            List<ResolvedProblem> problems = result.Problems.Select(problem => new ResolvedProblem(problem)).ToList();
            Database.Problems.Upsert(problems);
            counting += problems.Count;
            if (result.Total > 0)
                OnProgressChanged?.Invoke(null , Math.Min(counting / (double)result.Total * 100d , 100d));
            if (counting >= result.Total)
                break;
        }
    }

    /// <summary>
    /// solved.ac 클래스는 1~10까지 있습니다. (API v4에는 클래스 목록 API가 없다)
    /// </summary>
    public const int ClassCount = 10;
    private static async Task DownloadClasses()
    {
        for (int classId = 1 ; classId <= ClassCount ; classId++)
        {
            int[] full = (await SearchProblemIdsAsync($"in_class:{classId}").ConfigureAwait(false)).ToArray();
            int[] essential = (await SearchProblemIdsAsync($"in_class_essentials:{classId}").ConfigureAwait(false)).ToArray();

            ResolvedClass latest = new(classId , full , essential);
            Database.Classes.Upsert(latest);
        }
    }

    public static void RemoveProblems()
    {
        Database.Problems.DeleteAll();
        Database.Classes.DeleteAll();
        Database.Bookmarks.DeleteAll();

        Stats = new();
        TryDelete(StatsFile);
        Interlocked.Increment(ref _problemsVersion);
        OnDataChanged?.Invoke(null , EventArgs.Empty);
    }
    public static void RemoveBookmarks()
    {
        Database.Bookmarks.DeleteAll();
        OnDataChanged?.Invoke(null , EventArgs.Empty);
    }
    public static void RemoveUsers()
    {
        Database.Users.DeleteAll();
        Configuration.Config.CurrentUser = null;
        Configuration.Save();
        OnDataChanged?.Invoke(null , EventArgs.Empty);
    }
}

/// <summary>
/// solved.ac API v4 클라이언트 ID/시크릿이 설정되지 않았을 때 발생합니다.
/// </summary>
public class MissingCredentialsException : InvalidOperationException
{
    public MissingCredentialsException()
        : base("solved.ac API client ID and secret are not set. Enter them in Settings.") { }
}
