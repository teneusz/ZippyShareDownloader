using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Microsoft.Toolkit.Uwp.Notifications;
using Prism.Commands;
using Prism.Mvvm;
using TenekDownloader.download;
using TenekDownloader.download.model;
using TenekDownloader.link.model;
using TenekDownloader.util;
using TenekDownloader.view;
using Application = System.Windows.Application;
using ToastContent = Microsoft.Toolkit.Uwp.Notifications.ToastContent;


namespace TenekDownloader.viewModel
{
    public class ViewModel : BindableBase
    {
        private ObservableCollection<DownloadGroup> _groups = new ObservableCollection<DownloadGroup>();
        private ObservableCollection<DownloadEntity> _entities = new ObservableCollection<DownloadEntity>();
        private LinksHelper _linksHelper = new LinksHelper();

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
            get => _entities;
            set => SetProperty(ref _entities, value);
        }

        public ICommand ExitCommand { get; set; }
        public ICommand AddLinksCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public ICommand AboutCommand { get; set; }
        public ICommand UncheckAllCommand { get; }
        public ICommand SettingsCommand { get; set; }
        public ICommand ClearListCommand { get; }
        public ICommand SaveDownloadPathCommand { get; }
        public ICommand SaveSevenZipLibraryPathCommand { get; }

        public string SevenZipLibraryLocation
        {
            get => Properties.Settings.Default.SevenZipDll;
            set
            {
                Properties.Settings.Default.SevenZipDll = value;
                Properties.Settings.Default.Save();
                var defaultSevenZipDll = Properties.Settings.Default.SevenZipDll;
                SetProperty(ref defaultSevenZipDll, value);
            }
        }

        public string DownloadLocation
        {
            get => Properties.Settings.Default.DownloadLocation;
            set
            {
                Properties.Settings.Default.DownloadLocation = value;
                Properties.Settings.Default.Save();
                var defaultDownloadLocation = Properties.Settings.Default.DownloadLocation;
                SetProperty(ref defaultDownloadLocation, value);
            }
        }


        public ViewModel()
        {
            ExitCommand = new DelegateCommand(Exit, ReturnTrue);
            AddLinksCommand = new DelegateCommand(AddLinks, ReturnTrue);
            AboutCommand = new DelegateCommand(About, ReturnTrue);
            DownloadCommand = new DelegateCommand(Download, ReturnTrue);
            SettingsCommand = new DelegateCommand(SettingsWindow, ReturnTrue);
            UncheckAllCommand = new DelegateCommand(UncheckAll, ReturnTrue);
            ClearListCommand = new DelegateCommand(ClearList, ReturnTrue);
            SaveDownloadPathCommand = new DelegateCommand(SaveDownloadPath, ReturnTrue);
            SaveSevenZipLibraryPathCommand = new DelegateCommand(SaveSevenZipLibraryPath, ReturnTrue);
        }

        public bool ReturnTrue()
        {
            return true;
        }

        public void Download()
        {
            DownloadService.Downloading = true;
            DownloadService.Download();
        }

        public void SaveDownloadPath()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DownloadLocation = dialog.SelectedPath + @"\";
            }
        }

        private void SaveSevenZipLibraryPath()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "7-z library|7z.dll";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SevenZipLibraryLocation = dialog.SafeFileName;
            }
        }

        public void AfterDownload(object obj)
        {
            //_downloadingCount--;
            Download();
            SerializerUtils.SaveConfig(new ConfigHelper() {DownloadGroups = this.Groups.ToList()});
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
            }
            else
            {
                foreach (var link in links)
                {
                    var group = new DownloadGroup(new List<string> {link},String.Empty,LinksHelper.IsCompressed);
                    Groups.Add(group);
                }
            }

            ShowNotification();
        }

        private static void ShowNotification()
        {
            throw  new NotImplementedException();
        }

        public void SettingsWindow()
        {
            var window = new SettingsWindow();
            window.ShowDialog();
        }

        public void About()
        {
        }

        public void UncheckAll()
        {
//            foreach (var entity in _downloads)
//            {
//                entity.SaveToFile = false;
//            }
        }

        public void ClearList()
        {
//            foreach (var entity in _downloads.ToList())
//            {
//                if (!entity.Status.Equals(DownloadStatus.Downloading))
//                {
//                    _downloads.Remove(entity);
//                }
//            }
//
//            SerializerUtils.SaveConfig(new App.ConfigHelper() { DownloadGroups = this.DownloadGroups });
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}