using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZippyShareDownloader.View;

namespace ZippyShareDownloader.ViewModel
{
    class SettingsVM : BindableBase
    {
        private string _downloadLocation;
        private string _sevenZipLibraryLocation;

        public string DownloadLocation
        {
            get
            {
                //return _downloadLocation = Properties.Settings.Default.downloadPath;
                return _downloadLocation;
            }
            set
            {
                //Properties.Settings.Default.downloadPath = value;
                //Properties.Settings.Default.Save(); // TODO: move to another place
                //SetProperty(ref _downloadLocation, Properties.Settings.Default.downloadPath);
                SetProperty(ref _downloadLocation, value);
            }
        }

        public string SevenZipLibraryLocation
        {
            get
            {
                //return _downloadLocation = Properties.Settings.Default.sevenZipPath;
                return _sevenZipLibraryLocation;
            }
            set
            {
                //Properties.Settings.Default.sevenZipPath = value;
                //Properties.Settings.Default.Save(); // TODO: move to another place
                //SetProperty(ref _sevenZipLibraryLocation, Properties.Settings.Default.sevenZipPath);

                SetProperty(ref _sevenZipLibraryLocation, value);
            }
        }
        public DelegateCommand SaveDownloadPathCommand { get; }
        public DelegateCommand SaveSevenZipLibraryPathCommand { get; }
        public DelegateCommand<object> SaveCommand { get; }
        public DelegateCommand<object> CancelCommand { get; }

        public SettingsVM()
        {
            DownloadLocation = Properties.Settings.Default.downloadPath;
            SevenZipLibraryLocation = Properties.Settings.Default.sevenZipPath;

            SaveDownloadPathCommand = new DelegateCommand(SaveDownloadPath);
            SaveSevenZipLibraryPathCommand = new DelegateCommand(SaveSevenZipLibraryPath);
            SaveCommand = new DelegateCommand<object>(Exe_Save);
            CancelCommand = new DelegateCommand<object>(Exe_Cancel);
        }
        public void SaveDownloadPath()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DownloadLocation = dialog.SelectedPath + @"\";
            }
        }

        private void SaveSevenZipLibraryPath()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "7-z library|7z.dll";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SevenZipLibraryLocation = dialog.SafeFileName;
            }
        }
        private void Exe_Save(object obj)
        {
            Properties.Settings.Default.sevenZipPath = SevenZipLibraryLocation;
            Properties.Settings.Default.downloadPath= DownloadLocation;
            Properties.Settings.Default.Save(); // TODO: move to another place
            (obj as SettingsV).DialogResult = true;
        }
        private void Exe_Cancel(object obj)
        {
            (obj as SettingsV).DialogResult = false;
        }
    }
}
