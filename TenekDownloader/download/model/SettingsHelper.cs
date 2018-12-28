using Prism.Mvvm;
using TenekDownloader.Properties;
using TenekDownloader.view.model;

namespace TenekDownloader.download.model
{
	public class SettingsHelper : BindableBase
	{
		private static Settings Settings => Settings.Default;

		public string DownloadPath
		{
			set
			{
				Settings.DownloadLocation = value;
				Settings.Save();
				var downloadLocation = Settings.DownloadLocation;
				SetProperty(ref downloadLocation, value);
			}
			get => Settings.DownloadLocation;
		}

		public string SevenZipLibraryLocation
		{
			get => Settings.SevenZipDll;
			set
			{
				Settings.SevenZipDll = value;
				Settings.Save();
				var defaultSevenZipDll = Settings.SevenZipDll;
				SetProperty(ref defaultSevenZipDll, value);
			}
		}

		public bool AutoDownload
		{
			get => Settings.AutoDownload;
			set
			{
				Settings.AutoDownload = value;
				var settingsAutoDownload = Settings.AutoDownload;
				SetProperty(ref settingsAutoDownload, value);
				Settings.Save();
			}
		}

		public ColumnVisible ColumnVisible
		{
			get => Settings.ColumnVisiblity ?? new ColumnVisible();
			set
			{
				Settings.ColumnVisiblity = value;
				Settings.Save();
				var settingsColumnVisiblity = Settings.ColumnVisiblity;
				SetProperty(ref settingsColumnVisiblity, value);
			}
		}
	}
}