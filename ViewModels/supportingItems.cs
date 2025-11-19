using AdManagementSystem.Models.Enums;

namespace AdManagementSystem.ViewModels
{
    public class RegionReportItem
    {
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public long Impressions { get; set; }
        public long Clicks { get; set; }
    }

    public class EntityPerformanceItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public decimal CTR => Impressions == 0 ? 0 :
                              Math.Round((decimal)Clicks / Impressions * 100, 2);
        public AdStatus Status { get; set; }
    }

    public class AdPerformanceItem : EntityPerformanceItem
    {
        public decimal Spend { get; set; }
    }

    public class WebsitePerformanceItem : EntityPerformanceItem
    {
        public decimal Earnings { get; set; }
    }

}
