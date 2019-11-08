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
                DownloadLink = link,
                FileName = link.Substring(link.LastIndexOf('/') + 1)
            };
        }
    }
}