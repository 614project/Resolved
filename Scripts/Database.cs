using AcNET;
using LiteDB;
using Resolved.Collections;
using System.IO;
using System.Reflection;

namespace Resolved.Scripts;

public static class Database
{
    static LiteDatabase? database;
    static bool mapperConfigured;

    public static string SaveFilePath { get; } = Path.Combine(JsonManager.SaveFolder , "resolved.db");
    public static ILiteCollection<ResolvedProblem> Problems { get; private set; } = null!;
    public static ILiteCollection<ResolvedUser> Users { get; private set; } = null!;
    public static ILiteCollection<ResolvedBookmark> Bookmarks { get; private set; } = null!;
    public static ILiteCollection<ResolvedClass> Classes { get; private set; } = null!;

    public static void ConfigureMapper(BsonMapper? mapper = null)
    {
        mapper ??= BsonMapper.Global;

        //acNET 모델의 계산 속성(GetTierName, GetTierColor, SolvedBan.GetUntil 등 setter가 없는 속성)은
        //직렬화 대상이 아니며 값에 따라 예외를 던질 수 있으므로 모두 제외한다.
        Assembly acnet = typeof(SolvedAPI).Assembly;
        var previous = mapper.ResolveMember;
        mapper.ResolveMember = (type , member , memberMapper) => {
            previous?.Invoke(type , member , memberMapper);
            if (member is PropertyInfo property && property.SetMethod is not { IsPublic: true } && property.DeclaringType?.Assembly == acnet)
                memberMapper.FieldName = null;
        };

        //문제 번호를 _id로 쓴다. (SolvedProblem.Id는 solved.ac 내부 ID이므로 일반 필드로 저장된다)
        mapper.Entity<ResolvedProblem>()
            .Id(x => x.ProblemId , false);
    }

    /// <summary>
    /// DB를 엽니다. 앱 시작 시(LoadingPage) 다른 곳에서 사용하기 전에 한 번 호출해야 합니다.
    /// </summary>
    public static void Open()
    {
        if (database != null)
            return;
        if (!mapperConfigured)
        {
            ConfigureMapper();
            mapperConfigured = true;
        }
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
