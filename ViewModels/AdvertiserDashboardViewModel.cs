namespace AdSystem.ViewModels
{
    /// <summary>
    /// View model for Advertiser Dashboard — summarizes ad performance and stats.
    /// </summary>
    public class AdvertiserDashboardViewModel
    {
        public int TotalAds { get; set; }
        public int ApprovedAds { get; set; }
        public int PendingAds { get; set; }
        public int TotalClicks { get; set; }
        public int TotalImpressions { get; set; }

        // Optional future: per-ad statistics for chart display
        public List<AdPerformanceItem>? AdPerformance { get; set; }
    }

    /// <summary>
    /// Helper model for ad-level performance visualization.
    /// </summary>
    public class AdPerformanceItem
    {
        public string Title { get; set; } = string.Empty;
        public int Clicks { get; set; }
        public int Impressions { get; set; }
    }
}
