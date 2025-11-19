using AdSystem.ViewModels;

namespace AdManagementSystem.ViewModels
{
    // ViewModels/Reports/AdvertiserReportViewModel.cs
    public class AdvertiserReportViewModel
    {
        public long TotalImpressions { get; set; }
        public long TotalClicks { get; set; }
        public decimal CTR => TotalImpressions == 0 ? 0 :
                              Math.Round((decimal)TotalClicks / TotalImpressions * 100, 2);

        public decimal TotalSpend { get; set; }

        public List<AdPerformanceItem> Ads { get; set; } = new();
        public List<RegionReportItem> Regions { get; set; } = new();
    }

}
