using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resolved.Scripts;

public class DownloadProgress
{
    public DownloadProgress(out Action<double> update)
    {
        update = (percent) => {
            ChangeProgress?.Invoke(percent);
            if (percent >= 100.0)
            {
                Finished?.Invoke();
            }
        };
    }

    public Action<double>? ChangeProgress { get; set; } = null;
    public Action? Finished { get; set; } = null;
}
