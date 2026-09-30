using AcNET.Problem;
using LiteDB;

namespace Resolved.Collections;

public class ResolvedClass
{
    //LiteDB가 역직렬화할 때 사용
    public ResolvedClass() { }
    public ResolvedClass(int classId , int[] entireProblems , int[] essentialProblems , SolvedClassInfo information)
    {
        this.ClassId = classId;
        this.EntireProblems = entireProblems;
        this.EssentialProblems = essentialProblems;
        this.Information = information;
    }

    [BsonId]
    public int ClassId { get; set; }
    public int[] EntireProblems { get; set; } = [];
    public int[] EssentialProblems { get; set; } = [];
    public SolvedClassInfo Information { get; set; } = new();
}
