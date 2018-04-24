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
        public static MainWindowVM InstatnceMainVM = new MainWindowVM();
        private int _downloadingCount = 0;
        private ObservableCollectionEx<DownloadEntity> _downloads = new ObservableCollectionEx<DownloadEntity>();
        public List<DownloadGroup> DownloadGroups = new List<DownloadGroup>();

        public ObservableCollectionEx<DownloadEntity> Downloads
        {
            get => _downloads;
            set
            {
                SetProperty(ref _downloads, value);
            }
        }

        public string DownloadLocation
        {
            get => Properties.Settings.Default.downloadPath;
            //set
            //{
            //    Properties.Settings.Default.downloadPath = value;
            //    Properties.Settings.Default.Save(); // TODO: move to another place
            //    OnPropertyChanged(nameof(DownloadLocation));
            //}
        }

        //public string SevenZipLibraryLocation
        //{
        //    get => Properties.Settings.Default.sevenZipPath;
        //    set
        //    {
        //        Properties.Settings.Default.sevenZipPath = value;
        //        Properties.Settings.Default.Save();
        //        OnPropertyChanged(nameof(SevenZipLibraryLocation));
        //    }
        //}


        private int _downloadAmount = 1;

        public int DownloadAmount
        {
            get => _downloadAmount;
            set
            {
               SetProperty(ref _downloadAmount, value);
            }
        }
        #region Command and Constructor
        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand AddLinksCommand { get; set; }
        public DelegateCommand DownloadCommand { get; set; }
        public DelegateCommand AboutCommand { get; set; }
        public DelegateCommand UncheckAllCommand { get; }
        public DelegateCommand SettingsCommand { get; set; }
        public DelegateCommand ClearListCommand { get; }

        public MainWindowVM()
        {
            ExitCommand = new DelegateCommand(Exit);
            AddLinksCommand = new DelegateCommand(AddLinks);
            AboutCommand = new DelegateCommand(About);
            DownloadCommand = new DelegateCommand(Download);
            SettingsCommand = new DelegateCommand(SettingsWindow);
            UncheckAllCommand = new DelegateCommand(UncheckAll);
            ClearListCommand = new DelegateCommand(ClearList);
          
        }
        #endregion
        
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
            var window = new SettingsV();
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
                if (!entity.Status.Equals(DownloadStatus.Downloading))//zmieniłem warunek z !entity.Status.Equals(DownloadStatus.Downloading
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

//http://www53.zippyshare.com/v/SWBpqwM9/file.html
//http://www53.zippyshare.com/v/gfO0uA0C/file.html
//http://www53.zippyshare.com/v/YUfUdYpU/file.html
//http://www53.zippyshare.com/v/3ibie84b/file.html
//http://www53.zippyshare.com/v/5vARWPM1/file.html
//http://www53.zippyshare.com/v/C5DjO8Dm/file.html
//http://www53.zippyshare.com/v/bF1EKWX0/file.html
//http://www53.zippyshare.com/v/mTaBemMd/file.html
//http://www53.zippyshare.com/v/jXeL1aK7/file.html