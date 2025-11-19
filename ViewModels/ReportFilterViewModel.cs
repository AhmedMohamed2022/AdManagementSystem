namespace AdManagementSystem.ViewModels
{
    // ViewModels/Reports/ReportFilterViewModel.cs
    public class ReportFilterViewModel
    {
        // Date range
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Optional targeting filters
        public string? Country { get; set; }
        public string? City { get; set; }

        // Optional context identifiers
        public int? AdId { get; set; }
        public int? WebsiteId { get; set; }
    }

}
