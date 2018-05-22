using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenekDownloader.link.model;

namespace TenekDownloader.link
{
    public interface ILinkInterpreter
    {
        LinkInfo LinkInfo { get; }

        void ProcessLink(string link);
    }

}
