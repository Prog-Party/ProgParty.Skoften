using HtmlAgilityPack;
using ProgParty.Skoften.Api.Result;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ProgParty.Skoften.Api.Scrape
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
            string url = ConstructUrl();

            using (HttpClient client = new HttpClient())
            {
                if (Parameters.Type == Parameter.OverviewType.PicDump
                    || Parameters.Type == Parameter.OverviewType.GifDump)
                    client.DefaultRequestHeaders.Host = "www.skoften.net";
                if (Parameters.Type == Parameter.OverviewType.EroDump)
                    client.DefaultRequestHeaders.Host = "babes.skoften.net";
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.125 Safari/537.36");
                //client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                var response = client.GetAsync(url).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                return ConvertToResult(result);
            }
        }

        public string ConstructUrl()
        {
            var baseUrl = "";
            switch(Parameters.Type)
            {
                case Parameter.OverviewType.GifDump:
                    baseUrl = "http://www.skoften.net/ajax/entriesTags/skoften/gifdump/";
                    break;
                case Parameter.OverviewType.PicDump:
                    baseUrl = "http://www.skoften.net/ajax/entriesTags/skoften/picdump/";
                    break;
                case Parameter.OverviewType.EroDump:
                    baseUrl = "http://babes.skoften.net/ajax/entriesBabes/babes/";
                    break; ;
                default:
                    throw new System.Exception("Not implemented the parameter: " + Parameters.Type);
            }

            if (Parameters.Paging != 0)
                baseUrl = $"{baseUrl}/P{Parameters.Paging}";

            return baseUrl;
        }

        public List<OverviewResult> ConvertToResult(string result)
        {
            List<OverviewResult> overviewResult = new List<OverviewResult>();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            var node = document.DocumentNode;

            string classContains = "grid-item small";
            if (Parameters.Type == Parameter.OverviewType.EroDump)
                classContains = "grid-item large";

            var divs = node.Descendants("div").Where(c => c.Attributes["class"]?.Value?.Contains(classContains) ?? false);
            if (divs == null)
                return overviewResult;

            foreach (var gridItem in divs)
            {
                overviewResult.Add(ConvertSingleResult(gridItem));
            }

            if (overviewResult.Count > 12)
                overviewResult = overviewResult.Take(12).ToList();

            return overviewResult;
        }

        public OverviewResult ConvertSingleResult(HtmlNode node)
        {
            OverviewResult result = new OverviewResult();
            result.Url = node.Descendants("a").FirstOrDefault()?.Attributes["href"]?.Value;
            result.Type = node.Descendants("p").FirstOrDefault(p => p.Attributes["class"]?.Value?.Contains("description") ?? false)?.InnerText;
            result.Name = System.Net.WebUtility.HtmlDecode(node.Descendants("h3").FirstOrDefault()?.InnerText);
            result.ImageUrl = node.Descendants("img").FirstOrDefault()?.Attributes["src"]?.Value;

            if (!result.Url.StartsWith("http://"))
            {
                if(Parameters.Type == Parameter.OverviewType.PicDump
                    || Parameters.Type == Parameter.OverviewType.GifDump) 
                    result.Url = $"http://www.skoften.net/{result.Url}";
                else if (Parameters.Type == Parameter.OverviewType.EroDump)
                    result.Url = $"http://babes.skoften.net/{result.Url}";

            }
            return result;
        }
    }
}
