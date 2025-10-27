namespace AdSystem.ViewModels
{
    /// <summary>
    /// View model for Admin Dashboard — provides a global overview of the system.
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalAdvertisers { get; set; }
        public int TotalPublishers { get; set; }
        public int TotalAds { get; set; }
        public int TotalWebsites { get; set; }
        public int PendingWebsites { get; set; }
        public int ActiveAds { get; set; }

        // Optional future charts or analytics
        public List<string>? TopAdvertisers { get; set; }
        public List<string>? TopWebsites { get; set; }
    }
}
