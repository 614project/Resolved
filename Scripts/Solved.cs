using acNET;
using acNET.Problem;
using acNET.Site;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Pwm;
using Windows.Networking.NetworkOperators;
using static Resolved.Scripts.JsonManager;

namespace Resolved.Scripts;

static class Solved
{
    public static acAPI API { get; } = new();
    public static List<TaggedProblem> Problems = [];
    public static Dictionary<string,SolvedUser> Users = [];
    public static List<(int[] full, int[] essential, ClassInfo info)> Classis = [];
    public static Stats Stats = new();

    public static DateTime? GetLastWriteTime()
    {
        string file = Path.Combine(SaveFolder , "problems.json");
        if (File.Exists(file))
            return File.GetLastWriteTime(file);
        return null;
    }

    public static event EventHandler<string>? OnLodingEvent = null;
    public static void Load()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            return;
        }
        OnLodingEvent?.Invoke(null, "loading problems");
        TryRead(ref Stats , "stats.json");
        TryRead(ref Classis , "classis.json");

        OnLodingEvent?.Invoke(null , $"loading about {Stats.problemCount} problems");
        TryRead(ref Problems , "problems.json");

        OnLodingEvent?.Invoke(null , "loading user data");
        TryRead(ref Users , "users.json");
    }
    public static void ProblemsSave()
    {
        Write(Problems , "problems.json");
        Write(Classis , "classis.json");
        Write(Stats , "stats.json");
    }
    public static void UsersSave()
    {
        Write(Users , "users.json");
    }
    public static event EventHandler<double>? OnProgressChanged = null;
    public static event EventHandler<Exception?>? OnDownloadEnd = null;
    public async static void Download()
    {
        try
        {
            //준비
            OnProgressChanged?.Invoke(null , 0);
            var stat = (await API.GetSiteStatsAsync()).GetResultOrThrow();
            //비동기
            var classTask = DownloadClassis();
            var problemTask = DownloadProblems(stat);

            await Task.WhenAll(classTask, problemTask);
            //저장
            Classis = await classTask;
            Problems = await problemTask;
            ProblemsSave();
            OnDownloadEnd?.Invoke(null , null);
        } catch (Exception ex)
        {
            OnDownloadEnd?.Invoke(null , ex);
        }
    }

    private static async Task<List<TaggedProblem>> DownloadProblems(Stats stat)
    {
        List<TaggedProblem> downloads = new(capacity: (int)stat.problemCount * 2);
        for (int id = 1000 ; downloads.Count < stat.problemCount ; id += 100)
        {
            downloads.AddRange((await API.GetProblemListAsync(string.Join(',' , Enumerable.Range(id , 100)))).GetResultOrThrow());
            OnProgressChanged?.Invoke(null , downloads.Count / (double)stat.problemCount * 100d);
            Thread.Sleep(1);
        }
        return downloads;
    }
    private static async Task<List<(int[], int[], ClassInfo)>> DownloadClassis()
    {
        List<(int[], int[], ClassInfo)> list = new(capacity: 20);
        var informations = (await API.GetClassListAsync()).GetResultOrThrow();
        foreach (var info in informations) {
            int[] full = (await API.GetSearchProblemAsync($"in_class:{info.@class}")).GetResultOrThrow().items.Select(p => p.problemId).ToArray();
            int[] essential = (await API.GetSearchProblemAsync($"in_class_essentials:{info.@class}")).GetResultOrThrow().items.Select(p => p.problemId).ToArray();
            list.Add((full, essential, info));
        }
        return list;
    }

    public static void RemoveProblems()
    {
        Problems.Clear();
        Classis.Clear();
        Stats = new();

        TryDelete("problems.json");
        TryDelete("classis.json");
        TryDelete("stats.json");
    }

    public static void RemoveUsers()
    {
        Users.Clear();
        UsersSave();
    }
}
