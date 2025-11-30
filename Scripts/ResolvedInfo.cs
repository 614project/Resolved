using AcNET;
using AcNET.Problem;
using AcNET.Site;
using Resolved.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.RemoteSystems;
using static Resolved.Scripts.JsonManager;

namespace Resolved.Scripts;

static class ResolvedInfo
{
    public static SolvedAPI API { get; } = new();
    public static SolvedStats Stats = new();

    public static DateTime? GetLastWriteTime()
    {
        string file = Path.Combine(SaveFolder , "problems.json");
        if (File.Exists(file))
            return File.GetLastWriteTime(file);
        return null;
    }

    public static event EventHandler<string>? OnLoadingEvent = null;
    public static void Load()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            return;
        }
        OnLoadingEvent?.Invoke(null, "Loading problems");
        TryRead(ref Stats , "stats.json");
    }
    public static void ProblemsSave()
    {
        Write(Stats , "stats.json");
    }
    public static event EventHandler<double>? OnProgressChanged = null;
    public static event EventHandler<Exception?>? OnDownloadEnd = null;
    public async static void Download()
    {
        try
        {
            //준비
            OnProgressChanged?.Invoke(null , 0);
            SolvedStats stat = (await API.GetSiteStatsAsync()).GetResultOrThrow();
            //비동기
            var classTask = DownloadClassis();
            var problemTask = DownloadProblems(stat);
            await Task.WhenAll(classTask, problemTask);
            //저장
            ProblemsSave();
            OnDownloadEnd?.Invoke(null , null);
        } catch (Exception ex)
        {
            OnDownloadEnd?.Invoke(null , ex);
        }
    }

    private static async Task DownloadProblems(SolvedStats stat)
    {
        int counting = 0;
        for (int id = 1000 ; counting < stat.ProblemCount ; id += 100)
        {
            foreach(var problem in (await API.GetProblemListAsync(string.Join(',' , Enumerable.Range(id , 100)))).GetResultOrThrow())
            {
                ResolvedDatabase.Problems.Upsert(new ResolvedProblem(problem));
                counting++;
            }
            OnProgressChanged?.Invoke(null , counting / (double)stat.ProblemCount * 100d);
            Thread.Sleep(1);
        }
    }

    private static async Task<T> GetQueryOrThrow<T>(Func<Task<SolvedResult<T>>> queryFunction, int attemptCount = 3) {
        T? result;
        Exception? ex = null;

        for (int attempt = 0; attempt < attemptCount; attempt++) {
            (result, ex) = await queryFunction();
            if (result is not null) return result;
        }
        throw ex ?? throw new ResolvedException("Failed to query.");
    }
    private static async Task DownloadClassis()
    {
        List<SolvedClassInfo> downloaded = await GetQueryOrThrow(API.GetClassListAsync);

        for (int i = 0; i < downloaded.Count; i++) {
            SolvedClassInfo info = downloaded[i];
            for (int trial = 0; trial < 3; trial++) {
                try
                {
                    int[] full = (await GetQueryOrThrow(() => API.GetSearchProblemAsync($"in_class:{info.Class}"))).Items.Select(p => p.ProblemId).ToArray();
                    int[] essential = (await GetQueryOrThrow(() => API.GetSearchProblemAsync($"in_class_essentials:{info.Class}"))).Items.Select(p => p.ProblemId).ToArray();

                    ResolvedClass latest = new(i + 1, full, essential, info);
                    ResolvedDatabase.Classis.Upsert(latest);
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to download class {info.Class} (trial {trial + 1}): {ex.Message}");
                    continue;
                }
            }
        }
    }

    public static void RemoveProblems()
    {
        ResolvedDatabase.Problems.DeleteAll();
        ResolvedDatabase.Classis.DeleteAll();
        ResolvedDatabase.Bookmarks.DeleteAll();

        Stats = new();
        TryDelete("stats.json");
    }
    public static void RemoveBookmarks()
    {
        ResolvedDatabase.Bookmarks.DeleteAll();
    }
    public static void RemoveUsers()
    {
        ResolvedDatabase.Users.DeleteAll();
        ResolvedConfiguration.Config.currentUser = null;
    }
}