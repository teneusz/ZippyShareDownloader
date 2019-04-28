using System;
using System.Net;
using System.Text.RegularExpressions;
using TenekDownloader.link.engine;
using TenekDownloader.link.model;

namespace TenekDownloader.link.imp
{
	public class ZippyshareInterpreter : ILinkInterpreter
	{
		private static readonly string JS_FORMAT =
			@"function download(){{var zippyObj={{}}; var dummy = undefined; {0} return zippyObj.href;}}";

		private string _htmlCode;
		private string _prefix;
        private static readonly string JavascriptPattern = "(?<=(<script type=\"text/javascript\">))(\\w|\\d|\\n|\\s|\\S)+?(?=(</script>))";
        public LinkInfo LinkInfo { get; private set; }

		public void ProcessLink(string link)
		{
			LinkInfo = new LinkInfo {ServiceName = "ZippyShare", OrignalLink = link};
			try
			{
				DownloadPage();
				GetDirectLinkFromLink();
				ProcessFileName();
				ProcessFileSize();
				CheckFileExistence();
			}
			catch (WebException ex)
			{
				if (ex.Status == WebExceptionStatus.Timeout) LinkInfo.BackToQueue = true;
			}
		}

		private void DownloadPage()
		{
			var link = LinkInfo.OrignalLink;
			if (!link.StartsWith("http")) link = "http://" + link;
			_prefix = link.Substring(0, link.IndexOf(".com", StringComparison.Ordinal) + 4);
			using (var client = new WebClient())
			{
				if (link.Length > 0) _htmlCode = client.DownloadString(link);
			}
		}

		private void GetDirectLinkFromLink()
		{
			var js = GetJavaScriptSection().Replace("document.getElementById('dlbutton')", "zippyObj")
				.Replace("document.getElementById('omg').getAttribute('class')", "2")
				.Replace("document.getElementById('fimage')", "dummy");

			var jsScript = string.Format(JS_FORMAT, js);
            var link = string.Empty;
			using (var engine = new ScriptEngine("jscript"))
			{
				var parsed = engine.Parse(jsScript);
				link = (string) parsed.CallMethod("download");
			}

			LinkInfo.DownloadLink = _prefix + link;
		}

		private string GetJavaScriptSection()
		{
            var script = Regex.Matches(_htmlCode,
				JavascriptPattern);
			foreach (Match temp in script)
				if (temp.Value.Contains("dlbutton"))
					return temp.Value;

			return string.Empty;
		}

		private void ProcessFileName()
		{
			var returnLine = Regex.Match(LinkInfo.DownloadLink, "[^\\/]{1,}$");
			LinkInfo.FileName = returnLine.Success ? returnLine.Value : null;
		}

		private void ProcessFileSize()
		{
			LinkInfo.FileSize = 0;
		}

		private void CheckFileExistence()
		{
			LinkInfo.IsFileExists = !_htmlCode.Contains("File does not exist on this server") && !_htmlCode.Contains("File has expired and does not exist anymore on this server");
		}
	}
}