using System;
using System.Collections.Generic;
using TenekDownloader.link.imp;

namespace TenekDownloader.link
{
    public class ServicesEnum
    {
        public static readonly ServicesEnum ZIPPYSHARE = new ServicesEnum("zippyshare", typeof(ZippyshareInterpreter));
        public static readonly ServicesEnum DEFAULT = new ServicesEnum("default", typeof(DefaultInterpreter));

        private readonly Type _type;

        private ServicesEnum(string name, Type type)
        {
            ServiceName = name;
            _type = type;
        }

        public static IEnumerable<ServicesEnum> Values
        {
            get { yield return ZIPPYSHARE; }
        }

        public string ServiceName { get; }

        public static ServicesEnum ValueOf(string serviceName)
        {
            var result = DEFAULT;

            foreach (var temp in Values)
                if (temp.ServiceName.Equals(serviceName?.ToLower()))
                    result = temp;

            return result;
        }

        public ILinkInterpreter CreateInstace()
        {
            return Activator.CreateInstance(_type) as ILinkInterpreter;
        }
    }
}