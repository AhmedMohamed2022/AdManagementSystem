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
        // ✅ Pricing
        public decimal AppliedCPM { get; set; }
        public decimal AppliedCPC { get; set; }
        public string PricingRuleType { get; set; } = "";
        public string PricingCountry { get; set; } = "";
        public string PricingCity { get; set; } = "";

        // ✅ Cost Estimates
        public decimal EstimatedSpend { get; set; }

        // Optional future: per-ad statistics for chart display
        public List<AdPerformanceItem>? AdPerformance { get; set; }
        public List<AdBillingRow> BillingRows { get; set; } = new();

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
    public class AdBillingRow
    {
        public int AdId { get; set; }
        public string AdName { get; set; } = "";
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public decimal CPM { get; set; }
        public decimal CPC { get; set; }
        public decimal TotalCost { get; set; }
    }
}
