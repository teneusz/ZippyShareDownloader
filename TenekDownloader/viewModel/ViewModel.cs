using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using TenekDownloader.download;
using TenekDownloader.download.model;
using TenekDownloader.link.model;
using TenekDownloader.util;
using Windows.Data.Xml;
using TenekDownloader.download.service;
using TenekDownloader.util.dlc;

namespace TenekDownloader.viewModel
{
	public class ViewModel : BindableBase
	{
		private ObservableCollection<DownloadGroup> _groups = new ObservableCollection<DownloadGroup>();
		private ObservableCollection<DownloadEntity> _entities = new ObservableCollection<DownloadEntity>();
		private LinksHelper _linksHelper = new LinksHelper();
		private const string ConfigJson = "config.json";
		public SettingsHelper SettingHelper { get; set; } = new SettingsHelper();
		public static AbstractDownloadService DownloadService = new DownloadServiceSecondVersion();


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
				//TODO: No other idea :D
				_entities.Clear();
				foreach (var downloadGroup in Groups)
				{
					_entities.AddRange(downloadGroup.Entities);
				}

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
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				SettingHelper.DownloadPath = dialog.SelectedPath + @"\";
			}
		}

		private void SaveSevenZipLibraryPath()
		{
			var dialog = new OpenFileDialog {Filter = "7-z library|7z.dll"};
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				SettingHelper.SevenZipLibraryLocation = dialog.SafeFileName;
			}
		}

		public void AddLinks()
		{
			var links = LinksHelper.Links.Split('\n');
			if (LinksHelper.IsInGroup)
			{
				var group = new DownloadGroup(new List<string>(links), LinksHelper.Name,
					LinksHelper.IsCompressed)
				{
					ManyArchives = LinksHelper.HasManyArchives
				};
				Groups.Add(group);
				foreach (var downloadEntity in @group.Entities)
				{
					AbstractDownloadService.DownloadQueue.Enqueue(downloadEntity);
				}
			}
			else
			{
				foreach (var link in links)
				{
					var group = new DownloadGroup(new List<string> {link}, string.Empty, LinksHelper.IsCompressed);
					Groups.Add(group);
					foreach (var downloadEntity in @group.Entities)
					{
						AbstractDownloadService.DownloadQueue.Enqueue(downloadEntity);
					}
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
			var open = new OpenFileDialog {Filter = "DLC files (*.dlc)|*.dlc"};
			var links = new List<string>();
			if (open.ShowDialog() == DialogResult.OK)
			{
				LinksHelper.Links = string.Empty;
				foreach (var packageFile in new DlcContainer(File.ReadAllText(open.FileName)).Content.Package.Files)
				{
					if (packageFile.URL.Trim() != String.Empty)
					{
						links.Add(packageFile.URL);
					}
				}

				LinksHelper.Links = string.Join(Environment.NewLine, links);
			}
		}

		public void About()
		{
		}

		public void UncheckAll()
		{
			foreach (var entity in Groups)
			{
				entity.IsSerialized = false;
			}
		}

		public void ClearList()
		{
			foreach (var entity in Groups.ToList())
			{
				if (!entity.IsSerialized)
				{
					Groups.Remove(entity);
				}
			}

			RaisePropertyChanged(nameof(Entities));

			SaveGroupsToFile();
		}

		public void Exit()
		{
			System.Windows.Application.Current.Shutdown();
		}
	}
}