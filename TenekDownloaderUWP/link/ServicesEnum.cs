using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenekDownloaderUWP.link;
using TenekDownloaderUWP.link.imp;

namespace TenekDownloaderUWP.link
{
    public class ServicesEnum
    {
        public static readonly ServicesEnum ZIPPYSHARE = new ServicesEnum("zippyshare", typeof(ZippyshareInterpreter));
        public static readonly ServicesEnum DEFAULT = new ServicesEnum("default", typeof(DefaultInterpreter));

        public static IEnumerable<ServicesEnum> Values
        {
            get { yield return ZIPPYSHARE; }
        }

        public static ServicesEnum ValueOf(string serviceName)
        {
            var result = DEFAULT;

            foreach (var temp in Values)
            {
                if (temp.ServiceName.Equals(serviceName?.ToLower()))
                {
                    result = temp;
                }
            }

            return result;
        }

        private readonly Type _type;
        public string ServiceName { get; }

        private ServicesEnum(string name, Type type)
        {
            this.ServiceName = name;
            this._type = type;
        }

        public ILinkInterpreter CreateInstace()
        {
            return Activator.CreateInstance(_type) as ILinkInterpreter;
        }
    }
}
