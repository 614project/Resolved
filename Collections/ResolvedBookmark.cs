using LiteDB;
using Resolved.Scripts;

namespace Resolved.Collections;

public record class ResolvedBookmark([property: BsonId] int ProblemId)
{
    public ResolvedProblem Problem => Database.Problems.FindById(ProblemId);
}