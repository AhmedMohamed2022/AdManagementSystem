using AdManagementSystem.Models.Enums;
using AdManagementSystem.Services;
using AdManagementSystem.ViewModels;
using AdSystem.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;


[Authorize(Roles = "Advertiser")]
[Route("Advertiser/Reports")]
public class AdvertiserReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPdfService _pdf;


    public AdvertiserReportsController(IReportService reportService, UserManager<ApplicationUser> userManager, IPdfService pdf)
    {
        _reportService = reportService;
        _userManager = userManager;
        _pdf = pdf;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ReportFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        var model = await _reportService.GetAdvertiserReportAsync(user.Id, filter);
        ViewBag.Filter = filter;
        return View(model);
    }
    //[HttpGet("ExportCsv")]
    //public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel filter)
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    var report = await _reportService.GetAdvertiserReportAsync(user.Id, filter);

    //    var sb = new StringBuilder();
    //    sb.AppendLine("Ad,Impressions,Clicks,Spend");

    //    foreach (var ad in report.Ads)
    //        sb.AppendLine($"{ad.Name},{ad.Impressions},{ad.Clicks},{ad.Spend}");

    //    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
    //    return File(bytes, "text/csv", "AdvertiserReport.csv");
    //}

    //[HttpGet("ExportPdf")]
    //public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterViewModel filter)
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    var report = await _reportService.GetAdvertiserReportAsync(user.Id, filter);

    //    using var ms = new MemoryStream();
    //    var doc = new Document(PageSize.A4);
    //    PdfWriter.GetInstance(doc, ms);
    //    doc.Open();

    //    doc.Add(new Paragraph("Advertiser Report"));
    //    doc.Add(new Paragraph($"Generated: {DateTime.UtcNow}"));
    //    doc.Add(new Paragraph(" "));

    //    var table = new PdfPTable(4);
    //    table.AddCell("Ad");
    //    table.AddCell("Impressions");
    //    table.AddCell("Clicks");
    //    table.AddCell("Spend");

    //    foreach (var ad in report.Ads)
    //    {
    //        table.AddCell(ad.Name);
    //        table.AddCell(ad.Impressions.ToString());
    //        table.AddCell(ad.Clicks.ToString());
    //        table.AddCell(ad.Spend.ToString("0.00"));
    //    }

    //    doc.Add(table);
    //    doc.Close();

    //    return File(ms.ToArray(), "application/pdf", "AdvertiserReport.pdf");
    //}
    [HttpGet("ExportCsv")]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        var report = await _reportService.GetAdvertiserReportAsync(user.Id, filter);
        var sb = new StringBuilder();

        // UTF-8 BOM for proper Excel Arabic support
        sb.Append("\uFEFF");

        // Header with report info
        sb.AppendLine("تقرير المعلن - Advertiser Report");
        sb.AppendLine($"المعلن: {user.UserName}");
        sb.AppendLine($"تاريخ التوليد: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"الفترة: {filter?.StartDate?.ToString("yyyy-MM-dd") ?? "غير محدد"} إلى {filter?.EndDate?.ToString("yyyy-MM-dd") ?? "غير محدد"}");
        sb.AppendLine();

        // Campaign Summary
        sb.AppendLine("=== ملخص الحملة الإعلانية ===");
        sb.AppendLine($"إجمالي المشاهدات,{report.TotalImpressions:N0}");
        sb.AppendLine($"إجمالي النقرات,{report.TotalClicks:N0}");
        sb.AppendLine($"معدل النقر (CTR),{report.CTR:F2}%");
        sb.AppendLine($"إجمالي الإنفاق,${report.TotalSpend:N2}");

        var cpc = report.TotalClicks > 0 ? (report.TotalSpend / report.TotalClicks) : 0;
        var cpm = report.TotalImpressions > 0 ? (report.TotalSpend / report.TotalImpressions * 1000) : 0;

        sb.AppendLine($"تكلفة النقرة (CPC),${cpc:F2}");
        sb.AppendLine($"تكلفة الألف ظهور (CPM),${cpm:F2}");
        sb.AppendLine();

        // Ad Performance Details
        if (report.Ads?.Any() == true)
        {
            sb.AppendLine("=== أداء الإعلانات ===");
            sb.AppendLine("الإعلان,الحالة,المشاهدات,النقرات,معدل النقر,الإنفاق,CPC,CPM");

            foreach (var ad in report.Ads)
            {
                var adCtr = ad.Impressions > 0 ? (ad.Clicks * 100.0 / ad.Impressions) : 0;
                var adCpc = ad.Clicks > 0 ? (ad.Spend / ad.Clicks) : 0;
                var adCpm = ad.Impressions > 0 ? (ad.Spend / ad.Impressions * 1000) : 0;

                sb.AppendLine($"{ad.Name},{ad.Status},{ad.Impressions:N0},{ad.Clicks:N0},{adCtr:F2}%,${ad.Spend:N2},${adCpc:F2},${adCpm:F2}");
            }
            sb.AppendLine();

            // Top Performing Ads
            var topAds = report.Ads.OrderByDescending(a => a.Clicks).Take(5);
            sb.AppendLine("=== أفضل 5 إعلانات (حسب النقرات) ===");
            sb.AppendLine("الترتيب,الإعلان,النقرات,معدل النقر");
            int rank = 1;
            foreach (var ad in topAds)
            {
                var adCtr = ad.Impressions > 0 ? (ad.Clicks * 100.0 / ad.Impressions) : 0;
                sb.AppendLine($"{rank},{ad.Name},{ad.Clicks:N0},{adCtr:F2}%");
                rank++;
            }
            sb.AppendLine();

            // Most Expensive Ads
            var expensiveAds = report.Ads.OrderByDescending(a => a.Spend).Take(5);
            sb.AppendLine("=== أعلى 5 إعلانات إنفاقاً ===");
            sb.AppendLine("الترتيب,الإعلان,الإنفاق,النقرات");
            rank = 1;
            foreach (var ad in expensiveAds)
            {
                sb.AppendLine($"{rank},{ad.Name},${ad.Spend:N2},{ad.Clicks:N0}");
                rank++;
            }
            sb.AppendLine();
        }

        // Regional Performance
        if (report.Regions?.Any() == true)
        {
            sb.AppendLine("=== الأداء الجغرافي ===");
            sb.AppendLine("الدولة,المدينة,المشاهدات,النقرات,معدل النقر");

            foreach (var region in report.Regions)
            {
                var regionCtr = region.Impressions > 0 ? (region.Clicks * 100.0 / region.Impressions) : 0;
                sb.AppendLine($"{region.Country},{region.City},{region.Impressions:N0},{region.Clicks:N0},{regionCtr:F2}%");
            }
            sb.AppendLine();

            // Best Performing Regions
            var topRegions = report.Regions.OrderByDescending(r => r.Clicks).Take(5);
            sb.AppendLine("=== أفضل 5 مناطق (حسب النقرات) ===");
            sb.AppendLine("الترتيب,المنطقة,النقرات");
            var rank = 1;
            foreach (var region in topRegions)
            {
                sb.AppendLine($"{rank},{region.Country} - {region.City},{region.Clicks:N0}");
                rank++;
            }
            sb.AppendLine();
        }

        // Campaign Insights
        sb.AppendLine("=== رؤى الحملة ===");
        sb.AppendLine($"عدد الإعلانات النشطة,{report.Ads?.Count(a => a.Status == AdStatus.Approved) ?? 0}");
        sb.AppendLine($"متوسط تكلفة النقرة,${cpc:F2}");
        sb.AppendLine($"متوسط معدل النقر,{report.CTR:F2}%");
        if (report.Ads?.Any() == true)
        {
            sb.AppendLine($"متوسط الإنفاق لكل إعلان,${report.TotalSpend / report.Ads.Count():F2}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"AdvertiserReport_{user.UserName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

     //---------------- PDF EXPORT ----------------
    [HttpGet("ExportPdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        var report = await _reportService.GetAdvertiserReportAsync(user.Id, filter);
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4, 40, 40, 50, 50);
        var writer = PdfWriter.GetInstance(doc, ms);
        doc.Open();

        // Setup fonts
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(255, 86, 48)); // Advertiser red/orange
        var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(32, 101, 209));
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.DARK_GRAY);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);

        // Title
        var title = new Paragraph("ADVERTISER CAMPAIGN REPORT", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 10
        };
        doc.Add(title);

        // Advertiser Info
        var advertiserInfo = new Paragraph($"Advertiser: {user.UserName}",
            FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(255, 86, 48)))
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 20
        };
        doc.Add(advertiserInfo);

        // Report Info Table
        var infoTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 20 };
        infoTable.SetWidths(new float[] { 1, 2 });

        AddInfoCell(infoTable, "Generated:", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), normalFont, boldFont);
        AddInfoCell(infoTable, "Campaign Period:",
            $"{filter?.StartDate?.ToString("yyyy-MM-dd") ?? "N/A"} to {filter?.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}",
            normalFont, boldFont);
        AddInfoCell(infoTable, "Report ID:", $"ADV-{DateTime.Now:yyyyMMddHHmmss}", normalFont, boldFont);

        doc.Add(infoTable);

        // Campaign Summary - Highlighted Section
        doc.Add(new Paragraph("CAMPAIGN SUMMARY", headerFont) { SpacingAfter = 10 });

        var summaryTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
        summaryTable.DefaultCell.Padding = 10;
        summaryTable.DefaultCell.BackgroundColor = new BaseColor(255, 247, 245); // Light red/orange
        summaryTable.DefaultCell.Border = Rectangle.NO_BORDER;

        var cpc = report.TotalClicks > 0 ? (report.TotalSpend / report.TotalClicks) : 0;
        var cpm = report.TotalImpressions > 0 ? (report.TotalSpend / report.TotalImpressions * 1000) : 0;

        AddSummaryCard(summaryTable, "Total Spend", $"${report.TotalSpend:N2}", new BaseColor(255, 86, 48));
        AddSummaryCard(summaryTable, "Total Clicks", report.TotalClicks.ToString("N0"), new BaseColor(0, 171, 85));
        AddSummaryCard(summaryTable, "Campaign CTR", $"{report.CTR:F2}%", new BaseColor(32, 101, 209));
        AddSummaryCard(summaryTable, "Avg CPC", $"${cpc:F2}", new BaseColor(255, 171, 0));

        doc.Add(summaryTable);

        // Cost Metrics
        doc.Add(new Paragraph("COST ANALYSIS", subHeaderFont) { SpacingAfter = 10 });

        var costTable = new PdfPTable(3) { WidthPercentage = 100, SpacingAfter = 20 };
        costTable.DefaultCell.Padding = 8;

        AddMetricRow(costTable, "Cost Per Click (CPC)", $"${cpc:F2}", normalFont, boldFont);
        AddMetricRow(costTable, "Cost Per Mille (CPM)", $"${cpm:F2}", normalFont, boldFont);
        AddMetricRow(costTable, "Total Impressions", report.TotalImpressions.ToString("N0"), normalFont, boldFont);
        if (report.Ads?.Any() == true)
        {
            AddMetricRow(costTable, "Average Spend per Ad", $"${report.TotalSpend / report.Ads.Count():F2}", normalFont, boldFont);
        }

        doc.Add(costTable);

        // Ad Performance Details
        if (report.Ads?.Any() == true)
        {
            doc.Add(new Paragraph("AD PERFORMANCE BREAKDOWN", headerFont) { SpacingAfter = 10 });

            var adTable = new PdfPTable(7) { WidthPercentage = 100, SpacingAfter = 20 };
            adTable.SetWidths(new float[] { 3, 2, 2, 2, 2, 2, 2 });

            AddTableHeader(adTable, new[] { "Ad Name", "Status", "Impressions", "Clicks", "CTR", "Spend", "CPC" }, boldFont);

            foreach (var ad in report.Ads.OrderByDescending(a => a.Spend))
            {
                var adCtr = ad.Impressions > 0 ? (ad.Clicks * 100.0 / ad.Impressions) : 0;
                var adCpc = ad.Clicks > 0 ? (ad.Spend / ad.Clicks) : 0;

                // Color code status
                var statusColor = ad.Status == AdStatus.Approved ? new BaseColor(0, 171, 85) :
                                ad.Status == AdStatus.Pending ? new BaseColor(255, 171, 0) :
                                new BaseColor(145, 158, 171);

                AddTableRowWithStatus(adTable, new[]
                {
                ad.Name,
                ad.Status.ToString(),
                ad.Impressions.ToString("N0"),
                ad.Clicks.ToString("N0"),
                $"{adCtr:F2}%",
                $"${ad.Spend:F2}",
                $"${adCpc:F2}"
            }, normalFont, statusColor);
            }

            doc.Add(adTable);
        }

        // Top Performing Ads
        if (report.Ads?.Any() == true && report.Ads.Count() >= 3)
        {
            doc.Add(new Paragraph("TOP PERFORMING ADS", subHeaderFont) { SpacingAfter = 10 });

            var topTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
            topTable.SetWidths(new float[] { 1, 3, 2, 2 });

            AddTableHeader(topTable, new[] { "Rank", "Ad Name", "Clicks", "CTR" }, boldFont);

            int rank = 1;
            foreach (var ad in report.Ads.OrderByDescending(a => a.Clicks).Take(5))
            {
                var adCtr = ad.Impressions > 0 ? (ad.Clicks * 100.0 / ad.Impressions) : 0;

                var rankCell = new PdfPCell(new Phrase($"#{rank}", boldFont))
                {
                    Padding = 8,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BackgroundColor = rank <= 3 ? new BaseColor(255, 243, 205) : BaseColor.WHITE,
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = new BaseColor(240, 242, 245)
                };
                topTable.AddCell(rankCell);

                AddTableCell(topTable, ad.Name, normalFont);
                AddTableCell(topTable, ad.Clicks.ToString("N0"), boldFont);
                AddTableCell(topTable, $"{adCtr:F2}%", normalFont);

                rank++;
            }

            doc.Add(topTable);
        }

        // Regional Performance
        if (report.Regions?.Any() == true)
        {
            doc.Add(new Paragraph("GEOGRAPHIC PERFORMANCE", headerFont) { SpacingAfter = 10 });

            var regionTable = new PdfPTable(5) { WidthPercentage = 100, SpacingAfter = 20 };
            regionTable.SetWidths(new float[] { 2, 2, 2, 2, 2 });

            AddTableHeader(regionTable, new[] { "Country", "City", "Impressions", "Clicks", "CTR" }, boldFont);

            foreach (var region in report.Regions.OrderByDescending(r => r.Clicks))
            {
                var regionCtr = region.Impressions > 0 ? (region.Clicks * 100.0 / region.Impressions) : 0;

                AddTableRow(regionTable, new[]
                {
                region.Country,
                region.City,
                region.Impressions.ToString("N0"),
                region.Clicks.ToString("N0"),
                $"{regionCtr:F2}%"
            }, normalFont);
            }

            doc.Add(regionTable);
        }

        // Campaign Insights & Recommendations
        doc.Add(new Paragraph("CAMPAIGN INSIGHTS", subHeaderFont) { SpacingAfter = 10 });

        var insightsTable = new PdfPTable(1) { WidthPercentage = 100, SpacingAfter = 10 };
        insightsTable.DefaultCell.Padding = 10;
        insightsTable.DefaultCell.BackgroundColor = new BaseColor(249, 250, 251);
        insightsTable.DefaultCell.Border = Rectangle.NO_BORDER;

        var insights = new List<string>();

        if (report.CTR < 1.0m)
        {
            insights.Add("• Your CTR is below industry average (1-2%). Consider improving ad creatives or targeting.");
        }
        else if (report.CTR >= 2.0m)
        {
            insights.Add("• Excellent CTR! Your ads are highly engaging with your target audience.");
        }

        if (cpc > 2.0m)
        {
            insights.Add("• Your CPC is high. Consider optimizing targeting or ad quality to reduce costs.");
        }
        else if (cpc <= 0.5m)
        {
            insights.Add("• Great CPC! You're getting excellent value for your clicks.");
        }

        if (report.Ads?.Any() == true)
        {
            var approvedAds = report.Ads.Count(a => a.Status == AdStatus.Approved);
            var totalAds = report.Ads.Count();

            if (approvedAds < totalAds)
            {
                insights.Add($"• You have {totalAds - approvedAds} non-approved ads. Get them approved to maximize reach.");
            }

            var lowPerformers = report.Ads.Count(a => a.Impressions > 100 && (a.Clicks * 100.0 / a.Impressions) < 0.5);
            if (lowPerformers > 0)
            {
                insights.Add($"• {lowPerformers} ad(s) have low CTR. Consider pausing or updating their creatives.");
            }
        }

        if (report.Regions?.Any() == true && report.Regions.Count() >= 3)
        {
            var topRegion = report.Regions.OrderByDescending(r => r.Clicks).First();
            insights.Add($"• {topRegion.Country} - {topRegion.City} is your best performing region. Consider allocating more budget here.");
        }

        if (!insights.Any())
        {
            insights.Add("• Your campaign is performing well. Continue monitoring and optimizing for best results.");
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
        var footer = new Paragraph("Generated by Ad Management Platform - Advertiser Dashboard",
            FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY))
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(footer);

        doc.Close();

        var fileName = $"AdvertiserReport_{user.UserName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(ms.ToArray(), "application/pdf", fileName);
    }
    private void AddTableRowWithStatus(PdfPTable table, string[] values, Font font, BaseColor statusColor)
    {
        for (int i = 0; i < values.Length; i++)
        {
            var cellFont = (i == 1) ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, statusColor) : font;

            var cell = new PdfPCell(new Phrase(values[i], cellFont))
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

    // Keep the existing helper methods from previous implementation
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
            BackgroundColor = new BaseColor(255, 247, 245),
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
                BackgroundColor = new BaseColor(255, 86, 48), // Advertiser orange/red
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
    public IActionResult Invoice()
    {
        var pdfBytes = _pdf.GenerateArabicPdf(
            "فاتورة رقم 1023",
            "هذا المستند يحتوي على تفاصيل الفاتورة الخاصة بالعميل..."
        );

        return File(pdfBytes, "application/pdf", "Invoice.pdf");
    }
}
