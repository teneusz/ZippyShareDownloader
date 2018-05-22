using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public DownloadGroup(List<string> links, string name, bool? unpack)
        {
            this._links = links;
            this._name = name;
            this._unpack = unpack;
            foreach(var link in links)
            {
                if(string.IsNullOrEmpty(link)) continue;
                Entities.Add(new DownloadEntity(link){DownloadGroup = this});
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
