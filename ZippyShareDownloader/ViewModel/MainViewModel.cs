using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using ZippyShareDownloader.Annotations;
using ZippyShareDownloader.Entity;
using ZippyShareDownloader.util;

namespace ZippyShareDownloader.View
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public static MainViewModel InstatnceMainViewModel = new MainViewModel();
        private int _downloadingCount = 0;
        private ObservableCollectionEx<DownloadEntity> _downloads = new ObservableCollectionEx<DownloadEntity>();

        public ObservableCollectionEx<DownloadEntity> Downloads
        {
            get => _downloads;
            set
            {
                _downloads = value;
                OnPropertyChanged(nameof(Downloads));
            }
        }

        private string _downloadLocation = "D:\\Downloads\\";

        public string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                _downloadLocation = value;
                OnPropertyChanged(nameof(Downloads));
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
        }

        public void Download(object obj)
        {
            if (_downloadingCount > _downloadAmount) return;
            var first = Downloads.FirstOrDefault(en => en.Status == DownloadStatus.NotDownloading || en.Status == DownloadStatus.Preparing);
            if (first == null) return;
            first.AfterDownload = AfterDownload;
            first.StartDownload(DownloadLocation);
            _downloadingCount++;
        }

        public void AfterDownload(object obj)
        {
            _downloadingCount--;
            Download(null);
            SerializerUtils.SaveDownloadEntities(_downloads.ToList());
        }

        public void AddLinks(object obj)
        {
            var window = new AddLinks();
            window.ShowDialog();
        }

        public void About(object obj)
        {
        }

        public void Exit(object obj)
        {
            Application.Current.Shutdown();
        }
    }
}