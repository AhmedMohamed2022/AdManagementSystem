using AdManagementSystem.ViewModels;
using System.Threading.Tasks;
namespace YourNamespace.Services.Interfaces
{
    public interface IReportExportService
    {
        Task<byte[]> ExportAdminReportCsvAsync(AdminReportViewModel report);
        Task<byte[]> ExportAdvertiserReportCsvAsync(AdvertiserReportViewModel report);
        Task<byte[]> ExportPublisherReportCsvAsync(PublisherReportViewModel report);

        Task<byte[]> ExportAdminReportPdfAsync(AdminReportViewModel report);
        Task<byte[]> ExportAdvertiserReportPdfAsync(AdvertiserReportViewModel report);
        Task<byte[]> ExportPublisherReportPdfAsync(PublisherReportViewModel report);
    }
}
