using AcNET.Problem;
using LiteDB;
using System.Linq;

namespace Resolved.Collections;

public class ResolvedProblem : SolvedTaggedProblem
{
    [BsonId]
    public int Id { get => ProblemId; set => ProblemId = value; }

    public ResolvedProblem() { }
    public ResolvedProblem(SolvedTaggedProblem problem)
    {
        this.ProblemId = problem.ProblemId;
        this.TitleKo = problem.TitleKo ?? string.Empty;
        this.Titles = problem.Titles;
        this.IsSolvable = problem.IsSolvable;
        this.IsPartial = problem.IsPartial;
        this.AcceptedUserCount = problem.AcceptedUserCount;
        this.Level = problem.Level;
        this.VotedUserCount = problem.VotedUserCount;
        this.Sprout = problem.Sprout;
        this.GivesNoRating = problem.GivesNoRating;
        this.IsLevelLocked = problem.IsLevelLocked;
        this.AverageTries = problem.AverageTries;
        this.Official = problem.Official;
        this.Tags = problem.Tags;
    }

    public string GetTitle()
    {
        return ((string?)TitleKo) ?? Titles?.Select(t => t.Title).FirstOrDefault(string.Empty) ?? string.Empty;
    }
    public bool IsMatching(string pattern)
    {
        return (TitleKo?.Contains(pattern) ?? false) || Titles.Any(x => x.LanguageDisplayName.Contains(pattern)) || ProblemId.ToString().Contains(pattern);
    }
}
