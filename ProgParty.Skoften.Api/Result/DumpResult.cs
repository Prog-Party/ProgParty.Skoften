namespace ProgParty.Skoften.Api.Result
{
    public class DumpResult
    {
        public string Url { get; set; }
        public bool IsGif
        {
            get
            {
                if (string.IsNullOrEmpty(Url))
                    return false;
                if (Url.EndsWith(".gif"))
                    return true;
                return false;
            }
        }
        
        public bool IsNoGif => !IsGif;
    }
}
