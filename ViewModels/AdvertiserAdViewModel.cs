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
    }
}