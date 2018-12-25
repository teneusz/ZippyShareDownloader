using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenekDownloaderUWP.link;
using TenekDownloaderUWP.link.model;

namespace TenekDownloaderUWP.link.imp
{
    public class DefaultInterpreter : ILinkInterpreter
    {
        public LinkInfo LinkInfo { get; private set; }

        public void ProcessLink(string link)
        {
            LinkInfo = new LinkInfo
            {
                OrignalLink = link,
                DownloadLink = link
            };
        }
    }
}