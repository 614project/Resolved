using AcNET.Problem;
using LiteDB;

namespace Resolved.Collections;

public record ResolvedClass([property: BsonId] int ClassId, int[] EntireProblems , int[] EssentialProblems , SolvedClassInfo Information)
{

}