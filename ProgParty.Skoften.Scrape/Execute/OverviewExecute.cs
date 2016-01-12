using ProgParty.Skoften.Scrape.Scrape;

namespace ProgParty.Skoften.Scrape.Execute
{
    public class OverviewExecute
    {
        internal Parameter.OverviewParameter Parameters = new Parameter.OverviewParameter();

        public bool Execute()
        {
            try
            {
                new OverviewScrape(Parameters).Execute();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
