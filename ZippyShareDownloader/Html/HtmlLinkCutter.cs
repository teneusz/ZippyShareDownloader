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

        string GetDirectLinkFromLink(string link);
    }
}