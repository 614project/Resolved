using LiteDB;
using Resolved.Collections;
using System.IO;

namespace Resolved.Scripts;

public static class Database
{
    static LiteDatabase? database;

    public static string SaveFilePath { get; } = Path.Combine(JsonManager.SaveFolder , "resolved.db");
    public static ILiteCollection<ResolvedProblem> Problems { get; private set; } = null!;
    public static ILiteCollection<ResolvedUser> Users { get; private set; } = null!;
    public static ILiteCollection<ResolvedBookmark> Bookmarks { get; private set; } = null!;
    public static ILiteCollection<ResolvedClass> Classes { get; private set; } = null!;

    /// <summary>
    /// DB를 엽니다. 앱 시작 시(LoadingPage) 다른 곳에서 사용하기 전에 한 번 호출해야 합니다.
    /// </summary>
    public static void Open()
    {
        if (database != null)
            return;
        database = new(SaveFilePath);
        Problems = database.GetCollection<ResolvedProblem>("problems");
        Users = database.GetCollection<ResolvedUser>("users");
        Bookmarks = database.GetCollection<ResolvedBookmark>("bookmarks");
        //기존 DB와의 호환을 위해 컬렉션 이름은 그대로 둔다.
        Classes = database.GetCollection<ResolvedClass>("classis");
    }
    public static void Close()
    {
        database?.Dispose();
        database = null;
    }
}
