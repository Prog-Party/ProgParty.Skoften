using ProgParty.Skoften.Api.Result;
using ProgParty.Skoften.Api.Scrape;
using System.Collections.Generic;

namespace ProgParty.Skoften.Api.Execute
{
    public class OverviewExecute
    {
        public Parameter.OverviewParameter Parameters = new Parameter.OverviewParameter();

        public List<OverviewResult> Result;

        public bool Execute()
        {
            try
            {
                Result = new OverviewScrape(Parameters).Execute();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
