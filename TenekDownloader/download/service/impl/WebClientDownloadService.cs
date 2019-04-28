using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using TenekDownloader.download.model;
using TenekDownloader.link;

namespace TenekDownloader.download.service
{
	public class WebClientDownloadService : AbstractDownloadService
	{
		public override void Download()
		{
			if (!DownloadQueue.Any() || !Downloading) return;
			using (var webClient = new WebClient())
			{
				var entity = DownloadQueue.Dequeue();
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

				webClient.DownloadFileCompleted += DownloadFileCompleted(entity, downloadLocation);
				webClient.DownloadProgressChanged += (sender, e) => entity.DownloadPercent = e.ProgressPercentage;
				webClient.DownloadFileAsync(new Uri(entity.LinkInfo.DownloadLink), downloadLocation);
				webClient.Proxy = null;
			}
		}

		private AsyncCompletedEventHandler DownloadFileCompleted(DownloadEntity entity, string downloadLocation)
		{
			return (sender, e) =>
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
					CheckIfFileIsDownloadedSuccessful(downloadLocation, entity);
					ExtractArchive(entity);
				}

				AfterDownload();
			};
		}
	}
}