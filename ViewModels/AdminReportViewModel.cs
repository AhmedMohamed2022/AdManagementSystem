namespace AdManagementSystem.ViewModels
{
    // ViewModels/Reports/AdminReportViewModel.cs
    public class AdminReportViewModel
    {
        // Global KPIs
        public long TotalImpressions { get; set; }
        public long TotalClicks { get; set; }
        public decimal CTR => TotalImpressions == 0 ? 0 :
                              Math.Round((decimal)TotalClicks / TotalImpressions * 100, 2);

        // Finance summary
        public decimal TotalAdvertiserSpend { get; set; }
        public decimal TotalPublisherEarnings { get; set; }
        public decimal PlatformProfit => TotalAdvertiserSpend - TotalPublisherEarnings;

        // Regional breakdown
        public List<RegionReportItem> Regions { get; set; } = new();

        // Top performing entities
        public List<EntityPerformanceItem> TopAds { get; set; } = new();
        public List<EntityPerformanceItem> TopPublishers { get; set; } = new();
    }

}
