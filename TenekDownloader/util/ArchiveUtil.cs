﻿using System;
using System.Threading.Tasks;
using log4net;
using SevenZip;
using TenekDownloader.Properties;

namespace TenekDownloader.util
{
	public static class ArchiveUtil
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ArchiveUtil));

		public static void UnpackArchive(string inFile, string outFile,
			EventHandler<ProgressEventArgs> extractingEventHandler, string password = null)
		{
			var unzipTask = new Task(() =>
			{
				try
				{
					SevenZipBase.SetLibraryPath(Settings.Default.SevenZipDll);
					using (var tmp = string.IsNullOrEmpty(password) ? new SevenZipExtractor(inFile) : new SevenZipExtractor(inFile, password))
					{
						foreach (var data in tmp.ArchiveFileData)
						{
                            tmp.Extracting += extractingEventHandler;
							tmp.ExtractFiles(outFile, data.Index);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Debug("Error while extracting file <" + inFile + ">", ex);
				}
			});

			unzipTask.Start();
		}
	}
}