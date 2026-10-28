using AcNET.Problem;
using AcNET.User;
using LiteDB;
using Resolved.Collections;
using System.IO;

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

        //acNET 모델의 계산 속성은 직렬화 대상이 아니며,
        //빈 날짜 문자열이나 윤년 생일(연도 미지정)에서 예외를 던질 수 있으므로 제외한다.
        mapper.Entity<SolvedSocialUser>()
            .Ignore(x => x.GetExactCoin)
            .Ignore(x => x.GetJoinedAt)
            .Ignore(x => x.GetBannedUntil)
            .Ignore(x => x.GetProUntil)
            .Ignore(x => x.GetTierName)
            .Ignore(x => x.GetTierColor)
            .Ignore(x => x.GetArenaTierName);

        mapper.Entity<SolvedAdditionalInformation>()
            .Ignore(x => x.GetBirthDate);

        mapper.Entity<SolvedProblem>()
            .Ignore(x => x.GetTierName)
            .Ignore(x => x.GetTierColor);

        mapper.Entity<SolvedTaggedProblem>()
            .Ignore(x => x.GetTierName)
            .Ignore(x => x.GetTierColor);

        mapper.Entity<ResolvedProblem>()
            .Ignore(x => x.GetTierName)
            .Ignore(x => x.GetTierColor);
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
