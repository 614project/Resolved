using acNET.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Resolved.Scripts;

static class ResolvedHelper
{
    public static void OpenToBrowser(this string url)
    {
        _ = Launcher.LaunchUriAsync(new Uri(url));
    }

    public static bool Matching(this TaggedProblem problem, string pattern)
    {
        return problem.titleKo.Contains(pattern) || problem.titles.Any(x => x.languageDisplayName.Contains(pattern)) || problem.problemId.ToString().Contains(pattern);
    }
}
