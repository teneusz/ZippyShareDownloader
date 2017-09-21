using System;
using System.Diagnostics;
using System.Net;
using log4net;
using Expression = NCalc.Expression;

namespace ZippyShareDownloader.Html.Cutter
{
    public class HtmlCutterZippyShare : IHtmlLinkCutter
    {

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

        private static string GetDirectLinkFromLink(string htmlCode, string prefix)
        {
            log.Debug("start -- GetDirectLinkFromLink(string,string)");
            log.Debug(htmlCode);
            log.Debug(prefix);
            var link = htmlCode;
            link = link.Remove(0, link.IndexOf("/d/", StringComparison.Ordinal));
            var count = link.Length - link.IndexOf("\";", StringComparison.Ordinal);
            link = link.Remove(link.IndexOf("\";", StringComparison.Ordinal), count);

            var mathExpression = link.Remove(0, link.IndexOf('(') + 1);
            var secondBracket = mathExpression.IndexOf(')');
            mathExpression = mathExpression.Remove(secondBracket, mathExpression.Length - secondBracket);
            var expression = new Expression(mathExpression);


            link = link.Replace("\" + (" + mathExpression + ") + \"", expression.Evaluate().ToString());
            log.Debug(prefix + link);
            log.Debug("start -- GetDirectLinkFromLink(string,string)");

            return prefix + link;
        }

        public string ServiceName { get; } = "ZippyShare";
    }
}