using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TenekDownloader.download.model;
using TenekDownloader.link.engine;
using TenekDownloader.link.model;

namespace TenekDownloader.link.imp
{
    public class ZippyshareInterpreter : ILinkInterpreter
    {
        private string _prefix;
        private string _htmlCode;
        private static readonly string JS_FORMAT =
            @"function download(){{var zippyObj={{}}; var dummy = undefined; {0} return zippyObj.href;}}";
        public LinkInfo LinkInfo { get; private set; }

        public void ProcessLink(string link)
        {
            LinkInfo = new LinkInfo { ServiceName = "ZippyShare", OrignalLink = link };
	        try
	        {
		        DownloadPage();
		        GetDirectLinkFromLink();
		        ProcessFileName();
		        ProcessFileSize();
		        CheckFileExistnce();
	        }
	        catch (WebException ex)
	        {
		        if (ex.Status == WebExceptionStatus.Timeout)
		        {
			        LinkInfo.BackToQueue = true;
		        }
	        }
        }

        private void DownloadPage()
        {
            var link = LinkInfo.OrignalLink;
            if (!link.StartsWith("http"))
            {
                link = "http://" + link;
            }
            _prefix = link.Substring(0, link.IndexOf(".com", StringComparison.Ordinal) + 4);
            using (var client = new WebClient())
            {
                if (link.Length > 0)
                {
	                _htmlCode = client.DownloadString(link);
                }
            }
        }

        private void GetDirectLinkFromLink()
        {
            var js = GetJavaScriptSection().Replace("document.getElementById('dlbutton')", "zippyObj").Replace("document.getElementById('omg').getAttribute('class')","2").Replace("document.getElementById('fimage')", "dummy");

            var jsScript = string.Format(JS_FORMAT, js);
            var link = "";
            using (var engine = new ScriptEngine("jscript"))
            {
                var parsed = engine.Parse(jsScript);
                link = (string)parsed.CallMethod("download");
            }

            LinkInfo.DownloadLink = _prefix + link;
        }

        private string GetJavaScriptSection()
        {
            var script = Regex.Matches(_htmlCode, "(?<=(<script type=\"text/javascript\">))(\\w|\\d|\\n|\\s|\\S)+?(?=(</script>))");
            foreach (Match temp in script)
            {
                if (temp.Value.Contains("dlbutton"))
                {
                    return temp.Value;
                }
            }

            return "";
        }

        private void ProcessFileName()
        {
            var returnLine = Regex.Match(LinkInfo.DownloadLink, "[^\\/]{1,}$");
            var result = returnLine.Success ? returnLine.Value : null;
            LinkInfo.FileName = result;
        }

        private void ProcessFileSize()
        {
            LinkInfo.FileSize = null;
        }

        private void CheckFileExistnce() => LinkInfo.IsFileExists = !_htmlCode.Contains("File does not exist on this server");
    }
}