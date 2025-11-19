//using AdManagementSystem.Models.Enums;
//using AdManagementSystem.ViewModels;
//using AdSystem.Data;
//using AdSystem.Models;
//using Microsoft.EntityFrameworkCore;

//namespace AdManagementSystem.Services
//{
//    // Services/Implementations/ReportService.cs
//    public class ReportService : IReportService
//    {
//        private readonly AppDbContext _context;

//        public ReportService(AppDbContext context)
//        {
//            _context = context;
//        }

//        // ------------------------- ADMIN REPORT -------------------------
//        public async Task<AdminReportViewModel> GetAdminReportAsync(ReportFilterViewModel filter)
//        {
//            var impressions = ApplyImpressionFilter(_context.AdImpressions.AsQueryable(), filter);
//            var clicks = ApplyClickFilter(_context.AdClicks.AsQueryable(), filter);

//            var vm = new AdminReportViewModel
//            {
//                TotalImpressions = await impressions.LongCountAsync(),
//                TotalClicks = await clicks.LongCountAsync(),
//                TotalAdvertiserSpend = await _context.WalletTransactions
//                    .Where(t => t.Type == TransactionType.AdSpend)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0,
//                TotalPublisherEarnings = await _context.WalletTransactions
//                    .Where(t => t.Type == TransactionType.PublisherEarning)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0
//            };

//            vm.Regions = await impressions
//                .GroupBy(i => new { i.Country, i.City })
//                .Select(g => new RegionReportItem
//                {
//                    Country = g.Key.Country ?? "Unknown",
//                    City = g.Key.City ?? "Unknown",
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Country == g.Key.Country && c.City == g.Key.City)
//                }).ToListAsync();

//            vm.TopAds = await impressions
//                .GroupBy(i => new { i.AdId, i.Ad.Title })
//                .Select(g => new EntityPerformanceItem
//                {
//                    Id = g.Key.AdId,
//                    Name = g.Key.Title,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.AdId == g.Key.AdId)
//                })
//                .OrderByDescending(x => x.Clicks)
//                .Take(10)
//                .ToListAsync();

//            vm.TopPublishers = await impressions
//                .GroupBy(i => new { i.Website.OwnerId, i.Website.Domain })
//                .Select(g => new EntityPerformanceItem
//                {
//                    Id = g.Key.OwnerId.GetHashCode(),
//                    Name = g.Key.Domain,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Website.OwnerId == g.Key.OwnerId)
//                })
//                .OrderByDescending(x => x.Impressions)
//                .Take(10)
//                .ToListAsync();

//            return vm;
//        }

//        // ----------------------- ADVERTISER REPORT ----------------------
//        public async Task<AdvertiserReportViewModel> GetAdvertiserReportAsync(string advertiserId, ReportFilterViewModel filter)
//        {
//            var impressions = ApplyImpressionFilter(
//                _context.AdImpressions.Where(i => i.Ad.AdvertiserId == advertiserId), filter);

//            var clicks = ApplyClickFilter(
//                _context.AdClicks.Where(c => c.Ad.AdvertiserId == advertiserId), filter);

//            var vm = new AdvertiserReportViewModel
//            {
//                TotalImpressions = await impressions.LongCountAsync(),
//                TotalClicks = await clicks.LongCountAsync(),
//                TotalSpend = await _context.WalletTransactions
//                    .Where(t => t.ToUserId == advertiserId && t.Type == TransactionType.AdSpend)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0
//            };

//            vm.Ads = await impressions
//                .GroupBy(i => new { i.AdId, i.Ad.Title })
//                .Select(g => new AdPerformanceItem
//                {
//                    Id = g.Key.AdId,
//                    Name = g.Key.Title,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.AdId == g.Key.AdId),
//                    Spend = _context.WalletTransactions
//                        .Where(t => t.AdId == g.Key.AdId && t.UserId == advertiserId)
//                        .Sum(t => (decimal?)t.Amount) ?? 0
//                })
//                .OrderByDescending(a => a.Clicks)
//                .ToListAsync();

//            vm.Regions = await impressions
//                .GroupBy(i => new { i.Country, i.City })
//                .Select(g => new RegionReportItem
//                {
//                    Country = g.Key.Country ?? "Unknown",
//                    City = g.Key.City ?? "Unknown",
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Country == g.Key.Country && c.City == g.Key.City)
//                }).ToListAsync();

