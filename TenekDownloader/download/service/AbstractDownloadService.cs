using System.Collections.Generic;
using System.IO;
using System.Linq;
using TenekDownloader.download.model;
using TenekDownloader.util;

namespace TenekDownloader.download.service
{
	public abstract class AbstractDownloadService
	{
		public static Queue<DownloadEntity> DownloadQueue = new Queue<DownloadEntity>();
		public static bool Downloading { get; set; }
		public abstract void Download();

		protected void ExtractArchive(DownloadEntity entity)
		{
			if (!entity.DownloadGroup.IsAutoExtracting) return;
			var allDownloaded = true;
			foreach (var downloadGroupEntity in entity.DownloadGroup.Entities)
				allDownloaded = allDownloaded && downloadGroupEntity.Status == DownloadStatus.Completed;

			if (!allDownloaded) return;
			if (!entity.DownloadGroup.IsMoreThanOneArchive)
			{
				var file = entity.DownloadGroup.Entities.OrderBy(e => e.LinkInfo.FileName).First().LinkInfo
					.DownloadLocation;
				ArchiveUtil.UnpackArchive(file, Directory.GetParent(file).FullName,
					(sender, args) => entity.DownloadGroup.ExtractProgress = args.PercentDone);
			}
		}

		protected string ProcessDownloadLocation(DownloadEntity entity)
		{
			var downloadLocation = entity.DownloadGroup.DownloadLocation;
			if (!string.IsNullOrEmpty(entity.GroupName)) downloadLocation += "\\" + entity.GroupName;

			if (!Directory.Exists(downloadLocation)) Directory.CreateDirectory(downloadLocation);
			downloadLocation += "\\" + entity.LinkInfo.FileName;

			return downloadLocation;
		}


		protected virtual void CheckIfFileIsDownloadedSuccesful(string downloadLocation, DownloadEntity entity)
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

		protected virtual void AfterDownload()
		{
			Download();
		}
	}
}