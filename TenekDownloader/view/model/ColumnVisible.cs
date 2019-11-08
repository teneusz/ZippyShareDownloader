using System.Windows;
using Prism.Mvvm;

namespace TenekDownloader.view.model
{
    public class ColumnVisible : BindableBase
    {
        private Visibility _downloadLink = Visibility.Visible;
        private Visibility _fileName = Visibility.Collapsed;
        private Visibility _groupName = Visibility.Hidden;
        private Visibility _progress = Visibility.Visible;
        private Visibility _serializable = Visibility.Visible;
        private Visibility _serviceLink = Visibility.Visible;
        private Visibility _serviceName = Visibility.Visible;
        private Visibility _status = Visibility.Visible;

        public Visibility Serializable
        {
            get => _serializable;
            set => SetProperty(ref _serializable, value);
        }

        public Visibility FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public Visibility GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public Visibility ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        public Visibility ServiceLink
        {
            get => _serviceLink;
            set => SetProperty(ref _serviceLink, value);
        }

        public Visibility DownloadLink
        {
            get => _downloadLink;
            set => SetProperty(ref _downloadLink, value);
        }

        public Visibility Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public Visibility Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
    }
}