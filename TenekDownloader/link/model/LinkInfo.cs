using System;
using System.Security.Policy;
using Prism.Mvvm;
using TenekDownloader.download.model;

namespace TenekDownloader.link.model
{
    public class LinkInfo : BindableBase
    {
        private bool _backToQueue;
        private string _downloadLink;
        private string _downloadLocation;
        private string _fileName;
        private double _fileSize = 0.0;
        private double _downloadedSize = 0.0;
        private bool _isFileExists;
        private string _originalLink;
        private string _serviceName;
        private Uri _uri;
        private int _cachedSize = 0;
        private int _downloadSpeed = 0;
        private string _timeLeft;
        private DownloadEntity _downloadEntity;

        public bool BackToQueue
        {
            get => _backToQueue;
            set => SetProperty(ref _backToQueue, value);
        }

        public string OrignalLink
        {
            get => _originalLink;
            set => SetProperty(ref _originalLink, value);
        }

        public string DownloadLink
        {
            get => _downloadLink;
            set => SetProperty(ref _downloadLink, value);
        }

        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        public double FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        public double DownloadedSize
        {
            get => _downloadedSize;
            set => SetProperty(ref _downloadedSize, value);
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public bool IsFileExists
        {
            get => _isFileExists;
            set => SetProperty(ref _isFileExists, value);
        }

        public string DownloadLocation
        {
            get => _downloadLocation;
            set => SetProperty(ref _downloadLocation, value);
        }

        public Uri Uri
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }

        public string FileLocation => DownloadLocation + "\\" + FileName;

        public int CachedSize
        {
            get => _cachedSize;
            set
            {
                if (DownloadEntity != null)
                {
                    DownloadEntity.DownloadPercent = (int)Math.Round((DownloadedSize / FileSize) * 100);
                }

                SetProperty(ref _cachedSize, value);
            }
        }

        public DownloadEntity DownloadEntity
        {
            private get => _downloadEntity;
            set => SetProperty(ref _downloadEntity, value);
        }

        public int DownloadSpeed
        {
            get => _downloadSpeed;
            set => SetProperty(ref _downloadSpeed, value);
        }

        public string TimeLeft
        {
            get => _timeLeft;
            set => SetProperty(ref _timeLeft, value);
        }
    }
}