using AdManagementSystem.ViewModels;

namespace AdManagementSystem.Services
{
    // Services/Interfaces/IReportService.cs
    public interface IReportService
    {
        Task<AdminReportViewModel> GetAdminReportAsync(ReportFilterViewModel filter);
        Task<AdvertiserReportViewModel> GetAdvertiserReportAsync(string advertiserId, ReportFilterViewModel filter);
        Task<PublisherReportViewModel> GetPublisherReportAsync(string publisherId, ReportFilterViewModel filter);
    }

}