//            return vm;
//        }

//        // ----------------------- PUBLISHER REPORT -----------------------
//        public async Task<PublisherReportViewModel> GetPublisherReportAsync(string publisherId, ReportFilterViewModel filter)
//        {
//            var impressions = ApplyImpressionFilter(
//                _context.AdImpressions.Where(i => i.Website.OwnerId == publisherId), filter);

//            var clicks = ApplyClickFilter(
//                _context.AdClicks.Where(c => c.Website.OwnerId == publisherId), filter);

//            var vm = new PublisherReportViewModel
//            {
//                TotalImpressions = await impressions.LongCountAsync(),
//                TotalClicks = await clicks.LongCountAsync(),
//                TotalEarnings = await _context.WalletTransactions
//                    .Where(t => t.UserId == publisherId && t.Type == TransactionType.PublisherEarning)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0
//            };

//            vm.Websites = await impressions
//                .GroupBy(i => new { i.WebsiteId, i.Website.Domain })
//                .Select(g => new WebsitePerformanceItem
//                {
//                    Id = g.Key.WebsiteId,
//                    Name = g.Key.Domain,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.WebsiteId == g.Key.WebsiteId),
//                    Earnings = _context.WalletTransactions
//                        .Where(t => t.WebsiteId == g.Key.WebsiteId && t.UserId == publisherId)
//                        .Sum(t => (decimal?)t.Amount) ?? 0
//                })
//                .OrderByDescending(w => w.Impressions)
//                .ToListAsync();

//            vm.Regions = await impressions
//                .GroupBy(i => new { i.Country, i.City })
//                .Select(g => new RegionReportItem
//                {
//                    Country = g.Key.Country ?? "Unknown",
//                    City = g.Key.City ?? "Unknown",
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Country == g.Key.Country && c.City == g.Key.City)
//                }).ToListAsync();

//            return vm;
//        }

//        // ----------------------- FILTER HELPERS -------------------------
//        private IQueryable<AdImpression> ApplyImpressionFilter(IQueryable<AdImpression> query, ReportFilterViewModel filter)
//        {
//            if (filter.StartDate.HasValue)
//                query = query.Where(i => i.Timestamp >= filter.StartDate);
//            if (filter.EndDate.HasValue)
//                query = query.Where(i => i.Timestamp <= filter.EndDate);
//            if (!string.IsNullOrEmpty(filter.Country))
//                query = query.Where(i => i.Country == filter.Country);
//            if (!string.IsNullOrEmpty(filter.City))
//                query = query.Where(i => i.City == filter.City);
//            if (filter.AdId.HasValue)
//                query = query.Where(i => i.AdId == filter.AdId);
//            if (filter.WebsiteId.HasValue)
//                query = query.Where(i => i.WebsiteId == filter.WebsiteId);

//            return query;
//        }

//        private IQueryable<AdClick> ApplyClickFilter(IQueryable<AdClick> query, ReportFilterViewModel filter)
//        {
//            if (filter.StartDate.HasValue)
//                query = query.Where(c => c.Timestamp >= filter.StartDate);
//            if (filter.EndDate.HasValue)
//                query = query.Where(c => c.Timestamp <= filter.EndDate);
//            if (!string.IsNullOrEmpty(filter.Country))
//                query = query.Where(c => c.Country == filter.Country);
//            if (!string.IsNullOrEmpty(filter.City))
//                query = query.Where(c => c.City == filter.City);
//            if (filter.AdId.HasValue)
//                query = query.Where(c => c.AdId == filter.AdId);
//            if (filter.WebsiteId.HasValue)
//                query = query.Where(c => c.WebsiteId == filter.WebsiteId);

//            return query;
//        }
//    }

//}
//using AdManagementSystem.Models.Enums;
//using AdManagementSystem.ViewModels;
//using AdSystem.Data;
//using AdSystem.Models;
//using Microsoft.EntityFrameworkCore;

//namespace AdManagementSystem.Services
//{
//    public class ReportService : IReportService
//    {
//        private readonly AppDbContext _context;

//        public ReportService(AppDbContext context)
//        {
//            _context = context;
//        }

