using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZippyShareDownloader.Html.Cutter;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Web;
using log4net;
using ZippyShareDownloader.Annotations;
using ZippyShareDownloader.Entity;

namespace ZippyShareDownloader.Html
{
    public static class HtmlFactory
    {
        private static readonly Dictionary<string, IHtmlLinkCutter> FactoryHtml =
            new Dictionary<string, IHtmlLinkCutter>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(HtmlFactory));

        static HtmlFactory()
        {
            var zippy = new HtmlCutterZippyShare();
            FactoryHtml.Add(zippy.ServiceName.ToLower(), zippy);
  
        }


        public static (string service, string parsedLink, string fileName, bool isFileExist) ParseLink([NotNull]string link)
        {
            Log.Debug("start -- ParseLink(string)");
            Log.Debug("link = "+link);
            var service = GetService(link);
            var htmlCutter = ServicesEnum.ValueOf(service).CreateInstace();
            htmlCutter.Initialize(link);
            var parsedLink = htmlCutter.DirectLink;
            var fileName = htmlCutter.FileName;
            Log.Debug("end -- ParseLink(String)");
            return (service, parsedLink, fileName, htmlCutter.IsFileExist);
        }

        private static string GetFileName([NotNull]string link)
        {
            Log.Debug("start -- GetFileName(String)");
            Log.Debug("link = " + link);
            var lastSlashIndex = link.LastIndexOf('/') + 1;
            Log.Debug("end -- GetFileName(string)");
            return link.Substring(lastSlashIndex, link.Length - lastSlashIndex);
        }

        private static string GetService([NotNull]string link)
        {
            Log.Debug("start -- GetService(String)");
            Log.Debug("link = "+link);
            var result = "";
            if (link == null) throw new ArgumentNullException();

            if (link.ToLower().Contains(Http))
            {
                result = link.Replace(Http, "");
            }
            else if (link.ToLower().Contains(Https))
            {
                result = link.Replace(Https, "");
            }
            var tab = result.Remove(result.IndexOf('/')).Split('.');
            result = "";
            foreach (var s in tab)
            {
                if (s.Length > result.Length)
                    result = s;
            }
            Log.Debug("end -- GetFileName(String)");
            return result;
        }

        public const string Http = "http://";
        public const string Https = "https://";
    }
}