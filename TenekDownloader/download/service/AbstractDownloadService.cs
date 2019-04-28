﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TenekDownloader.download.model;
using TenekDownloader.util;

namespace TenekDownloader.download.service
{
    public abstract class AbstractDownloadService
	{
		public static Queue<DownloadEntity> DownloadQueue = new Queue<DownloadEntity>();
		public static List<WebRequestClient> Downloads = new List<WebRequestClient>();
		public static readonly string DoubleBackslash = "\\";
		public static bool Downloading { get; set; }
		
		protected static SettingsHelper SettingsHelper = new SettingsHelper();
		protected static HashSet<DownloadEntity> AlreadyDownloading = new HashSet<DownloadEntity>();

		protected void ExtractArchive(DownloadEntity entity)
        {
			if (!entity.DownloadGroup.IsAutoExtracting) return;
            var allDownloaded = entity.DownloadGroup.Entities.Any(e => e.Status != DownloadStatus.Completed);

            if (!allDownloaded) return;
			var file = entity.DownloadGroup.Entities.OrderBy(e => e.LinkInfo.FileName).First().LinkInfo
				.DownloadLocation;
			ArchiveUtil.UnpackArchive(file, Directory.GetParent(file).FullName,
				(sender, args) => entity.DownloadGroup.ExtractProgress = args.PercentDone, entity.DownloadGroup.ArchivePassword);
		}

		public static string ProcessDownloadLocation(DownloadEntity entity)
		{
			var downloadLocation = entity.DownloadGroup.DownloadLocation;
			if (!string.IsNullOrEmpty(entity.GroupName)) downloadLocation += DoubleBackslash + entity.GroupName;

			if (!Directory.Exists(downloadLocation)) Directory.CreateDirectory(downloadLocation);
			downloadLocation += DoubleBackslash + entity.LinkInfo.FileName;

			return downloadLocation;
		}


		protected virtual void CheckIfFileIsDownloadedSuccessful(string downloadLocation, DownloadEntity entity)
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

        public virtual void Download()
        {
            if (AlreadyDownloading.Count < SettingsHelper.MaxDownloadingCount)
            {
                new Task(Download).Start();
            }
        }

        protected virtual void AfterDownload()
		{
			Download();
		}


        protected static bool CanDownload()
        {
            return DownloadQueue.Any() && Downloading && SettingsHelper.MaxDownloadingCount >= AlreadyDownloading.Count;
        }

        public static int ActiveDownloads
        {
	        get
	        {
		        return Downloads.ToList()
			        .FindAll(e => e.DownloadEntity.Status == DownloadStatus.Downloading || e.DownloadEntity.Status == DownloadStatus.Waiting).Count;
	        }
        }
	}
}