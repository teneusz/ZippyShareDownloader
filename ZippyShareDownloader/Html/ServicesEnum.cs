using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZippyShareDownloader.Html.Cutter;

namespace ZippyShareDownloader.Html
{
    public class ServicesEnum
    {
        public static readonly ServicesEnum ZIPPYSHARE = new ServicesEnum("zippyshare", typeof(HtmlCutterZippyShare));

        public static IEnumerable<ServicesEnum> Values
        {
            get { yield return ZIPPYSHARE; }
        }

        public static ServicesEnum ValueOf(string serviceName)
        {
            ServicesEnum result = null;

            foreach (var temp in Values)
            {
                if (temp.ServiceName.Equals(serviceName))
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

        public IHtmlLinkCutter CreateInstace()
        {
            return Activator.CreateInstance(_type) as IHtmlLinkCutter;
        }
    }
}
