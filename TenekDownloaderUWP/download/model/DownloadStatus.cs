﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenekDownloaderUWP.download.model
{
    public enum DownloadStatus
    {
        Downloading,
        Error,
        NotFound,
        Waiting,
        NotDownloading,
        Preparing,
        Completed,
        Canceled
    }
}
