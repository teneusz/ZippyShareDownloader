using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using ZippyShareDownloader.Annotations;
using ZippyShareDownloader.Html;
using ZippyShareDownloader.util;

namespace ZippyShareDownloader.Entity
{
    public class DownloadEntity : INotifyPropertyChanged
    {
        public string ServiceLink { get; set; }

        public string DownloadLink { get; set; }
        public string FileName { get; set; }
        public string ServiceName { get; set; }
        public int DownloadPercent { get; set; } = 0;
        public Action<object> AfterDownload { get; set; }

        private string _fileLocation = "";
        public DownloadStatus Status { get; set; } = DownloadStatus.Preparing;

        public void StartDownload(string saveLocation)
        {
            _fileLocation = saveLocation + FileName;
            PrepareToDownload();
            time = DateTime.Now;
            using (var wc = new WebClient())
            {
                Status = DownloadStatus.Downloading;
                wc.DownloadProgressChanged += OnDownloadProgressChanged;
                wc.DownloadFileCompleted += OnDownloadFileCompleted;
                wc.DownloadFileAsync(new System.Uri(DownloadLink), _fileLocation);
                OnPropertyChanged(nameof(Status));
            }
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Status = DownloadStatus.Error;
                DownloadPercent = 0;
            }
            else
            {
                CheckIfFileIsDownloadedSuccesful();
                
            }
            OnPropertyChanged(nameof(DownloadPercent));
            OnPropertyChanged(nameof(Status));
            AfterDownload(null);
        }

        private void CheckIfFileIsDownloadedSuccesful()
        {
            if (FileUtil.CheckFileType(_fileLocation))
            {
                Status = DownloadStatus.Completed;
                DownloadPercent = 100;
            }
            else
            {
                Status = DownloadStatus.Preparing;
                DownloadPercent = 0;
                DownloadLink = null;
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            
            if ((DateTime.Now - time) < TimeSpan.FromMilliseconds(1000)) return;
            DownloadPercent = e.ProgressPercentage;
            OnPropertyChanged(nameof(DownloadPercent));
            time = DateTime.Now;
        }

        private DateTime time;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PrepareToDownload()
        {
            var tuple = HtmlFactory.ParseLink(ServiceLink);
            ServiceName = tuple.service;
            OnPropertyChanged(nameof(ServiceName));
            DownloadLink = tuple.parsedLink;
            OnPropertyChanged(nameof(DownloadLink));
            FileName = tuple.fileName;
            OnPropertyChanged(nameof(FileName));
            Status = DownloadStatus.NotDownloading;
            OnPropertyChanged(nameof(Status));
        }
    }

    public enum DownloadStatus
    {
        Preparing,
        NotDownloading,
        Downloading,
        Completed,
        Error
    }
}