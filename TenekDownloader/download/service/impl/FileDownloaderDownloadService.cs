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
		public override void Download()
		{
			if (!CanDownload()) return;
            new Thread(Download).Start();
            var entity = DownloadQueue.Dequeue();
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
            catch
            {
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

		private static bool IsFileNotExists(DownloadEntity entity)
		{
			return !entity.LinkInfo.IsFileExists;
		}

		private void SetUpEventHandlers(IFileDownloader fileDownloader, DownloadEntity entity, string downloadLocation)
		{
			fileDownloader.DownloadFileCompleted += DownloadFileCompleted(entity, downloadLocation);
			fileDownloader.DownloadProgressChanged += (sender, e) => entity.DownloadPercent = e.ProgressPercentage;
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