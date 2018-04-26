using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using ZippyShareDownloader.Model;
using ZippyShareDownloader.View;

namespace ZippyShareDownloader.ViewModel
{
    class AddLinksVM : BindableBase
    {
        private readonly MainWindowVM _viewModel = MainWindowVM.InstatnceMainVM;
        private static readonly ILog Log = LogManager.GetLogger(typeof(AddLinks));

        public DelegateCommand<object> Save { get; }
        public DelegateCommand<object> Cancel { get; }
        private string _links = "";
        private string _groupName = "";
        private bool _isDecompressedAfter = false;
        private bool _isInGroup;

        public bool IsDecompressedAfter
        {
            get => _isDecompressedAfter;
            set => SetProperty(ref _isDecompressedAfter, value);
        }
        public bool IsInGroup
        {
            get => _isInGroup;
            set => SetProperty(ref _isInGroup, value);
        }
        public string Links
        {
            get => _links;
            set => SetProperty(ref _links, value);
        }
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public AddLinksVM()
        {
            Save = new DelegateCommand<object>(Exe_Save);
            Cancel = new DelegateCommand<object>(Exe_Cancel);
        }
        private void Exe_Save(object obj)
        {
            Log.Debug("start -- saveOnClick");
            var tab = Links.Split('\n').Select(link => link.Trim()).ToList();
            var downloadGroup = new DownloadGroup(tab, GroupName, IsDecompressedAfter);
            MainWindowVM.DownloadGroups.Add(downloadGroup);
            (obj as AddLinks).DialogResult = true;
            Log.Debug("end -- saveOnClick");
        }
        private void Exe_Cancel(object obj)
        {
            (obj as AddLinks).DialogResult = false;
        }
    }
}
