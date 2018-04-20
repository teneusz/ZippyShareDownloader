using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using ZippyShareDownloader.Annotations;
using ZippyShareDownloader.Entity;
using ZippyShareDownloader.util;
using Application = System.Windows.Application;

namespace ZippyShareDownloader.View
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public static MainViewModel InstatnceMainViewModel = new MainViewModel();
        private int _downloadingCount = 0;
        private ObservableCollectionEx<DownloadEntity> _downloads = new ObservableCollectionEx<DownloadEntity>();
        public List<DownloadGroup> DownloadGroups = new List<DownloadGroup>();

        public ObservableCollectionEx<DownloadEntity> Downloads
        {
            get => _downloads;
            set
            {
                _downloads = value;
                OnPropertyChanged(nameof(Downloads));
            }
        }

        public string DownloadLocation
        {
            get => Properties.Settings.Default.downloadPath;
            set
            {
                Properties.Settings.Default.downloadPath = value;
                Properties.Settings.Default.Save(); // TODO: move to another place
                OnPropertyChanged(nameof(DownloadLocation));
            }
        }

        public string SevenZipLibraryLocation
        {
            get => Properties.Settings.Default.sevenZipPath;
            set
            {
                Properties.Settings.Default.sevenZipPath = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(SevenZipLibraryLocation));
            }
        }


        private int _downloadAmount = 1;

        public int DownloadAmount
        {
            get => _downloadAmount;
            set
            {
                _downloadAmount = value;
                OnPropertyChanged(nameof(DownloadAmount));
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel()
        {
            ExitCommand = new RelayCommand(Exit);
            AddLinksCommand = new RelayCommand(AddLinks);
            AboutCommand = new RelayCommand(About);
            DownloadCommand = new RelayCommand(Download);
            SettingsCommand = new RelayCommand(SettingsWindow);
            UncheckAllCommand = new RelayCommand(UncheckAll);
            ClearListCommand = new RelayCommand(ClearList);
            SaveDownloadPathCommand = new RelayCommand(SaveDownloadPath);
            SaveSevenZipLibraryPathCommand = new RelayCommand(SaveSevenZipLibraryPath);
        }

        public void Download(object obj)
        {
            if (_downloadingCount > _downloadAmount) return;
            var first = Downloads.FirstOrDefault(en =>
                en.Status == DownloadStatus.NotDownloading || en.Status == DownloadStatus.Preparing);
            if (first == null) return;
            first.AfterDownload = AfterDownload;
            first.StartDownload(DownloadLocation);
            _downloadingCount++;
        }

        public void SaveDownloadPath(object obj)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DownloadLocation = dialog.SelectedPath + @"\";
            }
        }

        private void SaveSevenZipLibraryPath(object obj)
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
            _downloadingCount--;
            Download(null);
            SerializerUtils.SaveConfig(new App.ConfigHelper() {DownloadGroups = this.DownloadGroups});
        }

        public void AddLinks(object obj)
        {
            var window = new AddLinks();
            window.ShowDialog();
        }

        public void SettingsWindow(object obj)
        {
            var window = new SettingsWindow();
            window.ShowDialog();
        }

        public void About(object obj)
        {
        }

        public void UncheckAll(object obj)
        {
            foreach (var entity in _downloads)
            {
                entity.SaveToFile = false;
            }
        }

        public void ClearList(object obj)
        {
            foreach (var entity in _downloads.ToList())
            {
                if (!entity.Status.Equals(DownloadStatus.Downloading))
                {
                    _downloads.Remove(entity);
                }
            }

            SerializerUtils.SaveConfig(new App.ConfigHelper() {DownloadGroups = this.DownloadGroups});
        }

        public void Exit(object obj)
        {
            Application.Current.Shutdown();
        }
    }
}