namespace ProgParty.Skoften.Api.Result
{
    public class OverviewResult
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public int Index { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
