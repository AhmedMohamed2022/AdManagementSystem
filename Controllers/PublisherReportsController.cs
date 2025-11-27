using AdManagementSystem.Services;
using AdManagementSystem.ViewModels;
using AdSystem.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

[Authorize(Roles = "Publisher")]
[Route("Publisher/Reports")]
public class PublisherReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PublisherReportsController(IReportService reportService, UserManager<ApplicationUser> userManager)
    {
        _reportService = reportService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ReportFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        var model = await _reportService.GetPublisherReportAsync(user.Id, filter);
        ViewBag.Filter = filter;
        return View(model);
    }
    [HttpGet("ExportCsv")]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        var report = await _reportService.GetPublisherReportAsync(user.Id, filter);
        var sb = new StringBuilder();

        // UTF-8 BOM for proper Excel Arabic support
        sb.Append("\uFEFF");

        // Header with report info
        sb.AppendLine("تقرير الناشر - Publisher Report");
        sb.AppendLine($"الناشر: {user.UserName}");
        sb.AppendLine($"تاريخ التوليد: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"الفترة: {filter?.StartDate?.ToString("yyyy-MM-dd") ?? "غير محدد"} إلى {filter?.EndDate?.ToString("yyyy-MM-dd") ?? "غير محدد"}");
        sb.AppendLine();

        // Summary Section
        sb.AppendLine("=== ملخص الأداء ===");
        sb.AppendLine($"إجمالي المشاهدات,{report.TotalImpressions:N0}");
        sb.AppendLine($"إجمالي النقرات,{report.TotalClicks:N0}");
        sb.AppendLine($"معدل النقر (CTR),{report.CTR:F2}%");
        sb.AppendLine($"إجمالي الأرباح,${report.TotalEarnings:N2}");
        sb.AppendLine($"متوسط الربح لكل ألف ظهور (CPM),${(report.TotalImpressions > 0 ? (report.TotalEarnings / report.TotalImpressions * 1000) : 0):F2}");
        sb.AppendLine();

        // Website Performance
        sb.AppendLine("=== أداء المواقع ===");
        sb.AppendLine("الموقع,المشاهدات,النقرات,معدل النقر,الأرباح,CPM");
        foreach (var site in report.Websites)
        {
            var ctr = site.Impressions > 0 ? (site.Clicks * 100.0 / site.Impressions) : 0;
            var cpm = site.Impressions > 0 ? (site.Earnings / site.Impressions * 1000) : 0;
            sb.AppendLine($"{site.Name},{site.Impressions:N0},{site.Clicks:N0},{ctr:F2}%,${site.Earnings:N2},${cpm:F2}");
        }
        sb.AppendLine();

        // Top Performing Sites
        if (report.Websites?.Any() == true)
        {
            var topSites = report.Websites.OrderByDescending(w => w.Earnings).Take(5);
            sb.AppendLine("=== أفضل 5 مواقع (حسب الأرباح) ===");
            sb.AppendLine("الترتيب,الموقع,الأرباح");
            int rank = 1;
            foreach (var site in topSites)
            {
                sb.AppendLine($"{rank},{site.Name},${site.Earnings:N2}");
                rank++;
            }
            sb.AppendLine();
        }

        // Performance Insights
        sb.AppendLine("=== رؤى الأداء ===");
        var avgCtr = report.TotalImpressions > 0 ? (report.TotalClicks * 100.0 / report.TotalImpressions) : 0;
        var avgCpm = report.TotalImpressions > 0 ? (report.TotalEarnings / report.TotalImpressions * 1000) : 0;
        sb.AppendLine($"متوسط معدل النقر,{avgCtr:F2}%");
        sb.AppendLine($"متوسط CPM,${avgCpm:F2}");
        sb.AppendLine($"عدد المواقع النشطة,{report.Websites?.Count() ?? 0}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"PublisherReport_{user.UserName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    // ---------------- PDF EXPORT ----------------
    [HttpGet("ExportPdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        var report = await _reportService.GetPublisherReportAsync(user.Id, filter);
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4, 40, 40, 50, 50);
        var writer = PdfWriter.GetInstance(doc, ms);
        doc.Open();

        // Setup fonts
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 171, 85)); // Publisher green
        var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(32, 101, 209));
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.DARK_GRAY);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);

        // Title
        var title = new Paragraph("PUBLISHER PERFORMANCE REPORT", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 10
        };
        doc.Add(title);

        // Publisher Info
        var publisherInfo = new Paragraph($"Publisher: {user.UserName}",
            FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(0, 171, 85)))
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 20
        };
        doc.Add(publisherInfo);

        // Report Info Table
        var infoTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 20 };
        infoTable.SetWidths(new float[] { 1, 2 });

        AddInfoCell(infoTable, "Generated:", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), normalFont, boldFont);
        AddInfoCell(infoTable, "Period:",
            $"{filter?.StartDate?.ToString("yyyy-MM-dd") ?? "N/A"} to {filter?.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}",
            normalFont, boldFont);
        AddInfoCell(infoTable, "Report ID:", $"PUB-{DateTime.Now:yyyyMMddHHmmss}", normalFont, boldFont);

        doc.Add(infoTable);

        // Earnings Summary - Highlighted Section
        doc.Add(new Paragraph("EARNINGS OVERVIEW", headerFont) { SpacingAfter = 10 });

        var summaryTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
        summaryTable.DefaultCell.Padding = 10;
        summaryTable.DefaultCell.BackgroundColor = new BaseColor(240, 253, 244); // Light green
        summaryTable.DefaultCell.Border = Rectangle.NO_BORDER;

        var avgCpm = report.TotalImpressions > 0 ? (report.TotalEarnings / report.TotalImpressions * 1000) : 0;

        AddSummaryCard(summaryTable, "Total Earnings", $"${report.TotalEarnings:N2}", new BaseColor(0, 171, 85));
        AddSummaryCard(summaryTable, "Total Impressions", report.TotalImpressions.ToString("N0"), new BaseColor(32, 101, 209));
        AddSummaryCard(summaryTable, "Total Clicks", report.TotalClicks.ToString("N0"), new BaseColor(255, 171, 0));
        AddSummaryCard(summaryTable, "Avg CPM", $"${avgCpm:F2}", new BaseColor(16, 185, 129));

        doc.Add(summaryTable);

        // Performance Metrics
        doc.Add(new Paragraph("PERFORMANCE METRICS", subHeaderFont) { SpacingAfter = 10 });

        var metricsTable = new PdfPTable(3) { WidthPercentage = 100, SpacingAfter = 20 };
        metricsTable.DefaultCell.Padding = 8;

        AddMetricRow(metricsTable, "Click-Through Rate (CTR)", $"{report.CTR:F2}%", normalFont, boldFont);
        AddMetricRow(metricsTable, "Active Websites", $"{report.Websites?.Count() ?? 0}", normalFont, boldFont);
        AddMetricRow(metricsTable, "Average Earnings per Site",
            $"${(report.Websites?.Any() == true ? report.TotalEarnings / report.Websites.Count() : 0):F2}",
            normalFont, boldFont);

        doc.Add(metricsTable);

        // Website Performance Details
        if (report.Websites?.Any() == true)
        {
            doc.Add(new Paragraph("WEBSITE PERFORMANCE BREAKDOWN", headerFont) { SpacingAfter = 10 });

            var websiteTable = new PdfPTable(6) { WidthPercentage = 100, SpacingAfter = 20 };
            websiteTable.SetWidths(new float[] { 3, 2, 2, 2, 2, 2 });

            AddTableHeader(websiteTable, new[] { "Website", "Impressions", "Clicks", "CTR", "Earnings", "CPM" }, boldFont);

            foreach (var site in report.Websites.OrderByDescending(w => w.Earnings))
            {
                var ctr = site.Impressions > 0 ? (site.Clicks * 100.0 / site.Impressions) : 0;
                var cpm = site.Impressions > 0 ? (site.Earnings / site.Impressions * 1000) : 0;

                AddTableRow(websiteTable, new[]
                {
                site.Name,
                site.Impressions.ToString("N0"),
                site.Clicks.ToString("N0"),
                $"{ctr:F2}%",
                $"${site.Earnings:F2}",
                $"${cpm:F2}"
            }, normalFont);
            }

            doc.Add(websiteTable);
        }

        // Top Performers Section
        if (report.Websites?.Any() == true && report.Websites.Count() >= 3)
        {
            doc.Add(new Paragraph("TOP PERFORMING WEBSITES", subHeaderFont) { SpacingAfter = 10 });

            var topTable = new PdfPTable(3) { WidthPercentage = 100, SpacingAfter = 20 };
            topTable.SetWidths(new float[] { 1, 3, 2 });

            AddTableHeader(topTable, new[] { "Rank", "Website", "Earnings" }, boldFont);

            int rank = 1;
            foreach (var site in report.Websites.OrderByDescending(w => w.Earnings).Take(5))
            {
                var rankCell = new PdfPCell(new Phrase($"#{rank}", boldFont))
                {
                    Padding = 8,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BackgroundColor = rank <= 3 ? new BaseColor(255, 243, 205) : BaseColor.WHITE, // Gold for top 3
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = new BaseColor(240, 242, 245)
                };
                topTable.AddCell(rankCell);

                AddTableCell(topTable, site.Name, normalFont);
                AddTableCell(topTable, $"${site.Earnings:N2}",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 171, 85)));

                rank++;
            }

            doc.Add(topTable);
        }

        // Insights & Recommendations
        doc.Add(new Paragraph("INSIGHTS & RECOMMENDATIONS", subHeaderFont) { SpacingAfter = 10 });

        var insightsTable = new PdfPTable(1) { WidthPercentage = 100, SpacingAfter = 10 };
        insightsTable.DefaultCell.Padding = 10;
        insightsTable.DefaultCell.BackgroundColor = new BaseColor(249, 250, 251);
        insightsTable.DefaultCell.Border = Rectangle.NO_BORDER;

        var insights = new List<string>();

        if (report.CTR < 1.0m)
        {
            insights.Add("• Your CTR is below average. Consider optimizing ad placements for better visibility.");
        }
        else if (report.CTR >= 2.0m)
        {
            insights.Add("• Excellent CTR! Your ad placements are performing very well.");
        }

        if (avgCpm < 1.0m)
        {
            insights.Add("• Your CPM is low. Focus on high-quality content to attract premium advertisers.");
        }
        else if (avgCpm >= 3.0m)
        {
            insights.Add("• Great CPM! Your content is attracting high-value advertisers.");
        }

        if (report.Websites?.Any() == true && report.Websites.Count() == 1)
        {
            insights.Add("• Consider adding more websites to diversify your revenue streams.");
        }

        if (!insights.Any())
        {
            insights.Add("• Your performance is solid. Keep maintaining quality content and strategic ad placements.");
        }

        foreach (var insight in insights)
        {
            var cell = new PdfPCell(new Phrase(insight, normalFont))
            {
                Padding = 8,
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(249, 250, 251)
            };
            insightsTable.AddCell(cell);
        }

        doc.Add(insightsTable);

        // Footer
        doc.Add(new Paragraph("\n"));
        var footer = new Paragraph("Generated by Ad Management Platform - Publisher Dashboard",
            FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY))
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(footer);

        doc.Close();

        var fileName = $"PublisherReport_{user.UserName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
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
            BackgroundColor = new BaseColor(240, 253, 244),
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
                BackgroundColor = new BaseColor(0, 171, 85), // Publisher green
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

    private void AddTableCell(PdfPTable table, string value, Font font)
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

    private void AddMetricRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        var labelCell = new PdfPCell(new Phrase(label, labelFont))
        {
            Padding = 8,
            Border = Rectangle.BOTTOM_BORDER,
            BorderColor = new BaseColor(240, 242, 245),
            Colspan = 2
        };
        var valueCell = new PdfPCell(new Phrase(value, valueFont))
        {
            Padding = 8,
            HorizontalAlignment = Element.ALIGN_RIGHT,
            Border = Rectangle.BOTTOM_BORDER,
            BorderColor = new BaseColor(240, 242, 245)
        };
        table.AddCell(labelCell);
        table.AddCell(valueCell);
    }
}
