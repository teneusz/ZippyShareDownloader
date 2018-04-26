﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZippyShareDownloader.Html;
using ZippyShareDownloader.Model;
using ZippyShareDownloader.util;
using ZippyShareDownloader.View;
using ZippyShareDownloader.ViewModel;

namespace ZippyShareDownloader.Model
{
    [Serializable]
    public class DownloadGroup
    {
        private List<DownloadEntity> _downloadEntities = new List<DownloadEntity>();
        public List<DownloadEntity> DownloadEntities { get=>_downloadEntities;
            set => _downloadEntities = value;
        }
        public bool? IsDecompressedAfter { get=>_isDecompressedAfter; set=>_isDecompressedAfter = value; }
        private bool? _isDecompressedAfter;
        public string Name { get; set; }
        private bool _isEnded = false;

        public DownloadGroup()
        {
            //Default constractor for Serialization
        }

        private DownloadGroup(string name, bool? isDecompressedAfter)
        {
            this.Name = name;
            this.IsDecompressedAfter = isDecompressedAfter;
        }

        public DownloadGroup(List<DownloadEntity> entities, string name, bool? isDecompressedAfter) : this(name,
            isDecompressedAfter)
        {
            this.DownloadEntities = entities;
            foreach (var donwloadEntity in DownloadEntities)
            {
                donwloadEntity.DownloadGroup = this;
            }
        }

        public DownloadGroup(IEnumerable<string> links, string name, bool? isDecompressedAfter) : this(name,
            isDecompressedAfter)
        {
            ProcessLinks(links);
        }

        private void ProcessLinks(IEnumerable<string> links)
        {

            foreach (var s in links)
            {
                var link = s;
                if (!link.StartsWith(HtmlFactory.Http) && !link.StartsWith(HtmlFactory.Https))
                {
                    link = HtmlFactory.Http + s;
                }

                if (s.Length <= 0) continue;
                var dream = new DownloadEntity
                {
                    ServiceLink = link,
                    DownloadGroup = this
                };
                MainWindowVM.InstatnceMainVM.Downloads.Add(dream);
                DownloadEntities.Add(dream);
            }
        }

        private void Decompress()
        {
            var file = DownloadEntities.OrderBy(e => e.FileName).First().FileLocation;
            ArchiveUtil.UnpackArchive(file, Directory.GetParent(file).FullName);
        }

        public void Refresh()
        {
            if (_isEnded) return;
            var everyone = DownloadEntities.Aggregate(true,
                (current, donwloadEntity) => current && donwloadEntity.Status == DownloadStatus.Completed);

            if (!everyone) return;
            Decompress();
            _isEnded = true;
        }
    }
}