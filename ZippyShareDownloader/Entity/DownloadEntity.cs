using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using log4net;
using Newtonsoft.Json;
using ZippyShareDownloader.Annotations;
using ZippyShareDownloader.Html;
using ZippyShareDownloader.util;

namespace ZippyShareDownloader.Entity
{
    [Serializable]
    public class DownloadEntity : INotifyPropertyChanged
    {
        public string ServiceLink { get; set; }

        [JsonIgnore]
        public string DownloadLink { get; set; }

        public string FileName { get; set; }
        public string ServiceName { get; set; }

        [JsonIgnore]
        public int DownloadPercent { get; set; }

        [JsonIgnore]
        public bool SaveToFile { get; set; } = true;

        [JsonIgnore]
        public Action<object> AfterDownload { get; set; }

        public string Group { get; set; }
        public bool? IsInGroup { get; set; }

        private string _fileLocation = "";
        public DownloadStatus Status { get; set; } = DownloadStatus.Preparing;
        [JsonIgnore]
        private static readonly ILog Log = LogManager.GetLogger(typeof(HtmlFactory));
        public DownloadEntity()
        {
            //Default constructor required for serialize
        }

        public void StartDownload(string saveLocation)
        {
            try
            {
                PrepareToDownload();
                if (!IsStatusCorrect())
                {
                    AfterDownload(null);
                    return;
                }
                ProcessFileLocation(saveLocation);
                _time = DateTime.Now;
                using (var wc = new WebClient())
                {
                    Status = DownloadStatus.Downloading;
                    wc.DownloadProgressChanged += OnDownloadProgressChanged;
                    wc.DownloadFileCompleted += OnDownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(DownloadLink), _fileLocation);
                    wc.Proxy = null;
                    OnPropertyChanged(nameof(Status));
                }
            }
            catch (Exception ex)
            {
                Status = DownloadStatus.Error;
                Log.Debug("Error while starting a download process.", ex);
                Log.Debug(ToString());
            }
        }

        private bool IsStatusCorrect()
        {
            return Status == DownloadStatus.NotDownloading;
        }

        private void ProcessFileLocation(string saveLocation)
        {
            if (IsInGroup.HasValue && IsInGroup.Value && Group != null && Group?.Length > 0)
            {
                _fileLocation = saveLocation + Group;
                if (!Directory.Exists(_fileLocation))
                    Directory.CreateDirectory(_fileLocation);
                _fileLocation += "\\" + FileName;
            }
            else
                _fileLocation = saveLocation + FileName;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Status = DownloadStatus.Error;
                DownloadPercent = 0;
                Log.Debug("Error while downloading",e.Error);
                Log.Debug(ToString());
            }
            else if (e.Cancelled)
            {
                Status = DownloadStatus.Canceled;
                DownloadPercent = 0;
                Log.Debug("Download has been canceld.");
                Log.Debug(ToString());
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
            if ((DateTime.Now - _time) < TimeSpan.FromMilliseconds(1000)) return;
            DownloadPercent = e.ProgressPercentage;
            OnPropertyChanged(nameof(DownloadPercent));
            _time = DateTime.Now;
        }

        private DateTime _time;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PrepareToDownload()
        {
            var (service, parsedLink, fileName, isFileExist) = HtmlFactory.ParseLink(ServiceLink);
            if (isFileExist)
            {
                ServiceName = service;
                OnPropertyChanged(nameof(ServiceName));
                DownloadLink = parsedLink;
                OnPropertyChanged(nameof(DownloadLink));
                FileName = fileName;
                OnPropertyChanged(nameof(FileName));
                Status = DownloadStatus.NotDownloading;
                OnPropertyChanged(nameof(Status));
            }
            else
            {
                Status = DownloadStatus.NotFound;
            }
        }

        public override string ToString()
        {
            return new StringBuilder("{")
                .Append('\n')
                .Append("ServiceLink = '").Append(ServiceLink).Append("',").Append('\n')
                .Append("DownloadLink = '").Append(DownloadLink).Append("',").Append('\n')
                .Append("FileName = '").Append(FileName).Append("',").Append('\n')
                .Append("ServiceName = '").Append(ServiceName).Append("',").Append('\n')
                .ToString();
        }
    }

    public enum DownloadStatus
    {
        Preparing,
        NotDownloading,
        Downloading,
        Completed,
        Canceled,
        Error,
        NotFound
    }
}