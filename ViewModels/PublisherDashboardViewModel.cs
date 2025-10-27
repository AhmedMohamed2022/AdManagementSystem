namespace AdSystem.ViewModels
{
    /// <summary>
    /// View model for Publisher Dashboard — shows website approval and performance stats.
    /// </summary>
    public class PublisherDashboardViewModel
    {
        public int TotalWebsites { get; set; }
        public int ApprovedSites { get; set; }
        public int PendingSites { get; set; }
        public int TotalImpressions { get; set; }
        public int TotalClicks { get; set; }

        // Optional future: website-level metrics
        public List<PublisherWebsiteStat>? WebsiteStats { get; set; }
    }

    /// <summary>
    /// Helper model for detailed per-website metrics.
    /// </summary>
    public class PublisherWebsiteStat
    {
        public string Name { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
    }
}
