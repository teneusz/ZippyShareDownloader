using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace TenekDownloader.download.model
{
    public class DownloadGroup : BindableBase
    {
        private List<string> _links;
        private string _name;
        private bool? _unpack;
        public bool ManyArchives;

        public int ExtractProgress
        {
            get => _extractProgress;
            set => SetProperty(ref _extractProgress, value);
        }

        public DownloadGroup()
        {
        }

        public DownloadGroup(List<string> links, string name, bool? unpack)
        {
            this._links = links;
            this._name = name;
            this.IsAutoExtracting = unpack??false;

            _downloadLocation = Properties.Settings.Default.DownloadLocation;
            foreach(var link in links)
            {
                var unescapeLink = link.Replace("\r","");
                if(string.IsNullOrEmpty(unescapeLink)) continue;
                Entities.Add(new DownloadEntity(unescapeLink) {DownloadGroup = this});
            }
        }

        private ObservableCollection<DownloadEntity> _entities = new ObservableCollection<DownloadEntity>();

        public ObservableCollection<DownloadEntity> Entities
        {
            get => _entities;
            set => SetProperty(ref _entities, value);
        }

        private string _downloadLocation = "./";
        private bool _isAutoExtracting = false;
        private bool _isMoreThanOneArchive = false;
        private int _extractProgress;
        public string DownloadLocation { get=>_downloadLocation; set=>SetProperty(ref _downloadLocation,value); }

        public bool IsAutoExtracting
        {
            get => _isAutoExtracting;
            set => SetProperty(ref _isAutoExtracting ,value);
        }

        public bool IsMoreThanOneArchive
        {
            get => _isMoreThanOneArchive;
            set => SetProperty(ref _isMoreThanOneArchive, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public DownloadEntity this[int index]
        {
            get => Entities[index];
            set => Entities[index] = value;
        }


    }
}
