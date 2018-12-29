using System;
using System.Windows.Input;
using FileDownloader;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using TenekDownloader.link.model;

namespace TenekDownloader.download.model
{
	public class DownloadEntity : BindableBase
	{
		public const string Http = "http://";
		public const string Https = "https://";
		private int _downloadPercent;
		[JsonIgnore]
		private IFileDownloader _fileDownloader;
		private LinkInfo _linkInfo;
		private DownloadStatus _status = DownloadStatus.Waiting;

		public DownloadEntity(string link) : this()
		{
			LinkInfo = new LinkInfo
			{
				OrignalLink = link,
				ServiceName = GetService(link)
			};
		}

		public DownloadEntity()
		{
			PauseDownload = new DelegateCommand(PauseDownloadCommand);
			ResumeDownload = new DelegateCommand(ResumeDownloadCommand);
		}

		public ICommand PauseDownload { get; }
		public ICommand ResumeDownload { get; }

		public LinkInfo LinkInfo
		{
			get => _linkInfo;
			set => SetProperty(ref _linkInfo, value);
		}

		public DownloadStatus Status
		{
			get => _status;
			set => SetProperty(ref _status, value);
		}

		public int DownloadPercent
		{
			get => _downloadPercent;
			set => SetProperty(ref _downloadPercent, value);
		}

		[JsonIgnore]
		public IFileDownloader FileDownloaderObj
		{
			get => _fileDownloader;
			set
			{
				SetProperty(ref _fileDownloader, value);
				if (_fileDownloader != null)
				{
					_fileDownloader.DownloadProgressChanged += DownloadProgressChanged;
					_fileDownloader.DownloadFileCompleted += (sender, args) =>
					{
						switch (args.State)
						{
							case CompletedState.Succeeded:
								Status = DownloadStatus.Completed;
								break;
							case CompletedState.Canceled:
								Status = DownloadStatus.Canceled;
								break;
							case CompletedState.Failed:
								Status = DownloadStatus.Error;
								break;
						}
					};
				}
			}
		}

		[JsonIgnore] public DownloadGroup DownloadGroup { get; set; }

		public string GroupName => DownloadGroup?.Name;
		public Action<object> AfterDownload { get; set; }

		public Action<object> StartDownload { get; set; }

		public int ExtractProgress => DownloadGroup.ExtractProgress;

		private void PauseDownloadCommand()
		{
			_fileDownloader.CancelDownloadAsync();
		}

		private void ResumeDownloadCommand()
		{
			_fileDownloader.DownloadFileAsync(new Uri(_linkInfo.DownloadLink), _linkInfo.DownloadLocation);
		}

		private void DownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
		{
			var percent = args.BytesReceived / (double) args.TotalBytesToReceive;
			DownloadPercent = (int) (percent * 100);
		}

		private static string GetService(string link)
		{
			var result = string.Empty;
			if (link == null) throw new ArgumentNullException();

			if (link.ToLower().Contains(Http))
				result = link.Replace(Http, string.Empty);
			else if (link.ToLower().Contains(Https)) result = link.Replace(Https, string.Empty);

			var tab = result.Remove(result.IndexOf('/')).Split('.');
			result = string.Empty;
			foreach (var s in tab)
				if (s.Length > result.Length)
					result = s;

			return result;
		}
	}
}