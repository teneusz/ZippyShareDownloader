using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZippyShareDownloader.Model
{    public enum DownloadStatus
    {
        Preparing,
        NotDownloading,
        Downloading,
        Completed,
        Canceled,
        Error,
        NotFound
    }
}
