using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using TenekDownloaderUWP.download.model;
using TenekDownloader.util;
using TenekDownloaderUWP.link;

namespace TenekDownloaderUWP.download
{
    public static class DownloadService
    {
        public static Queue<DownloadEntity> DownloadQueue = new Queue<DownloadEntity>();
        public static bool Downloading { get; set; }

        public static async System.Threading.Tasks.Task DownloadAsync()
        {
            if (!DownloadQueue.Any() || !Downloading) return;
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
            var source = new Uri(entity.LinkInfo.DownloadLink);

            var destinationFile = await KnownFolders.PicturesLibrary.CreateFileAsync(entity.LinkInfo.DownloadLocation,
                CreationCollisionOption.ReplaceExisting);

            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(source, destinationFile);
            download.AttachAsync().Progress += (info, progressInfo) =>
            {
                entity.DownloadPercent =
                    (int) ((progressInfo.Progress.BytesReceived / progressInfo.Progress.TotalBytesToReceive) * 100);
            };
            download.AttachAsync().Completed += (info, status) =>
            {
                switch (status)
                {
                    case AsyncStatus.Completed:
                        entity.Status = DownloadStatus.Completed;
                        CheckIfFileIsDownloadedSuccesful(downloadLocation, entity);
                        ExtractArchive(entity);
                        break;
                    case AsyncStatus.Error:
                        entity.Status = DownloadStatus.Error;
                        entity.DownloadPercent = 0;
                        break;
                    case AsyncStatus.Canceled:
                        entity.Status = DownloadStatus.Canceled;
                        break;
                    case AsyncStatus.Started:
                        entity.Status = DownloadStatus.Downloading;
                        break;
                    default:
                        break;
                }
                AfterDownload();
            };
            // Attach progress and completion handlers.
            await download.StartAsync();
            entity.Status = DownloadStatus.Downloading;
//                webClient.DownloadFileCompleted += (sender, e) =>
//                {
//                    if (e.Error != null)
//                    {
//                        entity.Status = DownloadStatus.Error;
//                        entity.DownloadPercent = 0;
//                    }
//                    else if (e.Cancelled)
//                    {
//                        entity.Status = DownloadStatus.Canceled;
//                        entity.DownloadPercent = 0;
//                    }
//                    else
//                    {
//                        CheckIfFileIsDownloadedSuccesful(downloadLocation, entity);
//                        ExtractArchive(entity);
//                    }
//
//                    AfterDownload();
//                };
//
//                webClient.DownloadProgressChanged += (sender, e) => entity.DownloadPercent = e.ProgressPercentage;
//                webClient.DownloadFileAsync(new Uri(entity.LinkInfo.DownloadLink), downloadLocation);
//                webClient.Proxy = null;
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
            if (entity.DownloadGroup.IsMoreThanOneArchive) return;
            var file = entity.DownloadGroup.Entities.OrderBy(e => e.LinkInfo.FileName).First().LinkInfo
                .DownloadLocation;
            ArchiveUtil.UnpackArchive(file, Directory.GetParent(file).FullName,null);
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
            DownloadAsync();
        }
    }
}