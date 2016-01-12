using HtmlAgilityPack;
using ProgParty.Skoften.Api.Result;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ProgParty.Skoften.Api.Scrape
{
    internal class DumpScrape
    {
        private Parameter.DumpParameter Parameters { get; set; }

        public DumpScrape(Parameter.DumpParameter parameters)
        {
            Parameters = parameters;
        }

        public List<DumpResult> Execute()
        {
            string query = string.Empty;

            string url = Parameters.Url;

            using (HttpClient client = new HttpClient())
            {
                if (Parameters.Type == Parameter.OverviewType.PicGif)
                    client.DefaultRequestHeaders.Host = "www.skoften.net";
                else if (Parameters.Type == Parameter.OverviewType.EroDump)
                    client.DefaultRequestHeaders.Host = "babes.skoften.net";

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.125 Safari/537.36");
                //client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                return ConvertToResult(result);
            }
        }

        public List<DumpResult> ConvertToResult(string result)
        {
            List<DumpResult> dumpResult = new List<DumpResult>();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            var node = document.DocumentNode;

            var imgNodes = node.Descendants("img").Where(c => c.Attributes["class"]?.Value.Contains("lazyload") ?? false);
            if (imgNodes == null || !imgNodes.Any())
                return dumpResult;

            foreach (var imgNode in imgNodes)
            {
                dumpResult.Add(ConvertSingleResult(imgNode));
            }
            
            return dumpResult;
        }

        public DumpResult ConvertSingleResult(HtmlNode node)
        {
            DumpResult result = new DumpResult();

            HtmlAttribute srcAttr = null;
            if (Parameters.Type == Parameter.OverviewType.PicGif)
                srcAttr = node.Attributes["data-src"];
            else if (Parameters.Type == Parameter.OverviewType.EroDump)
                srcAttr = node.Attributes["src"];

            result.Url = srcAttr?.Value ?? string.Empty;
            result.Url = result.Url.Trim();

            return result;
        }
    }
}
