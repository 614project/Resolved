using LiteDB;
using Resolved.Scripts;

namespace Resolved.Collections;

public record class ResolvedBookmark([property: BsonId] int ProblemId)
{
    /// <summary>
    /// 문제 데이터가 없으면 null입니다.
    /// </summary>
    [BsonIgnore]
    public ResolvedProblem? Problem => Database.Problems.FindById(ProblemId);
}
