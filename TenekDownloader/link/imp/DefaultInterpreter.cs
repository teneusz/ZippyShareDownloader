using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenekDownloader.link.model;

namespace TenekDownloader.link.imp
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