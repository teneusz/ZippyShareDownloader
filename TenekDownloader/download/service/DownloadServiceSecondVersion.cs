using System;
using System.Linq;
using FileDownloader;
using TenekDownloader.download.model;
using TenekDownloader.link;

namespace TenekDownloader.download.service
{
	public class DownloadServiceSecondVersion : AbstractDownloadService
	{
		public override void Download()
		{
			if (!DownloadQueue.Any() || !Downloading) return;
			var entity = DownloadQueue.Dequeue();

			var fileDownloader = entity.FileDownloaderObj ?? new FileDownloader.FileDownloader(new Cache());
			{
				entity.Status = DownloadStatus.Preparing;
				var interpreter = ServicesEnum.ValueOf(entity.LinkInfo.ServiceName).CreateInstace();
				interpreter.ProcessLink(entity.LinkInfo?.OrignalLink);
				entity.LinkInfo = interpreter.LinkInfo;
				if (entity.LinkInfo.BackToQueue)
				{
					entity.Status = DownloadStatus.Waiting;
					DownloadQueue.Enqueue(entity);
					return;
				}

				entity.Status = DownloadStatus.Downloading;
				if (!entity.LinkInfo.IsFileExists)
				{
					AfterDownload();
					return;
				}


				var downloadLocation = entity.LinkInfo.DownloadLocation =
					ProcessDownloadLocation(entity);

				fileDownloader.DownloadFileCompleted += (sender, e) =>
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
						CheckIfFileIsDownloadedSuccesful(downloadLocation, entity);
						ExtractArchive(entity);
					}

					AfterDownload();
				};

				fileDownloader.DownloadProgressChanged += (sender, e) => entity.DownloadPercent = e.ProgressPercentage;
				fileDownloader.DownloadFileAsync(new Uri(entity.LinkInfo.DownloadLink), downloadLocation);
			}
			entity.FileDownloaderObj = fileDownloader;
		}
	}
}