using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace TenekDownloader.viewModel
{
    public class LinksHelper : BindableBase
    {
        private string _links = "";
        private bool _isCompressed = true;
        private string _name;
        private bool _hasManyArchives;
        private bool _isInGroup;

        public string Links
        {
            get => _links;
            set => SetProperty(ref _links, value);
        }

        public bool IsCompressed
        {
            get => _isCompressed;
            set => SetProperty(ref _isCompressed, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool HasManyArchives
        {
            get => _hasManyArchives;
            set => SetProperty(ref _hasManyArchives, value);
        }

        public bool IsInGroup
        {
            get => _isInGroup;
            set => SetProperty(ref _isInGroup, value);
        }
    }
}