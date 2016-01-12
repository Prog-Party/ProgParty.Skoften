using HtmlAgilityPack;
using ProgParty.Skoften.Scrape.Result;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ProgParty.Skoften.Scrape.Scrape
{
    internal class OverviewScrape
    {
        private Parameter.OverviewParameter Parameters { get; set; }

        public OverviewScrape(Parameter.OverviewParameter parameters)
        {
            Parameters = parameters;
        }

        public List<OverviewResult> Execute()
        {
            string query = string.Empty;

            string url = $"http://www.skoften.net/galleries";
            if (!string.IsNullOrEmpty(Parameters.Category))
                url += $"/search&category={Parameters.Category}";

            using (var handler = new HttpClientHandler())//{ CookieContainer = Auth.CookieContainer }
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Host = "www.skoften.net";
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.125 Safari/537.36");
                    //client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                    var response = client.GetAsync(url).Result;
                    var result = response.Content.ReadAsStringAsync().Result;

                    return ConvertToResult(result);
                }
            }
        }

        public List<OverviewResult> ConvertToResult(string result)
        {
            List<OverviewResult> overviewResult = new List<OverviewResult>();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            var node = document.DocumentNode;

            var ulNode = node.Descendants("ul").SingleOrDefault(c => c.Attributes["class"]?.Value.Contains("channelBlocks") ?? false);
            if (ulNode == null)
                return overviewResult;

            var lis = ulNode.Descendants("li");

            foreach (var li in lis)
            {
                overviewResult.Add(ConvertSingleResult(li));
            }

            return overviewResult;
        }

        public OverviewResult ConvertSingleResult(HtmlNode node)
        {
            OverviewResult result = new OverviewResult();
            result.Url = node.Descendants("a").SingleOrDefault()?.Attributes["href"]?.Value;
            result.Type = node.Descendants("h4").SingleOrDefault()?.InnerText;
            result.Name = node.Descendants("h2").SingleOrDefault()?.InnerText;
            return result;
        }
    }
}
