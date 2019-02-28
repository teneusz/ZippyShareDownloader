using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using TenekDownloader.download.model;
using TenekDownloader.download.service;
using TenekDownloader.download.service.impl;
using TenekDownloader.Properties;
using TenekDownloader.util;
using TenekDownloader.util.dlc;
using Application = System.Windows.Application;

namespace TenekDownloader.viewModel
{
	public class ViewModel : BindableBase
	{
		private const string ConfigJson = "config.json";
		public static AbstractDownloadService DownloadService = new FileDownloaderDownloadService();
		private ObservableCollection<DownloadEntity> _entities = new ObservableCollection<DownloadEntity>();
		private ObservableCollection<DownloadGroup> _groups = new ObservableCollection<DownloadGroup>();
		private LinksHelper _linksHelper = new LinksHelper();

		public ViewModel()
		{
			ExitCommand = new DelegateCommand(Exit);
			AddLinksCommand = new DelegateCommand(AddLinks);
			AboutCommand = new DelegateCommand(About);
			DownloadCommand = new DelegateCommand(Download);
			UncheckAllCommand = new DelegateCommand(UncheckAll);
			ClearListCommand = new DelegateCommand(ClearList);
			SaveDownloadPathCommand = new DelegateCommand(SaveDownloadPath);
			SaveSevenZipLibraryPathCommand = new DelegateCommand(SaveSevenZipLibraryPath);
			TestCommand = new DelegateCommand(ShowNotification);
			DlcCommand = new DelegateCommand(ReadDlc);
			LoadGroupsFromFile();
		}

		public SettingsHelper SettingHelper { get; set; } = new SettingsHelper();


		public LinksHelper LinksHelper
		{
			get => _linksHelper;
			set => SetProperty(ref _linksHelper, value);
		}

		public ObservableCollection<DownloadGroup> Groups
		{
			get => _groups;
			set => SetProperty(ref _groups, value);
		}

		public ObservableCollection<DownloadEntity> Entities
		{
			get
			{
				//TODO: I have no other idea :D
				_entities.Clear();
                
				foreach (var downloadGroup in Groups) _entities.AddRange(downloadGroup.Entities);

				return _entities;
			}
			set => SetProperty(ref _entities, value);
		}

		public ICommand ExitCommand { get; set; }
		public ICommand AddLinksCommand { get; set; }
		public ICommand DownloadCommand { get; set; }
		public ICommand AboutCommand { get; set; }
		public ICommand UncheckAllCommand { get; }
		public ICommand ClearListCommand { get; }
		public ICommand SaveDownloadPathCommand { get; }
		public ICommand SaveSevenZipLibraryPathCommand { get; }
		public ICommand TestCommand { get; }
		public ICommand DlcCommand { get; }

		private void LoadGroupsFromFile()
		{
			if (!File.Exists(ConfigJson)) return;
			Groups = SerializerUtils.ReadFromJsonFile<ObservableCollection<DownloadGroup>>(ConfigJson);
			foreach (var downloadGroup in Groups)
			{
				foreach (var downloadGroupEntity in downloadGroup.Entities)
				{
					downloadGroupEntity.DownloadGroup = downloadGroup;
					AbstractDownloadService.DownloadQueue.Enqueue(downloadGroupEntity);
				}
			}

			if (SettingHelper.AutoDownload) Download();
		}

		private void SaveGroupsToFile()
		{
			SerializerUtils.WriteToJsonFile(ConfigJson, Groups.ToList().FindAll(e => e.IsSerialized));
		}

		public void Download()
		{
			AbstractDownloadService.Downloading = true;
			new Task(DownloadService.Download).Start();
		}

		public void SaveDownloadPath()
		{
			var dialog = new FolderBrowserDialog();
			if (dialog.ShowDialog() == DialogResult.OK) SettingHelper.DownloadPath = dialog.SelectedPath + @"\";
		}

		private void SaveSevenZipLibraryPath()
		{
			var dialog = new OpenFileDialog {Filter = Resources.SevenZipFileFilter};
			if (dialog.ShowDialog() == DialogResult.OK) SettingHelper.SevenZipLibraryLocation = dialog.SafeFileName;
		}

		public void AddLinks()
		{
			var links = LinksHelper.Links.Split('\n');
			if (LinksHelper.IsInGroup)
			{
				var group = new DownloadGroup(new List<string>(links), LinksHelper.Name,
					LinksHelper.IsCompressed)
				{
					ManyArchives = LinksHelper.HasManyArchives,
                    ArchivePassword = LinksHelper.ArchivePassword
				};
				Groups.Add(group);
				foreach (var downloadEntity in group.Entities)
					AbstractDownloadService.DownloadQueue.Enqueue(downloadEntity);
			}
			else
			{
				foreach (var link in links)
				{
					var group = new DownloadGroup(new List<string> {link}, string.Empty, LinksHelper.IsCompressed)
                    {
                        ArchivePassword = LinksHelper.ArchivePassword
                    };
					Groups.Add(group);
                    foreach (var downloadEntity in group.Entities)
                        AbstractDownloadService.DownloadQueue.Enqueue(downloadEntity);
                }
			}

			RaisePropertyChanged(nameof(Entities));
			LinksHelper = new LinksHelper();
			SaveGroupsToFile();
			ShowNotification();
			if (!AbstractDownloadService.Downloading && SettingHelper.AutoDownload) Download();
		}

		private void ShowNotification()
		{
		}

		private void ReadDlc()
		{
			var open = new OpenFileDialog {Filter = Resources.DlcFileFilter};
			if (open.ShowDialog() != DialogResult.OK) return;
			LinksHelper.Links = string.Empty;
			var links = (from packageFile in new DlcContainer(File.ReadAllText(open.FileName)).Content.Package.Files where !string.IsNullOrEmpty(packageFile.URL.Trim()) select packageFile.URL).ToList();
           
			LinksHelper.Links = string.Join(Environment.NewLine, links);
		}

		public void About()
		{
		}

		public void UncheckAll()
		{
			foreach (var entity in Groups) entity.IsSerialized = false;
		}

		public void ClearList()
		{
			foreach (var entity in Groups.ToList())
				if (!entity.IsSerialized)
					Groups.Remove(entity);

			RaisePropertyChanged(nameof(Entities));

			SaveGroupsToFile();
            GC.Collect();
		}

		public void Exit()
		{
			Application.Current.Shutdown();
		}

		~ViewModel()
		{
			SaveGroupsToFile();
		}
	}
}