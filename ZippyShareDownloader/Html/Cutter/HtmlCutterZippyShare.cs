using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using ZippyShareDownloader.util;
using Expression = NCalc.Expression;

namespace ZippyShareDownloader.Html.Cutter
{
    public class HtmlCutterZippyShare : IHtmlLinkCutter
    {
        private static readonly string JS_FORMAT =
            @"function download(){{{0} {1}}}";

        private static readonly ILog log = LogManager.GetLogger(typeof(HtmlCutterZippyShare));

        public string GetDirectLinkFromLink(string link)
        {
            log.Debug("start -- GetDirectionLinkFromLink(string)");
            log.Debug("link = " + link);
            var prefix = "";
            var htmlCode = link;
            if (!link.StartsWith("http"))
            {
                htmlCode = "http://" + link;
            }
            prefix = htmlCode.Substring(0, htmlCode.IndexOf(".com", StringComparison.Ordinal) + 4);
            using (var client = new WebClient())
            {
                if (link.Length > 0)
                {
                    htmlCode = client.DownloadString(htmlCode);
                }
            }
            log.Debug("end -- GetDirectionLinkFromLink(string)");
            return GetDirectLinkFromLink(htmlCode, prefix);
        }

        private string GetDirectLinkFromLink(string htmlCode, string prefix)
        {
            log.Debug("start -- GetDirectLinkFromLink(string,string)");
            log.Debug(htmlCode);
            log.Debug(prefix);
            var javascriptVariableA = GetVariableA(htmlCode);
            var javascriptReturnLine = getReturnLine(htmlCode);
            var jsScript = string.Format(JS_FORMAT, javascriptVariableA, javascriptReturnLine);
            var link = "";
            using (ScriptEngine engine = new ScriptEngine("jscript"))
            {
                ParsedScript parsed = engine.Parse(jsScript);
                link = (string) parsed.CallMethod("download");
            }

            return prefix + link;
        }

        private string getReturnLine(string htmlCode)
        {
            var returnLine = Regex.Match(htmlCode, "(document\\.getElementById\\(\'dlbutton\'\\).href = \"\\/d\\/\\S*;)");
            return returnLine.Success ? "return " + returnLine.Value.Replace("document.getElementById('dlbutton').href = ","") : null;
        }

        private static string GetVariableA(string htmlCode)
        {
            var indexOfVarA = Regex.Match(htmlCode, @"(var a = [\d]{1,}[\S]{1,};)");
            return indexOfVarA.Success ? indexOfVarA.Value : null;
        }

        public string ServiceName { get; } = "ZippyShare";
    }
}