using AdManagementSystem.Models.Enums;

namespace AdSystem.ViewModels
{
    public class AdvertiserAdViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public AdStatus Status { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int? MaxImpressions { get; internal set; }
        public int? MaxClicks { get; internal set; }
        public DateTime? StartDate { get; internal set; }= DateTime.UtcNow;
        public DateTime? EndDate { get; internal set; } 
    }
}