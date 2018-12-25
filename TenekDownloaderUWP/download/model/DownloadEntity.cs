using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Mvvm;
using TenekDownloaderUWP.link.model;

namespace TenekDownloaderUWP.download.model
{
    public class DownloadEntity: BindableBase
    {
        private LinkInfo _linkInfo;
        private DownloadStatus _status = DownloadStatus.Waiting;
        private int _downloadPercent;

        public DownloadEntity(string link)
        {
            LinkInfo = new LinkInfo()
            {
                OrignalLink = link,
                ServiceName = GetService(link)
            };
        }

        public DownloadEntity()
        {

        }

        public LinkInfo LinkInfo
        {
            get => _linkInfo;
            set => SetProperty( ref _linkInfo, value);
        }

        public DownloadStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public int DownloadPercent
        {
            get => _downloadPercent;
            set => SetProperty(ref _downloadPercent, value);
        }

        [JsonIgnore]
        public DownloadGroup DownloadGroup { get; set; }

        public string GroupName => DownloadGroup?.Name;
        public Action<object> AfterDownload { get; set; }

        public Action<object> StartDownload { get; set; }

        private static string GetService(string link)
        {
            var result = "";
            if (link == null) throw new ArgumentNullException();

            if (link.ToLower().Contains(Http))
            {
                result = link.Replace(Http, "");
            }
            else if (link.ToLower().Contains(Https))
            {
                result = link.Replace(Https, "");
            }
            var tab = result.Remove(result.IndexOf('/')).Split('.');
            result = "";
            foreach (var s in tab)
            {
                if (s.Length > result.Length)
                    result = s;
            }
            return result;
        }

        public int ExtractProgress => DownloadGroup.ExtractProgress;

        public const string Http = "http://";
        public const string Https = "https://";
    }
    
    
}
