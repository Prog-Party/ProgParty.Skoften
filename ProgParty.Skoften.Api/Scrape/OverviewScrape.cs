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
                if (Parameters.Type == Parameter.OverviewType.PicGif)
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
            switch(Parameters.Type)
            {
                case Parameter.OverviewType.PicGif:
                    string picBaseurl = $"http://www.skoften.net/galleries";
                    if (string.IsNullOrEmpty(Parameters.Category))
                    {
                        if (Parameters.Paging != 0)
                            picBaseurl = $"{picBaseurl}/ P{Parameters.Paging}";
                    }
                    else
                    {
                        picBaseurl = $"{picBaseurl}/search&category={Parameters.Category}";
                        if (Parameters.Paging != 0)
                            picBaseurl = $"{picBaseurl}&offset=0/P{Parameters.Paging}";
                    }
                    return picBaseurl;
                case Parameter.OverviewType.EroDump:
                    string eroBaseUrl = "http://babes.skoften.net/";

                    if (Parameters.Paging != 0)
                        eroBaseUrl = $"{eroBaseUrl}/ P{Parameters.Paging}";

                    return eroBaseUrl;
                default:
                    throw new System.Exception("Not implemented the parameter: " + Parameters.Type);
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

            if (overviewResult.Count > 12)
                overviewResult = overviewResult.Take(12).ToList();

            return overviewResult;
        }

        public OverviewResult ConvertSingleResult(HtmlNode node)
        {
            OverviewResult result = new OverviewResult();
            result.Url = node.Descendants("a").SingleOrDefault()?.Attributes["href"]?.Value;
            result.Type = node.Descendants("h4").SingleOrDefault()?.InnerText;
            result.Name = System.Net.WebUtility.HtmlDecode(node.Descendants("h2").SingleOrDefault()?.InnerText);
            result.ImageUrl = node.Descendants("img").FirstOrDefault()?.Attributes["src"]?.Value;

            if (!result.Url.StartsWith("http://"))
            {
                if(Parameters.Type == Parameter.OverviewType.PicGif) 
                    result.Url = $"http://www.skoften.net/{result.Url}";
                else if (Parameters.Type == Parameter.OverviewType.EroDump)
                    result.Url = $"http://babes.skoften.net/{result.Url}";

            }
            return result;
        }
    }
}
