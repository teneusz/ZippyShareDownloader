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
		public const string HTTP = "http://";
		public const string HTTPS = "https://";
		private int _downloadPercent;
		[JsonIgnore] private IFileDownloader _fileDownloader;
		private LinkInfo _linkInfo;
		private DownloadStatus _status = DownloadStatus.Waiting;
        private long _byteReceived;

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
					_fileDownloader.DownloadFileCompleted += DownloadFileCompleted;
				}
			}
		}

		private void DownloadFileCompleted(object sender, DownloadFileCompletedArgs args)
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
		}

		[JsonIgnore] public DownloadGroup DownloadGroup { get; set; }

		public string GroupName => DownloadGroup?.Name;
		public Action<object> AfterDownload { get; set; }

		public Action<object> StartDownload { get; set; }

		public int ExtractProgress => DownloadGroup.ExtractProgress;

        [JsonIgnore]
        public long ByteReceived
        {
            get => _byteReceived;
            set => SetProperty(ref _byteReceived ,value);
        }

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

			if (link == null) throw new ArgumentNullException();

				var result = string.Empty;

			if (HasHttp(link))
				result = RemoveHttp(link);
			else if (HasHttps(link))
				result = RemoveHttps(link);

			return ExtractServiceName(result);
		}

		private static string ExtractServiceName(string link)
		{
			var tab = link.Remove(link.IndexOf('/')).Split('.');
			link = string.Empty;
			foreach (var s in tab)
				if (s.Length > link.Length)
					link = s;

			return link;
		}

		private static string RemoveHttps(string link)
		{
			return link.Replace(HTTPS, string.Empty);
		}

		private static string RemoveHttp(string link)
		{
			return link.Replace(HTTP, string.Empty);
		}

		private static bool HasHttps(string link)
		{
			return link.ToLower().Contains(HTTPS);
		}

		private static bool HasHttp(string link)
		{
			return link.ToLower().Contains(HTTP);
		}
    }
}