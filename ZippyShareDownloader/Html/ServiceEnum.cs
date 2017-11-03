using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZippyShareDownloader.Html
{
    public class ServiceEnum
    {

        private ServiceEnum(string serviceName, Type type)
        {
            ServiceName = serviceName;
            _type = type;
        }
    }
}
