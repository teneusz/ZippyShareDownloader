using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TenekDownloader.download.model;
using TenekDownloader.link;
using TenekDownloader.util;

namespace TenekDownloader.download
{
    public static class DownloadService
    {
        public static Queue<DownloadEntity> DownloadQueue = new Queue<DownloadEntity>();
        public static bool Downloading { get; set; }

        public static void Download()
        {
            if (!DownloadQueue.Any() || !Downloading) return;
            using (var webClient = new WebClient())
            {
                var entity = DownloadQueue.Dequeue();
                entity.Status = DownloadStatus.Preparing;
                var interpreter = ServicesEnum.ValueOf(entity.LinkInfo.ServiceName).CreateInstace();
                interpreter.ProcessLink(entity.LinkInfo?.OrignalLink);
                entity.LinkInfo = interpreter.LinkInfo;
                entity.Status = DownloadStatus.Downloading;
                if (!entity.LinkInfo.IsFileExists)
                {
                    AfterDownload();
                    return;
                }


                var downloadLocation = entity.LinkInfo.DownloadLocation =
                    ProcessDownloadLocation(entity);

                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        entity.Status = DownloadStatus.Error;
                        entity.DownloadPercent = 0;
                    }
                    else if (e.Cancelled)
                    {
                        entity.Status = DownloadStatus.Canceled;
                        entity.DownloadPercent = 0;
                    }
                    else
                    {
                        CheckIfFileIsDownloadedSuccesful(downloadLocation, entity);
                        ExtractArchive(entity);
                    }

                    AfterDownload();
                };

                webClient.DownloadProgressChanged += (sender, e) => entity.DownloadPercent = e.ProgressPercentage;
                webClient.DownloadFileAsync(new Uri(entity.LinkInfo.DownloadLink), downloadLocation);
                webClient.Proxy = null;
            }
        }

        private static void ExtractArchive(DownloadEntity entity)
        {
            if (!entity.DownloadGroup.IsAutoExtracting) return;
            var allDownloaded = true;
            foreach (var downloadGroupEntity in entity.DownloadGroup.Entities)
            {
                allDownloaded = allDownloaded && downloadGroupEntity.Status == DownloadStatus.Completed;
            }

            if (!allDownloaded) return;
            if (!entity.DownloadGroup.IsMoreThanOneArchive)
            {
                var file = entity.DownloadGroup.Entities.OrderBy(e => e.LinkInfo.FileName).First().LinkInfo
                    .DownloadLocation;
                ArchiveUtil.UnpackArchive(file, Directory.GetParent(file).FullName, (sender, args) => entity.DownloadGroup.ExtractProgress = args.PercentDone);
            }
        }

        private static string ProcessDownloadLocation(DownloadEntity entity)
        {
            var downloadLocation = entity.DownloadGroup.DownloadLocation;
            if (!string.IsNullOrEmpty(entity.GroupName)) downloadLocation += "\\" + entity.GroupName;

            if (!Directory.Exists(downloadLocation)) Directory.CreateDirectory(downloadLocation);
            downloadLocation += "\\" + entity.LinkInfo.FileName;

            return downloadLocation;
        }


        private static void CheckIfFileIsDownloadedSuccesful(string downloadLocation, DownloadEntity entity)
        {
            if (FileUtil.CheckFileType(downloadLocation))
            {
                entity.Status = DownloadStatus.Completed;
                entity.DownloadPercent = 100;
            }
            else
            {
                entity.Status = DownloadStatus.Preparing;
                entity.DownloadPercent = 0;
                entity.LinkInfo.DownloadLink = null;
            }
        }

        private static void AfterDownload()
        {
            Download();
        }
    }
}