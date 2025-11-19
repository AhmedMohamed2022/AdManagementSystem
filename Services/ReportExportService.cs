using AdManagementSystem.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using YourNamespace.Services.Interfaces;

namespace YourNamespace.Services
{
    public class ReportExportService : IReportExportService
    {
        // ---------- CSV ----------

        public Task<byte[]> ExportAdminReportCsvAsync(AdminReportViewModel report)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Name,Impressions,Clicks,CTR,Profit");

            foreach (var p in report.TopPublishers)
                csv.AppendLine($"{p.Name},{p.Impressions},{p.Clicks},{p.CTR}");

            return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
        }

        public Task<byte[]> ExportAdvertiserReportCsvAsync(AdvertiserReportViewModel report)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Ad,Impressions,Clicks,CTR,Spend");

            foreach (var ad in report.Ads)
                csv.AppendLine($"{ad.Name},{ad.Impressions},{ad.Clicks},{ad.CTR},{ad.Spend}");

            return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
        }

        public Task<byte[]> ExportPublisherReportCsvAsync(PublisherReportViewModel report)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Website,Impressions,Clicks,CTR,Earnings");

            foreach (var w in report.Websites)
                csv.AppendLine($"{w.Name},{w.Impressions},{w.Clicks},{w.CTR},{w.Earnings}");

            return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
        }

        // ---------- PDF ----------

        public Task<byte[]> ExportAdminReportPdfAsync(AdminReportViewModel report)
        {
                using (var memoryStream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph("Admin Report"));
                var table = new PdfPTable(5); // 5 columns
                table.AddCell("Name");
                table.AddCell("Impressions");
                table.AddCell("Clicks");
                table.AddCell("CTR");
                table.AddCell("Profit");

                foreach (var p in report.TopPublishers)
                {
                    table.AddCell(p.Name);
                    table.AddCell(p.Impressions.ToString());
                    table.AddCell(p.Clicks.ToString());
                    table.AddCell(p.CTR.ToString("0.00") + "%");
                    //table.AddCell(p.Earnings.ToString("0.00"));
                }

                document.Add(table);
                document.Close();

                return Task.FromResult(memoryStream.ToArray());
            }
        }

        public Task<byte[]> ExportAdvertiserReportPdfAsync(AdvertiserReportViewModel report)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph("Advertiser Report"));
                var table = new PdfPTable(5); // 5 columns
                table.AddCell("Ad");
                table.AddCell("Impressions");
                table.AddCell("Clicks");
                table.AddCell("CTR");
                table.AddCell("Spend");

                foreach (var ad in report.Ads)
                {
                    table.AddCell(ad.Name);
                    table.AddCell(ad.Impressions.ToString());
                    table.AddCell(ad.Clicks.ToString());
                    table.AddCell(ad.CTR.ToString("0.00") + "%");
                    table.AddCell(ad.Spend.ToString("0.00"));
                }

                document.Add(table);
                document.Close();

                return Task.FromResult(memoryStream.ToArray());
            }
        }

        public Task<byte[]> ExportPublisherReportPdfAsync(PublisherReportViewModel report)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph("Publisher Report"));
                var table = new PdfPTable(5); // 5 columns
                table.AddCell("Website");
                table.AddCell("Impressions");
                table.AddCell("Clicks");
                table.AddCell("CTR");
                table.AddCell("Earnings");

                foreach (var w in report.Websites)
                {
                    table.AddCell(w.Name);
                    table.AddCell(w.Impressions.ToString());
                    table.AddCell(w.Clicks.ToString());
                    table.AddCell(w.CTR.ToString("0.00") + "%");
                    table.AddCell(w.Earnings.ToString("0.00"));
                }

                document.Add(table);
                document.Close();

                return Task.FromResult(memoryStream.ToArray());
            }
        }
    }
}
