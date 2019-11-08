using Prism.Mvvm;

namespace TenekDownloader.viewModel
{
    public class LinksHelper : BindableBase
    {
        private bool _hasManyArchives;
        private bool _isCompressed = true;
        private bool _isInGroup = true;
        private string _links = "";
        private string _name;
        private string _archivePassword;

        public string Links
        {
            get => _links;
            set => SetProperty(ref _links, value.Replace("\n\n", "\n"));
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

        public string ArchivePassword
        {
            get => _archivePassword;
            set => SetProperty(ref _archivePassword, value);
        }
    }
}