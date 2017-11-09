using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using ZippyShareDownloader.Annotations;
using ZippyShareDownloader.util;

namespace ZippyShareDownloader.Html.Cutter
{
    public class HtmlCutterZippyShare : IHtmlLinkCutter
    {
        private static readonly string JS_FORMAT =
            @"function download(){{{0} {1}}}";

        private static readonly ILog log = LogManager.GetLogger(typeof(HtmlCutterZippyShare));
        private string _link;
        private string _htmlCode;
        private string _prefix;
        private string _directLink;
        private string _fileName;
        private long? _fileSize;

        private void DownloadPage()
        {
            var htmlCode = _link;
            if (!_link.StartsWith("http"))
            {
                htmlCode = "http://" + _link;
            }
            _prefix = htmlCode.Substring(0, htmlCode.IndexOf(".com", StringComparison.Ordinal) + 4);
            using (var client = new WebClient())
            {
                if (_link.Length > 0)
                {
                    _htmlCode = client.DownloadString(htmlCode);
                }
            }
        }

        public string GetDirectLink()
        {
            return _directLink;
        }

        public string GetFileName()
        {
            return _fileName;
        }

        public long? GetFileSize()
        {
            return _fileSize;
        }

        private void GetDirectLinkFromLink()
        {
            var javascriptVariableA = GetVariableA();
            var javascriptReturnLine = GetReturnLine();
            var jsScript = string.Format(JS_FORMAT, javascriptVariableA, javascriptReturnLine);
            var link = "";
            using (var engine = new ScriptEngine("jscript"))
            {
                var parsed = engine.Parse(jsScript);
                link = (string) parsed.CallMethod("download");
            }

            _directLink = _prefix + link;
        }

        private string GetReturnLine()
        {
            var returnLine = Regex.Match(_htmlCode,
                "(document\\.getElementById\\(\'dlbutton\'\\).href.+;)");
            return returnLine.Success
                ? "return " + returnLine.Value.Replace("document.getElementById('dlbutton').href = ", "")
                : "";
        }

        private string GetVariableA()
        {
            var indexOfVarA = Regex.Match(_htmlCode, @"(var a = [\d]{1,}[\S]{1,};)");
            return indexOfVarA.Success ? indexOfVarA.Value : "";
        }

        public string ServiceName { get; } = "ZippyShare";

        public void Initialize(string link)
        {
            _link = link;
            DownloadPage();
            GetDirectLinkFromLink();
            ProcessFileName();
            ProcessFileSize();
            log.Debug(this.ToString());
            log.Info(this.ToString());
        }

        private void ProcessFileSize()
        {
            _fileSize = null;
        }

        private void ProcessFileName()
        {
            var returnLine = Regex.Match(_directLink, "[^\\/]{1,}$");
            var result = returnLine.Success ? returnLine.Value : null;
            _fileName = result;
        }

        public override string ToString()
        {
            return "HtmlCutterZippyShare[" +
                   "_link = " + _link + "," +
                   "_htmlCode = " + _htmlCode + "," +
                   "_prefix = " + _prefix + "," +
                   "_directLink = " + _directLink + "," +
                   "_fileName = " + _fileName + "," +
                   "]";
        }
    }
}