using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using TenekDownloader.Properties;

namespace TenekDownloader.download.model
{
	public class DownloadGroup : BindableBase
	{
		private string _downloadLocation = "./";

		private ObservableCollection<DownloadEntity> _entities = new ObservableCollection<DownloadEntity>();
		private int _extractProgress;
		private bool _isAutoExtracting;
		private bool _isMoreThanOneArchive;
		private bool _isSerialized = true;
		private List<string> _links;
		private string _name;
		private bool? _unpack;
		public bool ManyArchives;

		public DownloadGroup()
		{
		}

		public DownloadGroup(List<string> links, string name, bool? unpack)
		{
			_links = links;
			_name = name;
			IsAutoExtracting = unpack ?? false;

			_downloadLocation = Settings.Default.DownloadLocation;
			foreach (var link in links)
			{
				var unescapeLink = link.Replace("\r", "");
				if (string.IsNullOrEmpty(unescapeLink)) continue;
				Entities.Add(new DownloadEntity(unescapeLink) {DownloadGroup = this});
			}
		}

		public bool IsSerialized
		{
			get => _isSerialized;
			set => SetProperty(ref _isSerialized, value);
		}

		public int ExtractProgress
		{
			get => _extractProgress;
			set => SetProperty(ref _extractProgress, value);
		}

		public ObservableCollection<DownloadEntity> Entities
		{
			get => _entities;
			set => SetProperty(ref _entities, value);
		}

		public string DownloadLocation
		{
			get => _downloadLocation;
			set => SetProperty(ref _downloadLocation, value);
		}

		public bool IsAutoExtracting
		{
			get => _isAutoExtracting;
			set => SetProperty(ref _isAutoExtracting, value);
		}

		public bool IsMoreThanOneArchive
		{
			get => _isMoreThanOneArchive;
			set => SetProperty(ref _isMoreThanOneArchive, value);
		}

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public DownloadEntity this[int index]
		{
			get => Entities[index];
			set => Entities[index] = value;
		}
	}
}