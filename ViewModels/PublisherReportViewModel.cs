namespace AdManagementSystem.ViewModels
{
    // ViewModels/Reports/PublisherReportViewModel.cs
    public class PublisherReportViewModel
    {
        public long TotalImpressions { get; set; }
        public long TotalClicks { get; set; }
        public decimal CTR => TotalImpressions == 0 ? 0 :
                              Math.Round((decimal)TotalClicks / TotalImpressions * 100, 2);

        public decimal TotalEarnings { get; set; }

        public List<WebsitePerformanceItem> Websites { get; set; } = new();
        public List<RegionReportItem> Regions { get; set; } = new();
    }

}
