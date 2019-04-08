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
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Download(object o = null)
		{
			if (o == null) return;
			using (var webClient = new WebClient())
			{
                log.Debug("Enter Download()");
                var entity = o as DownloadEntity;
                log.Debug("Entry: " + entity.ToString());
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
				webClient.DownloadProgressChanged += (sender, e) =>
                {
                    log.Debug(
                        $"{{{entity.LinkInfo.FileName} ==> {e.BytesReceived}}}");
                    entity.DownloadPercent = e.ProgressPercentage;
                    entity.ByteReceived=e.BytesReceived;
                };
                webClient.Proxy = null;
                log.Debug($"Start downloading {entity.LinkInfo.FileName}");
                webClient.DownloadFile(new Uri(entity.LinkInfo.DownloadLink), downloadLocation);
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