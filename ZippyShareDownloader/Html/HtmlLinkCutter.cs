using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZippyShareDownloader.Html
{
    public interface IHtmlLinkCutter
    {
        string ServiceName { get;}

        string GetDirectLink();

        string GetFileName();

        long? GetFileSize();

        void Initialize(string link);
    }
}