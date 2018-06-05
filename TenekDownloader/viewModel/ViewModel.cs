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
//using Windows.UI.Notifications;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using TenekDownloader.download;
using TenekDownloader.download.model;
using TenekDownloader.link.model;
using TenekDownloader.util;
using Windows.Data.Xml;
using Application = System.Windows.Application;

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
            UncheckAllCommand = new DelegateCommand(UncheckAll, ReturnTrue);
            ClearListCommand = new DelegateCommand(ClearList, ReturnTrue);
            SaveDownloadPathCommand = new DelegateCommand(SaveDownloadPath, ReturnTrue);
            SaveSevenZipLibraryPathCommand = new DelegateCommand(SaveSevenZipLibraryPath, ReturnTrue);

            LoadObjectFromFile();
        }

        private void LoadObjectFromFile()
        {
            if (!File.Exists("config.json")) return;
            Groups = SerializerUtils.ReadFromJsonFile<ObservableCollection<DownloadGroup>>("config.json");
            foreach (var downloadGroup in Groups)
            {
                foreach (var downloadGroupEntity in downloadGroup.Entities)
                {
                    downloadGroupEntity.DownloadGroup = downloadGroup;
                }
            }
        }

        private void SaveGroupsToFile()
        {
            SerializerUtils.WriteToJsonFile("config.json",Groups);
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
            var dialog = new System.Windows.Forms.OpenFileDialog {Filter = "7-z library|7z.dll"};
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
                foreach (var downloadEntity in @group.Entities)
                {
                    DownloadService.DownloadQueue.Enqueue(downloadEntity);
                }
            }
            else
            {
                foreach (var link in links)
                {
                    var group = new DownloadGroup(new List<string> {link},string.Empty,LinksHelper.IsCompressed);
                    Groups.Add(group);
                    foreach (var downloadEntity in @group.Entities)
                    {
                        DownloadService.DownloadQueue.Enqueue(downloadEntity);
                    }
                }
            }
            RaisePropertyChanged(nameof(Entities));
            LinksHelper = new LinksHelper();
            SaveGroupsToFile();
            ShowNotification();
        }

        private static void ShowNotification()
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Matt sent you a friend request"
                            },
                            new AdaptiveText()
                            {
                                Text = "Hey, wanna dress up as wizards and ride around on our hoverboards together?"
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "https://unsplash.it/64?image=1005",
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    }
                }
            };


            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(toastContent.GetContent());

            var toast = new ToastNotification(xmlDoc);
            ToastNotificationManager.CreateToastNotifier("tet").Show(toast);

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