using LiteDB;
using Resolved.Collections;
using System.IO;

namespace Resolved.Scripts;

public static class Database
{
    static readonly LiteDatabase database;
    static Database()
    {
        SaveFilePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path , "resolved.db");
        database = new(SaveFilePath);
        Problems = database.GetCollection<ResolvedProblem>("problems");
        Users = database.GetCollection<ResolvedUser>("users");
        Bookmarks = database.GetCollection<ResolvedBookmark>("bookmarks");
        Classis = database.GetCollection<ResolvedClass>("classis");
    }

    public static string SaveFilePath { get; }
    public static ILiteCollection<ResolvedProblem> Problems { get; }
    public static ILiteCollection<ResolvedUser> Users { get; }
    public static ILiteCollection<ResolvedBookmark> Bookmarks { get; }
    public static ILiteCollection<ResolvedClass> Classis { get; }
}
