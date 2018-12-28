using Prism.Mvvm;

namespace TenekDownloader.link.model
{
	public class LinkInfo : BindableBase
	{
		private bool _backToQueue;
		private string _downloadLink;
		private string _downloadLocation;
		private string _fileName;
		private double? _fileSize;
		private bool _isFileExists;
		private string _orignalLink;
		private string _serviceName;

		public bool BackToQueue
		{
			get => _backToQueue;
			set => SetProperty(ref _backToQueue, value);
		}

		public string OrignalLink
		{
			get => _orignalLink;
			set => SetProperty(ref _orignalLink, value);
		}

		public string DownloadLink
		{
			get => _downloadLink;
			set => SetProperty(ref _downloadLink, value);
		}

		public string ServiceName
		{
			get => _serviceName;
			set => SetProperty(ref _serviceName, value);
		}

		public double? FileSize
		{
			get => _fileSize;
			set => SetProperty(ref _fileSize, value);
		}

		public string FileName
		{
			get => _fileName;
			set => SetProperty(ref _fileName, value);
		}

		public bool IsFileExists
		{
			get => _isFileExists;
			set => SetProperty(ref _isFileExists, value);
		}

		public string DownloadLocation
		{
			get => _downloadLocation;
			set => SetProperty(ref _downloadLocation, value);
		}
	}
}