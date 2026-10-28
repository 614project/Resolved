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
    public static SolvedAPI API { get; } = new();
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
            SolvedStats stat = (await API.GetSiteStatsAsync()).GetResultOrThrow();
            //비동기
            var classTask = DownloadClasses();
            var problemTask = DownloadProblems(stat);
            await Task.WhenAll(classTask, problemTask);
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
        string escapedQuery = Uri.EscapeDataString(query);
        for (int page = 1 ; ; page++)
        {
            var result = (await API.GetSearchProblemAsync($"{escapedQuery}&page={page}").ConfigureAwait(false)).GetResultOrThrow();
            if (result.Items == null || result.Items.Count == 0)
                break;
            ids.AddRange(result.Items.Select(p => p.ProblemId));
            if (ids.Count >= result.Count)
                break;
        }
        return ids;
    }

    private const int ProblemBatchSize = 100;
    /// <summary>
    /// 문제가 하나도 없는 구간이 이만큼 연속되면 더 이상 문제가 없다고 보고 멈춥니다.
    /// (solved.ac 통계의 문제 수와 실제로 받을 수 있는 문제 수가 다를 때 끝없이 요청하지 않도록)
    /// </summary>
    private const int MaxEmptyBatches = 50;
    private static async Task DownloadProblems(SolvedStats stat)
    {
        int counting = 0;
        int emptyBatches = 0;
        for (int id = 1000 ; counting < stat.ProblemCount && emptyBatches < MaxEmptyBatches ; id += ProblemBatchSize)
        {
            List<ResolvedProblem> problems = (await API.GetProblemListAsync(string.Join(',' , Enumerable.Range(id , ProblemBatchSize))).ConfigureAwait(false))
                .GetResultOrThrow()
                .Select(problem => new ResolvedProblem(problem))
                .ToList();
            if (problems.Count == 0)
            {
                emptyBatches++;
                continue;
            }
            emptyBatches = 0;
            Database.Problems.Upsert(problems);
            counting += problems.Count;
            OnProgressChanged?.Invoke(null , Math.Min(counting / (double)stat.ProblemCount * 100d , 100d));
        }
    }
    private static async Task DownloadClasses()
    {
        List<SolvedClassInfo> downloaded = (await API.GetClassListAsync().ConfigureAwait(false)).GetResultOrThrow();
        for(int i=0 ; i < downloaded.Count ; i++)
        {
            SolvedClassInfo info = downloaded[i];

            int[] full = (await SearchProblemIdsAsync($"in_class:{info.Class}").ConfigureAwait(false)).ToArray();
            int[] essential = (await SearchProblemIdsAsync($"in_class_essentials:{info.Class}").ConfigureAwait(false)).ToArray();

            ResolvedClass latest = new(i + 1 , full , essential , info);
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
