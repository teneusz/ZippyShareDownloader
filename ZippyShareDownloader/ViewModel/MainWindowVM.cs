using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
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
using ZippyShareDownloader.Model;
using ZippyShareDownloader.util;
using ZippyShareDownloader.View;
using Application = System.Windows.Application;

namespace ZippyShareDownloader.ViewModel
{
    public class MainWindowVM : BindableBase
    {
        public static MainWindowVM InstatnceMainViewModel = new MainWindowVM();
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

        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand AddLinksCommand { get; set; }
        public DelegateCommand DownloadCommand { get; set; }
        public DelegateCommand AboutCommand { get; set; }
        public DelegateCommand UncheckAllCommand { get; }
        public DelegateCommand SettingsCommand { get; set; }
        public DelegateCommand ClearListCommand { get; }
        public DelegateCommand SaveDownloadPathCommand { get; }
        public DelegateCommand SaveSevenZipLibraryPathCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowVM()
        {
            ExitCommand = new DelegateCommand(Exit);
            AddLinksCommand = new DelegateCommand(AddLinks);
            AboutCommand = new DelegateCommand(About);
            DownloadCommand = new DelegateCommand(Download);
            SettingsCommand = new DelegateCommand(SettingsWindow);
            UncheckAllCommand = new DelegateCommand(UncheckAll);
            ClearListCommand = new DelegateCommand(ClearList);
            SaveDownloadPathCommand = new DelegateCommand(SaveDownloadPath);
            SaveSevenZipLibraryPathCommand = new DelegateCommand(SaveSevenZipLibraryPath);
        }

        private void Download()
        {
            if (_downloadingCount > _downloadAmount) return;
            var first = Downloads.FirstOrDefault(en =>
                en.Status == DownloadStatus.NotDownloading || en.Status == DownloadStatus.Preparing);
            if (first == null) return;
            first.AfterDownload = new DelegateCommand(AfterDownload);
            first.StartDownload(DownloadLocation);
            _downloadingCount++;
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

        public void AfterDownload()
        {
            _downloadingCount--;
            Download();
            SerializerUtils.SaveConfig(new App.ConfigHelper() {DownloadGroups = this.DownloadGroups});
        }

        public void AddLinks()
        {
            var window = new AddLinks();
            window.ShowDialog();
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
            foreach (var entity in _downloads)
            {
                entity.SaveToFile = false;
            }
        }

        public void ClearList()
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

        public void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}