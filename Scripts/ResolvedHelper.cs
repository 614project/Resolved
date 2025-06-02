using System;
using Windows.System;

namespace Resolved.Scripts;

static class ResolvedHelper
{
    public static void OpenToBrowser(this string url)
    {
        _ = Launcher.LaunchUriAsync(new Uri(url));
    }
}
