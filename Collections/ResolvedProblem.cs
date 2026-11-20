using AcNET.Problem;
using System;
using System.Linq;

namespace Resolved.Collections;

/// <summary>
/// DB에 저장하는 문제입니다. LiteDB의 _id는 <see cref="SolvedProblemRef.ProblemId"/>입니다. (<see cref="Scripts.Database.ConfigureMapper"/> 참고)
/// </summary>
public class ResolvedProblem : SolvedProblem
{
    public ResolvedProblem() { }
    public ResolvedProblem(SolvedProblem problem)
    {
        //SolvedProblemRef
        this.ProblemId = problem.ProblemId;
        this.SubId = problem.SubId;
        this.JudgeIds = problem.JudgeIds;
        this.Level = problem.Level;
        this.EffectiveVoteCount = problem.EffectiveVoteCount;
        this.GivesNoRating = problem.GivesNoRating;
        this.IsStandard = problem.IsStandard;
        //SolvedProblemSummary
        this.Titles = problem.Titles;
        //SolvedProblem
        this.Id = problem.Id;
        this.LevelAverage = problem.LevelAverage;
        this.LevelStdev = problem.LevelStdev;
        this.VotedUserCount = problem.VotedUserCount;
        this.SolvedUserCount = problem.SolvedUserCount;
        this.NoRatingReasons = problem.NoRatingReasons;
        this.NoRatingWarnings = problem.NoRatingWarnings;
        this.Contests = problem.Contests;
        this.Tags = problem.Tags;
        //사용자 토큰에 따라 달라지는 값(UserStatus, ViewerFolders)은 저장하지 않는다.
    }

    /// <summary>
    /// 한국어 제목을 우선으로, 없으면 원문 제목을 돌려줍니다.
    /// </summary>
    public string GetDisplayTitle()
    {
        if (Titles == null || Titles.Count == 0)
            return string.Empty;
        return GetTitle("ko") ?? string.Empty;
    }
    public bool IsMatching(string pattern)
    {
        return (Titles?.Any(x => x.Title?.Contains(pattern , StringComparison.OrdinalIgnoreCase) ?? false) ?? false)
            || ProblemId.ToString().Contains(pattern);
    }
}
