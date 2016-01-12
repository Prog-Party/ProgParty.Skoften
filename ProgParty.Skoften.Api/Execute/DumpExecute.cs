using ProgParty.Skoften.Api.Result;
using ProgParty.Skoften.Api.Scrape;
using System.Collections.Generic;

namespace ProgParty.Skoften.Api.Execute
{
    public class DumpExecute
    {
        public Parameter.DumpParameter Parameters = new Parameter.DumpParameter();

        public List<DumpResult> Result;

        public bool Execute()
        {
            try
            {
                Result = new DumpScrape(Parameters).Execute();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
