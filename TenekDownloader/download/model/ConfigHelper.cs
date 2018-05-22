using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenekDownloader.download.model
{
    [Serializable]
    public class ConfigHelper
    {

        private List<DownloadGroup> _downloadGroups = new List<DownloadGroup>();
        public List<DownloadGroup> DownloadGroups
        {
            get => _downloadGroups;
            set => _downloadGroups = value;
        }

        public ConfigHelper()
        {
        }
    }
}
