using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileDownloader;
using TenekDownloader.download.model;
using TenekDownloader.link;
using TenekDownloader.link.model;

namespace TenekDownloader.download.service.impl
{
    public class FileDownloaderDownloadService : AbstractDownloadService
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Download(object state = null)
        {
            log.Debug("Enter Download()");
            //            if (!CanDownload()) return;
            if (!(state is DownloadEntity entity))
            {
                return;
            }
            log.Debug("Entry: " + entity.ToString());
            try
            {
                var fileDownloaded = entity.FileDownloaderObj ?? new FileDownloader.FileDownloader(new Cache());

                entity.Status = DownloadStatus.Preparing;
                entity.LinkInfo = ProcessLinkInfo(entity);
                if (entity.LinkInfo.BackToQueue)
                {
                    entity.Status = DownloadStatus.Waiting;
                    DownloadQueue.Enqueue(entity);
                    return;
                }

                entity.Status = DownloadStatus.Downloading;
                AlreadyDownloading.Add(entity);
                if (IsFileNotExists(entity))
                {
                    entity.Status = DownloadStatus.NotFound;
                    AlreadyDownloading.Remove(entity);
                    AfterDownload();
                    return;
                }

                entity.LinkInfo.DownloadLocation = ProcessDownloadLocation(entity);
                SetUpEventHandlers(fileDownloaded, entity, entity.LinkInfo.DownloadLocation);
                entity.FileDownloaderObj = fileDownloaded;
            }
            catch (Exception ex)
            {
                log.Debug("Error while downloading", ex);
                entity.Status = DownloadStatus.Error;
            }
        }

        private static LinkInfo ProcessLinkInfo(DownloadEntity entity)
        {
            var interpreter = InitInterpreter(entity);
            return interpreter.LinkInfo;
        }

        private static ILinkInterpreter InitInterpreter(DownloadEntity entity)
        {
            var interpreter = ServicesEnum.ValueOf(entity.LinkInfo.ServiceName).CreateInstace();
            interpreter.ProcessLink(entity.LinkInfo?.OrignalLink);
            return interpreter;
        }

        private void SetUpEventHandlers(IFileDownloader fileDownloader, DownloadEntity entity, string downloadLocation)
        {
            fileDownloader.DownloadFileCompleted += DownloadFileCompleted(entity, downloadLocation);
            fileDownloader.DownloadProgressChanged += (sender, e) =>
            {
                entity.DownloadPercent = e.ProgressPercentage;
                entity.ByteReceived = e.BytesReceived;
                log.Debug(
                    $"{{{entity.LinkInfo.FileName} ==> {e.BytesReceived}}}");
            };
            fileDownloader.DownloadFileAsync(new Uri(entity.LinkInfo.DownloadLink), downloadLocation);
        }

        private EventHandler<DownloadFileCompletedArgs> DownloadFileCompleted(DownloadEntity entity,
            string downloadLocation)
        {
            return (sender, e) =>
            {
                if (e.Error != null)
                {
                    entity.Status = DownloadStatus.Error;
                    entity.DownloadPercent = 0;
                }
                else if (e.State == CompletedState.Canceled)
                {
                    entity.Status = DownloadStatus.Canceled;
                    entity.DownloadPercent = 0;
                }
                else
                {
                    CheckIfFileIsDownloadedSuccessful(downloadLocation, entity);
                    ExtractArchive(entity);
                }

                AlreadyDownloading.Remove(entity);

                AfterDownload();
            };
        }
    }
}