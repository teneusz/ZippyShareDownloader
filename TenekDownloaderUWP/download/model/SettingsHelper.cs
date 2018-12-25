using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace TenekDownloaderUWP.download.model
{
    public class SettingsHelper : BindableBase
    {
        private static Windows.Storage.ApplicationDataCompositeValue Settings =
            new Windows.Storage.ApplicationDataCompositeValue();

        public string DownloadPath
        {
            set
            {
                Settings["DownloadLocation"] = value;
                var downloadLocation = Settings["DownloadLocation"];
                SetProperty(ref downloadLocation, value);
            }
            get => (string) (Settings["DownloadLocation"] ?? "./");
        }

        public string SevenZipLibraryLocation
        {
            get => Settings["SevenZipDll"] as string;
            set
            {
                Settings["SevenZipDll"] = value;
                var defaultSevenZipDll = Settings["SevenZipDll"];
                SetProperty(ref defaultSevenZipDll, value);
            }
        }

        public bool AutoDownload
        {
            get => Settings["AutoDownload"] is bool b && b;
            set
            {
                Settings["AutoDownload"] = value;
                var settingsAutoDownload = Settings["AutoDownload"];
                SetProperty(ref settingsAutoDownload, value);
            }
        }
    }
}