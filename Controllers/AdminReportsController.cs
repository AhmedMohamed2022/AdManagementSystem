using AdManagementSystem.Services;
using AdManagementSystem.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;


[Authorize(Roles = "Admin")]
[Route("Admin/Reports")]
public class AdminReportsController : Controller
{
    private readonly IReportService _reportService;

    public AdminReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ReportFilterViewModel filter)
    {
        var model = await _reportService.GetAdminReportAsync(filter);
        ViewBag.Filter = filter;
        return View(model);
    }

    // ---------------- CSV EXPORT ----------------
    //[HttpGet("ExportCsv")]
    //public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel filter)
    //{
    //    var report = await _reportService.GetAdminReportAsync(filter);

    //    var sb = new StringBuilder();
    //    sb.AppendLine("Country,City,Impressions,Clicks");

    //    foreach (var region in report.Regions)
    //        sb.AppendLine($"{region.Country},{region.City},{region.Impressions},{region.Clicks}");

    //    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
    //    return File(bytes, "text/csv", "AdminReport.csv");
    //}

    //// ---------------- PDF EXPORT ----------------
    //[HttpGet("ExportPdf")]
    //public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterViewModel filter)
    //{
    //    var report = await _reportService.GetAdminReportAsync(filter);
    //    using var ms = new MemoryStream();
    //    var doc = new Document(PageSize.A4);
    //    PdfWriter.GetInstance(doc, ms);
    //    doc.Open();

    //    doc.Add(new Paragraph("Admin Report"));
    //    doc.Add(new Paragraph($"Generated: {DateTime.UtcNow}"));
    //    doc.Add(new Paragraph(" "));

    //    var table = new PdfPTable(4);
    //    table.AddCell("Country");
    //    table.AddCell("City");
    //    table.AddCell("Impressions");
    //    table.AddCell("Clicks");

    //    foreach (var region in report.Regions)
    //    {
    //        table.AddCell(region.Country);
    //        table.AddCell(region.City);
    //        table.AddCell(region.Impressions.ToString());
    //        table.AddCell(region.Clicks.ToString());
    //    }

    //    doc.Add(table);
    //    doc.Close();

    //    return File(ms.ToArray(), "application/pdf", "AdminReport.pdf");
    //}
    [HttpGet("ExportCsv")]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel filter)
    {
        var report = await _reportService.GetAdminReportAsync(filter);
        var sb = new StringBuilder();

        // UTF-8 BOM for proper Excel Arabic support
        sb.Append("\uFEFF");

        // Header with report info
        sb.AppendLine("تقرير إداري شامل - Admin Report");
        sb.AppendLine($"تاريخ التوليد: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"الفترة: {filter?.StartDate?.ToString("yyyy-MM-dd") ?? "غير محدد"} إلى {filter?.EndDate?.ToString("yyyy-MM-dd") ?? "غير محدد"}");
        sb.AppendLine();

        // Summary Section
        sb.AppendLine("=== الملخص العام ===");
        sb.AppendLine($"إجمالي المشاهدات,{report.TotalImpressions:N0}");
        sb.AppendLine($"إجمالي النقرات,{report.TotalClicks:N0}");
        sb.AppendLine($"معدل النقر (CTR),{report.CTR:F2}%");
        sb.AppendLine($"أرباح المنصة,${report.PlatformProfit:N2}");
        sb.AppendLine();

        // Regional Performance
        sb.AppendLine("=== الأداء الجغرافي ===");
        sb.AppendLine("الدولة,المدينة,المشاهدات,النقرات,معدل النقر");
        foreach (var region in report.Regions)
        {
            var ctr = region.Impressions > 0 ? (region.Clicks * 100.0 / region.Impressions) : 0;
            sb.AppendLine($"{region.Country},{region.City},{region.Impressions:N0},{region.Clicks:N0},{ctr:F2}%");
        }
        sb.AppendLine();

        // Top Publishers
        if (report.TopPublishers?.Any() == true)
        {
            sb.AppendLine("=== أفضل الناشرين ===");
            sb.AppendLine("الاسم,المشاهدات,النقرات,معدل النقر");
            foreach (var publisher in report.TopPublishers)
            {
                sb.AppendLine($"{publisher.Name},{publisher.Impressions:N0},{publisher.Clicks:N0},{publisher.CTR:F2}%");
            }
            sb.AppendLine();
        }

        // Top Ads
        if (report.TopAds?.Any() == true)
        {
            sb.AppendLine("=== أفضل الإعلانات ===");
            sb.AppendLine("اسم الإعلان,النقرات");
            foreach (var ad in report.TopAds)
            {
                sb.AppendLine($"{ad.Name},{ad.Clicks:N0}");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"AdminReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    // ---------------- PDF EXPORT ----------------
    [HttpGet("ExportPdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterViewModel filter)
    {
        var report = await _reportService.GetAdminReportAsync(filter);
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4, 40, 40, 50, 50);
        var writer = PdfWriter.GetInstance(doc, ms);
        doc.Open();

        // Setup Arabic font (you'll need to add Arial Unicode or similar font that supports Arabic)
        // For now, using default font for English
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(32, 101, 209));
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.DARK_GRAY);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);

        // Title
        var title = new Paragraph("ADMIN REPORT", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 10
        };
        doc.Add(title);

        // Report Info
        var infoTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 20 };
        infoTable.SetWidths(new float[] { 1, 2 });

        AddInfoCell(infoTable, "Generated:", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), normalFont, boldFont);
        AddInfoCell(infoTable, "Period:",
            $"{filter?.StartDate?.ToString("yyyy-MM-dd") ?? "N/A"} to {filter?.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}",
            normalFont, boldFont);
        if (!string.IsNullOrEmpty(filter?.Country))
            AddInfoCell(infoTable, "Country:", filter.Country, normalFont, boldFont);
        if (!string.IsNullOrEmpty(filter?.City))
            AddInfoCell(infoTable, "City:", filter.City, normalFont, boldFont);

        doc.Add(infoTable);

        // Summary Section
        doc.Add(new Paragraph("PERFORMANCE SUMMARY", headerFont) { SpacingAfter = 10 });

        var summaryTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
        summaryTable.DefaultCell.Padding = 8;
        summaryTable.DefaultCell.BackgroundColor = new BaseColor(240, 242, 245);
        summaryTable.DefaultCell.Border = Rectangle.NO_BORDER;

        AddSummaryCard(summaryTable, "Total Impressions", report.TotalImpressions.ToString("N0"), new BaseColor(32, 101, 209));
        AddSummaryCard(summaryTable, "Total Clicks", report.TotalClicks.ToString("N0"), new BaseColor(0, 171, 85));
        AddSummaryCard(summaryTable, "CTR", $"{report.CTR:F2}%", new BaseColor(255, 171, 0));
        AddSummaryCard(summaryTable, "Platform Profit", $"${report.PlatformProfit:N2}", new BaseColor(16, 185, 129));

        doc.Add(summaryTable);

        // Regional Performance
        if (report.Regions?.Any() == true)
        {
            doc.Add(new Paragraph("REGIONAL PERFORMANCE", headerFont) { SpacingAfter = 10 });

            var regionTable = new PdfPTable(5) { WidthPercentage = 100, SpacingAfter = 20 };
            regionTable.SetWidths(new float[] { 2, 2, 2, 2, 2 });

            AddTableHeader(regionTable, new[] { "Country", "City", "Impressions", "Clicks", "CTR" }, boldFont);

            foreach (var region in report.Regions)
            {
                var ctr = region.Impressions > 0 ? (region.Clicks * 100.0 / region.Impressions) : 0;
                AddTableRow(regionTable, new[]
                {
                region.Country,
                region.City,
                region.Impressions.ToString("N0"),
                region.Clicks.ToString("N0"),
                $"{ctr:F2}%"
            }, normalFont);
            }

            doc.Add(regionTable);
        }

        // Top Publishers
        if (report.TopPublishers?.Any() == true)
        {
            doc.Add(new Paragraph("TOP PUBLISHERS", headerFont) { SpacingAfter = 10 });

            var publisherTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
            publisherTable.SetWidths(new float[] { 3, 2, 2, 2 });

            AddTableHeader(publisherTable, new[] { "Publisher Name", "Impressions", "Clicks", "CTR" }, boldFont);

            foreach (var publisher in report.TopPublishers)
            {
                AddTableRow(publisherTable, new[]
                {
                publisher.Name,
                publisher.Impressions.ToString("N0"),
                publisher.Clicks.ToString("N0"),
                $"{publisher.CTR:F2}%"
            }, normalFont);
            }

            doc.Add(publisherTable);
        }

        // Top Ads
        if (report.TopAds?.Any() == true)
        {
            doc.Add(new Paragraph("TOP PERFORMING ADS", headerFont) { SpacingAfter = 10 });

            var adTable = new PdfPTable(2) { WidthPercentage = 100 };
            adTable.SetWidths(new float[] { 3, 1 });

            AddTableHeader(adTable, new[] { "Ad Name", "Clicks" }, boldFont);

            foreach (var ad in report.TopAds)
            {
                AddTableRow(adTable, new[] { ad.Name, ad.Clicks.ToString("N0") }, normalFont);
            }

            doc.Add(adTable);
        }

        // Footer
        doc.Add(new Paragraph("\n"));
        var footer = new Paragraph("Generated by Ad Management Platform",
            FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY))
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(footer);

        doc.Close();

        var fileName = $"AdminReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(ms.ToArray(), "application/pdf", fileName);
    }

    // Helper methods for PDF generation
    private void AddInfoCell(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        var labelCell = new PdfPCell(new Phrase(label, labelFont))
        {
            Border = Rectangle.NO_BORDER,
            Padding = 5,
            BackgroundColor = new BaseColor(249, 250, 251)
        };
        var valueCell = new PdfPCell(new Phrase(value, valueFont))
        {
            Border = Rectangle.NO_BORDER,
            Padding = 5
        };
        table.AddCell(labelCell);
        table.AddCell(valueCell);
    }

    private void AddSummaryCard(PdfPTable table, string label, string value, BaseColor color)
    {
        var cell = new PdfPCell
        {
            Border = Rectangle.NO_BORDER,
            Padding = 10,
            BackgroundColor = new BaseColor(240, 242, 245),
            VerticalAlignment = Element.ALIGN_MIDDLE
        };

        var labelPara = new Paragraph(label, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.GRAY))
        {
            Alignment = Element.ALIGN_CENTER
        };
        var valuePara = new Paragraph(value, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, color))
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingBefore = 5
        };

        cell.AddElement(labelPara);
        cell.AddElement(valuePara);
        table.AddCell(cell);
    }

    private void AddTableHeader(PdfPTable table, string[] headers, Font font)
    {
        foreach (var header in headers)
        {
            var cell = new PdfPCell(new Phrase(header, font))
            {
                BackgroundColor = new BaseColor(32, 101, 209),
                //ForegroundColor = BaseColor.WHITE,
                Padding = 8,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Border = Rectangle.NO_BORDER
            };
            table.AddCell(cell);
        }
    }

    private void AddTableRow(PdfPTable table, string[] values, Font font)
    {
        foreach (var value in values)
        {
            var cell = new PdfPCell(new Phrase(value, font))
            {
                Padding = 8,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = new BaseColor(240, 242, 245)
            };
            table.AddCell(cell);
        }
    }
}
