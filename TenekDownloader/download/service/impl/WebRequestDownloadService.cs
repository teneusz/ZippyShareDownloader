using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using TenekDownloader.download.model;

namespace TenekDownloader.download.service.impl
{
    public class WebRequestDownloadService : AbstractDownloadService
    {
        public static readonly AbstractDownloadService INSTANCE = new WebClientDownloadService();
        public override void Download()
        {
            if (Downloads.Count > 0)
            {
                foreach (WebRequestClient download in Downloads.ToList())
                {
                    if (download.DownloadEntity.Status != DownloadStatus.Completed)
                    {
                        download.Start();
                    }
                }
            }
        }
    }
}