//        // ------------------------- ADMIN REPORT -------------------------
//        public async Task<AdminReportViewModel> GetAdminReportAsync(ReportFilterViewModel filter)
//        {
//            var impressions = ApplyImpressionFilter(_context.AdImpressions.Include(i => i.Ad).Include(i => i.Website), filter);
//            var clicks = ApplyClickFilter(_context.AdClicks.Include(c => c.Ad).Include(c => c.Website), filter);

//            var vm = new AdminReportViewModel
//            {
//                TotalImpressions = await impressions.LongCountAsync(),
//                TotalClicks = await clicks.LongCountAsync(),
//                TotalAdvertiserSpend = await _context.WalletTransactions
//                    .Where(t => t.Type == TransactionType.AdPayment)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0,
//                TotalPublisherEarnings = await _context.WalletTransactions
//                    .Where(t => t.Type == TransactionType.PublisherPayment)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0
//            };

//            // Regions
//            vm.Regions = await impressions
//                .GroupBy(i => new { Country = i.Website.Country, City = i.Website.City })
//                .Select(g => new RegionReportItem
//                {
//                    Country = g.Key.Country ?? "Unknown",
//                    City = g.Key.City ?? "Unknown",
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Website.Country == g.Key.Country && c.Website.City == g.Key.City)
//                }).ToListAsync();

//            // Top Ads
//            vm.TopAds = await impressions
//                .GroupBy(i => new { i.AdId, i.Ad.Title })
//                .Select(g => new EntityPerformanceItem
//                {
//                    Id = g.Key.AdId,
//                    Name = g.Key.Title,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.AdId == g.Key.AdId)
//                })
//                .OrderByDescending(x => x.Clicks)
//                .Take(10)
//                .ToListAsync();

//            // Top Publishers
//            vm.TopPublishers = await impressions
//                .GroupBy(i => new { PublisherId = i.Website.Publisher.Id, i.Website.Domain })
//                .Select(g => new EntityPerformanceItem
//                {
//                    Id = g.Key.PublisherId.GetHashCode(),
//                    Name = g.Key.Domain,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Website.Publisher.Id == g.Key.PublisherId)
//                })
//                .OrderByDescending(x => x.Impressions)
//                .Take(10)
//                .ToListAsync();

//            return vm;
//        }

//        // ----------------------- ADVERTISER REPORT ----------------------
//        public async Task<AdvertiserReportViewModel> GetAdvertiserReportAsync(string advertiserId, ReportFilterViewModel filter)
//        {
//            var impressions = ApplyImpressionFilter(
//                _context.AdImpressions.Include(i => i.Ad).Include(i => i.Website)
//                .Where(i => i.Ad.AdvertiserId == advertiserId), filter);

//            var clicks = ApplyClickFilter(
//                _context.AdClicks.Include(c => c.Ad).Include(c => c.Website)
//                .Where(c => c.Ad.AdvertiserId == advertiserId), filter);

//            var vm = new AdvertiserReportViewModel
//            {
//                TotalImpressions = await impressions.LongCountAsync(),
//                TotalClicks = await clicks.LongCountAsync(),
//                TotalSpend = await _context.WalletTransactions
//                    .Where(t => t.ToUserId == advertiserId && t.Type == TransactionType.AdPayment)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0
//            };

//            // Ads
//            vm.Ads = await impressions
//                .GroupBy(i => new { i.AdId, i.Ad.Title })
//                .Select(g => new AdPerformanceItem
//                {
//                    Id = g.Key.AdId,
//                    Name = g.Key.Title,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.AdId == g.Key.AdId),
//                    Spend = _context.WalletTransactions
//                        .Where(t => t.AdId == g.Key.AdId && t.ToUserId == advertiserId)
//                        .Sum(t => (decimal?)t.Amount) ?? 0
//                })
//                .OrderByDescending(a => a.Clicks)
//                .ToListAsync();

//            // Regions
//            vm.Regions = await impressions
//                .GroupBy(i => new { Country = i.Website.Country, City = i.Website.City })
//                .Select(g => new RegionReportItem
//                {
//                    Country = g.Key.Country ?? "Unknown",
//                    City = g.Key.City ?? "Unknown",
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Website.Country == g.Key.Country && c.Website.City == g.Key.City)
//                }).ToListAsync();

