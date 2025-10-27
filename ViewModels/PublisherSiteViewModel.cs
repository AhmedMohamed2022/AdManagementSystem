namespace AdSystem.ViewModels
{
    public class PublisherSiteViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Url { get; set; }

        // computed aggregates
        public int AdsCount { get; set; }
        public int TotalClicks { get; set; }
        public int TotalImpressions { get; set; }

        // creation date - computed (see note below)
        public DateTime CreatedAt { get; set; }
    }
}
