using System;
using System.Diagnostics;
using System.Net;
using Expression = NCalc.Expression;

namespace ZippyShareDownloader.Html.Cutter
{
    public class HtmlCutterZippyShare : IHtmlLinkCutter
    {
        public string GetDirectLinkFromLink(string link)
        {
            Debug.Print(link);
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
            return GetDirectLinkFromLink(htmlCode, prefix);
        }

        private static string GetDirectLinkFromLink(string htmlCode, string prefix)
        {
            var link = htmlCode;
            link = link.Remove(0, link.IndexOf("/d/", StringComparison.Ordinal));
            var count = link.Length - link.IndexOf("\";", StringComparison.Ordinal);
            link = link.Remove(link.IndexOf("\";", StringComparison.Ordinal), count);

            var mathExpression = link.Remove(0, link.IndexOf('(') + 1);
            var secondBracket = mathExpression.IndexOf(')');
            mathExpression = mathExpression.Remove(secondBracket, mathExpression.Length - secondBracket);
            var expression = new Expression(mathExpression);


            link = link.Replace("\" + (" + mathExpression + ") + \"", expression.Evaluate().ToString());
            Debug.Print(prefix + link);

            return prefix + link;
        }

        public string ServiceName { get; } = "ZippyShare";
    }
}