//            return vm;
//        }

//        // ----------------------- PUBLISHER REPORT -----------------------
//        public async Task<PublisherReportViewModel> GetPublisherReportAsync(string publisherId, ReportFilterViewModel filter)
//        {
//            var impressions = ApplyImpressionFilter(
//                _context.AdImpressions.Include(i => i.Ad).Include(i => i.Website)
//                .Where(i => i.Website.Publisher.Id == publisherId), filter);

//            var clicks = ApplyClickFilter(
//                _context.AdClicks.Include(c => c.Ad).Include(c => c.Website)
//                .Where(c => c.Website.Publisher.Id == publisherId), filter);

//            var vm = new PublisherReportViewModel
//            {
//                TotalImpressions = await impressions.LongCountAsync(),
//                TotalClicks = await clicks.LongCountAsync(),
//                TotalEarnings = await _context.WalletTransactions
//                    .Where(t => t.Website.Publisher.Id == publisherId && t.Type == TransactionType.PublisherPayment)
//                    .SumAsync(t => (decimal?)t.Amount) ?? 0
//            };

//            // Websites
//            vm.Websites = await impressions
//                .GroupBy(i => new { i.WebsiteId, i.Website.Domain })
//                .Select(g => new WebsitePerformanceItem
//                {
//                    Id = g.Key.WebsiteId,
//                    Name = g.Key.Domain,
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.WebsiteId == g.Key.WebsiteId),
//                    Earnings = _context.WalletTransactions
//                        .Where(t => t.WebsiteId == g.Key.WebsiteId && t.Website.Publisher.Id == publisherId)
//                        .Sum(t => (decimal?)t.Amount) ?? 0
//                })
//                .OrderByDescending(w => w.Impressions)
//                .ToListAsync();

//            // Regions
//            vm.Regions = await impressions
//                .GroupBy(i => new { Country = i.Website.Country, City = i.Website.City })
//                .Select(g => new RegionReportItem
//                {
//                    Country = g.Key.Country ?? "Unknown",
//                    City = g.Key.City ?? "Unknown",
//                    Impressions = g.LongCount(),
//                    Clicks = clicks.Count(c => c.Website.Country == g.Key.Country && c.Website.City == g.Key.City)
//                }).ToListAsync();

//            return vm;
//        }

//        // ----------------------- FILTER HELPERS -------------------------
//        private IQueryable<AdImpression> ApplyImpressionFilter(IQueryable<AdImpression> query, ReportFilterViewModel filter)
//        {
//            if (filter.StartDate.HasValue)
//                query = query.Where(i => i.CreatedAt >= filter.StartDate);
//            if (filter.EndDate.HasValue)
//                query = query.Where(i => i.CreatedAt <= filter.EndDate);
//            if (!string.IsNullOrEmpty(filter.Country))
//                query = query.Where(i => i.Website.Country == filter.Country);
//            if (!string.IsNullOrEmpty(filter.City))
//                query = query.Where(i => i.Website.City == filter.City);
//            if (filter.AdId.HasValue)
//                query = query.Where(i => i.AdId == filter.AdId);
//            if (filter.WebsiteId.HasValue)
//                query = query.Where(i => i.WebsiteId == filter.WebsiteId);

//            return query;
//        }

//        private IQueryable<AdClick> ApplyClickFilter(IQueryable<AdClick> query, ReportFilterViewModel filter)
//        {
//            if (filter.StartDate.HasValue)
//                query = query.Where(c => c.CreatedAt >= filter.StartDate);
//            if (filter.EndDate.HasValue)
//                query = query.Where(c => c.CreatedAt <= filter.EndDate);
//            if (!string.IsNullOrEmpty(filter.Country))
//                query = query.Where(c => c.Website.Country == filter.Country);
//            if (!string.IsNullOrEmpty(filter.City))
//                query = query.Where(c => c.Website.City == filter.City);
//            if (filter.AdId.HasValue)
//                query = query.Where(c => c.AdId == filter.AdId);
//            if (filter.WebsiteId.HasValue)
//                query = query.Where(c => c.WebsiteId == filter.WebsiteId);

//            return query;
//        }
//    }
//}
using AdManagementSystem.Models.Enums;
using AdManagementSystem.ViewModels;
using AdSystem.Data;
using AdSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AdManagementSystem.Services
{
    /// <summary>
    /// Generates reporting and analytics data for Admin, Advertiser, and Publisher roles.
    /// Includes region-based summaries and supports filters by date, country, city, ad, or website.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------- ADMIN REPORT -------------------------
        public async Task<AdminReportViewModel> GetAdminReportAsync(ReportFilterViewModel filter)
        {
            var impressions = ApplyImpressionFilter(_context.AdImpressions.AsQueryable(), filter);
            var clicks = ApplyClickFilter(_context.AdClicks.AsQueryable(), filter);

            var vm = new AdminReportViewModel
            {
                TotalImpressions = await impressions.LongCountAsync(),
                TotalClicks = await clicks.LongCountAsync(),
                TotalAdvertiserSpend = await _context.WalletTransactions
                    .Where(t => t.Type == TransactionType.Click || t.Type == TransactionType.Impression)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0,
                TotalPublisherEarnings = await _context.WalletTransactions
                    .Where(t => t.Type == TransactionType.Click || t.Type == TransactionType.Impression)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0
            };
            Console.WriteLine(vm.TotalAdvertiserSpend);
            Console.WriteLine(vm.TotalPublisherEarnings);

            // Region stats
            vm.Regions = await impressions
                .GroupBy(i => new { i.Country, i.City })
                .Select(g => new RegionReportItem
                {
                    Country = g.Key.Country ?? "Unknown",
                    City = g.Key.City ?? "Unknown",
                    Impressions = g.LongCount(),
                    Clicks = clicks.Count(c => c.Country == g.Key.Country && c.City == g.Key.City)
                }).ToListAsync();

            // Top ads
            vm.TopAds = await impressions
                .GroupBy(i => new { i.AdId, i.Ad.Title })
                .Select(g => new EntityPerformanceItem
                {
                    Id = g.Key.AdId,
                    Name = g.Key.Title,
                    Impressions = g.LongCount(),
                    Clicks = clicks.Count(c => c.AdId == g.Key.AdId)
                })
                .OrderByDescending(x => x.Clicks)
                .Take(10)
                .ToListAsync();

            // Top publishers
            vm.TopPublishers = await impressions
                .GroupBy(i => new { i.Website.OwnerId, i.Website.Domain })
                .Select(g => new EntityPerformanceItem
                {
                    Id = g.Key.OwnerId.GetHashCode(),
                    Name = g.Key.Domain,
                    Impressions = g.LongCount(),
                    Clicks = clicks.Count(c => c.Website.OwnerId == g.Key.OwnerId)
                })
                .OrderByDescending(x => x.Impressions)
                .Take(10)
                .ToListAsync();

            return vm;
        }

        // ----------------------- ADVERTISER REPORT ----------------------
        public async Task<AdvertiserReportViewModel> GetAdvertiserReportAsync(string advertiserId, ReportFilterViewModel filter)
        {
            var impressions = ApplyImpressionFilter(
                _context.AdImpressions.Where(i => i.Ad.AdvertiserId == advertiserId), filter);

            var clicks = ApplyClickFilter(
                _context.AdClicks.Where(c => c.Ad.AdvertiserId == advertiserId), filter);

            var vm = new AdvertiserReportViewModel
            {
                TotalImpressions = await impressions.LongCountAsync(),
                TotalClicks = await clicks.LongCountAsync(),
                TotalSpend = await _context.WalletTransactions
                    .Where(t => t.FromUserId == advertiserId &&
                                (t.Type == TransactionType.Click || t.Type == TransactionType.Impression))
                    .SumAsync(t => (decimal?)t.Amount) ?? 0
            };

            // Ad performance
            vm.Ads = await impressions
                .GroupBy(i => new
                {
                    i.AdId,
                    i.Ad.Title,
                    i.Ad.Status
                })
                .Select(g => new AdPerformanceItem
                {
                    Id = g.Key.AdId,
                    Name = g.Key.Title,
                    Impressions = g.LongCount(),
                    Status= g.Key.Status,
                    Clicks = clicks.Count(c => c.AdId == g.Key.AdId),
                    Spend = _context.WalletTransactions
                        .Where(t => t.AdId == g.Key.AdId && t.FromUserId == advertiserId)
                        .Sum(t => (decimal?)t.Amount) ?? 0
                })
                .OrderByDescending(a => a.Clicks)
                .ToListAsync();

            // Region stats
            vm.Regions = await impressions
                .GroupBy(i => new { i.Country, i.City })
                .Select(g => new RegionReportItem
                {
                    Country = g.Key.Country ?? "Unknown",
                    City = g.Key.City ?? "Unknown",
                    Impressions = g.LongCount(),
                    Clicks = clicks.Count(c => c.Country == g.Key.Country && c.City == g.Key.City)
                }).ToListAsync();

            return vm;
        }

        // ----------------------- PUBLISHER REPORT -----------------------
        public async Task<PublisherReportViewModel> GetPublisherReportAsync(string publisherId, ReportFilterViewModel filter)
        {
            var impressions = ApplyImpressionFilter(
                _context.AdImpressions.Where(i => i.Website.OwnerId == publisherId), filter);

            var clicks = ApplyClickFilter(
                _context.AdClicks.Where(c => c.Website.OwnerId == publisherId), filter);

            var vm = new PublisherReportViewModel
            {
                TotalImpressions = await impressions.LongCountAsync(),
                TotalClicks = await clicks.LongCountAsync(),
                TotalEarnings = await _context.WalletTransactions
                    .Where(t => t.ToUserId == publisherId &&
                                (t.Type == TransactionType.Click || t.Type == TransactionType.Impression))
                    .SumAsync(t => (decimal?)t.Amount) ?? 0
            };

            // Website performance
            vm.Websites = await impressions
                .GroupBy(i => new { i.WebsiteId, i.Website.Domain })
                .Select(g => new WebsitePerformanceItem
                {
                    Id = g.Key.WebsiteId,
                    Name = g.Key.Domain,
                    Impressions = g.LongCount(),
                    Clicks = clicks.Count(c => c.WebsiteId == g.Key.WebsiteId),
                    Earnings = _context.WalletTransactions
                        .Where(t => t.WebsiteId == g.Key.WebsiteId && t.ToUserId == publisherId)
                        .Sum(t => (decimal?)t.Amount) ?? 0
                })
                .OrderByDescending(w => w.Impressions)
                .ToListAsync();

            // Region stats
            vm.Regions = await impressions
                .GroupBy(i => new { i.Country, i.City })
                .Select(g => new RegionReportItem
                {
                    Country = g.Key.Country ?? "Unknown",
                    City = g.Key.City ?? "Unknown",
                    Impressions = g.LongCount(),
                    Clicks = clicks.Count(c => c.Country == g.Key.Country && c.City == g.Key.City)
                }).ToListAsync();

            return vm;
        }

        // ----------------------- FILTER HELPERS -------------------------
        private IQueryable<AdImpression> ApplyImpressionFilter(IQueryable<AdImpression> query, ReportFilterViewModel filter)
        {
            if (filter.StartDate.HasValue)
                query = query.Where(i => i.ViewedAt >= filter.StartDate);
            if (filter.EndDate.HasValue)
                query = query.Where(i => i.ViewedAt <= filter.EndDate);
            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(i => i.Country == filter.Country);
            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(i => i.City == filter.City);
            if (filter.AdId.HasValue)
                query = query.Where(i => i.AdId == filter.AdId);
            if (filter.WebsiteId.HasValue)
                query = query.Where(i => i.WebsiteId == filter.WebsiteId);

            return query;
        }

        private IQueryable<AdClick> ApplyClickFilter(IQueryable<AdClick> query, ReportFilterViewModel filter)
        {
            if (filter.StartDate.HasValue)
                query = query.Where(c => c.ClickedAt >= filter.StartDate);
            if (filter.EndDate.HasValue)
                query = query.Where(c => c.ClickedAt <= filter.EndDate);
            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(c => c.Country == filter.Country);
            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(c => c.City == filter.City);
            if (filter.AdId.HasValue)
                query = query.Where(c => c.AdId == filter.AdId);
            if (filter.WebsiteId.HasValue)
                query = query.Where(c => c.WebsiteId == filter.WebsiteId);

            return query;
        }
    }
}
