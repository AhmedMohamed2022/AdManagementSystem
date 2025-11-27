using AdManagementSystem.Models;
using AdManagementSystem.Models.Enums;
using AdSystem.Data;
using AdSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AdSystem.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly AppDbContext _db;
        private readonly IAdPricingService _pricingService;
        private readonly IGeoLocationService _geoService;

        public FinanceService(AppDbContext db, IAdPricingService pricingService, IGeoLocationService geoService)
        {
            _db = db;
            _pricingService = pricingService;
            _geoService = geoService;
        }
        public async Task HandleImpressionAsync(AdImpression impression)
        {
            if (impression == null) return;

            // 🔹 Ensure we have country & city from GeoService if missing
            if (string.IsNullOrEmpty(impression.Country) || string.IsNullOrEmpty(impression.City))
            {
                try
                {
                    var ip = impression.IPAddress;
                    if (!string.IsNullOrEmpty(ip))
                    {
                        var (country, city) = await _geoService.ResolveIpAsync(ip);
                        impression.Country = country ?? "Unknown";
                        impression.City = city ?? "Unknown";

                        // If impression already tracked (Id != 0), update its geo fields
                        if (impression.Id != 0)
                            _db.AdImpressions.Update(impression);
                    }
                    else
                    {
                        impression.Country ??= "Unknown";
                        impression.City ??= "Unknown";
                    }
                }
                catch
                {
                    impression.Country ??= "Unknown";
                    impression.City ??= "Unknown";
                }
            }

            // 🔹 Load Ad and Website
            var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Id == impression.AdId);
            if (ad == null) return;

            if ((ad.MaxImpressions != null && ad.Impressions.Count >= ad.MaxImpressions) ||
                (ad.MaxClicks != null && ad.Clicks.Count >= ad.MaxClicks))
                ad.Status = AdStatus.Paused;

            var website = await _db.Websites.FirstOrDefaultAsync(w => w.Id == impression.WebsiteId);
            var publisherUserId = website?.OwnerId;

            // ✅ Dynamic pricing based on advertiser + country + city
            var rule = await _pricingService.GetEffectivePricingAsync(
                ad.AdvertiserId,
                impression.Country,
                impression.City
            );

            var perImpressionAmount = Math.Round(rule.CPM / 1000m, 6);
            impression.EarnedAmount = perImpressionAmount;

            var advertiser = await _db.Users.FirstOrDefaultAsync(u => u.Id == ad.AdvertiserId);
            var publisher = publisherUserId == null
                ? null
                : await _db.Users.FirstOrDefaultAsync(u => u.Id == publisherUserId);

            if (advertiser == null)
            {
                await _db.SaveChangesAsync();
                return;
            }

            // 🔹 Not enough balance = pause ad
            if (advertiser.Balance < perImpressionAmount)
            {
                ad.Status = AdStatus.Paused;

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    FromUserId = ad.AdvertiserId,
                    ToUserId = null,
                    Amount = 0,
                    Type = TransactionType.Adjustment,
                    AdId = ad.Id,
                    WebsiteId = impression.WebsiteId,
                    Description = $"Ad paused. Advertiser lacks balance for impression charge {perImpressionAmount}"
                });

                //await _db.SaveChangesAsync();
                return;
            }

            // 🔹 Deduct advertiser, credit publisher
            advertiser.Balance -= perImpressionAmount;
            if (publisher != null) publisher.Balance += perImpressionAmount;

            // 🔹 Log transaction
            _db.WalletTransactions.Add(new WalletTransaction
            {
                FromUserId = ad.AdvertiserId,
                ToUserId = publisherUserId,
                Amount = perImpressionAmount,
                AdId = impression.AdId,
                WebsiteId = impression.WebsiteId,
                Type = TransactionType.Impression,
                Description = $"Impression charged ({perImpressionAmount}) [{impression.Country}/{impression.City}]"
            });

            // 🔹 Save impression (now includes geo)
            _db.AdImpressions.Add(impression);

            await _db.SaveChangesAsync();
        }

        public async Task HandleClickAsync(AdClick click)
        {
            if (click == null) return;

            // 🔹 Resolve geo if missing
            if (string.IsNullOrEmpty(click.Country) || string.IsNullOrEmpty(click.City))
            {
                try
                {
                    var ip = click.IPAddress;
                    if (!string.IsNullOrEmpty(ip))
                    {
                        var (country, city) = await _geoService.ResolveIpAsync(ip);
                        click.Country = country ?? "Unknown";
                        click.City = city ?? "Unknown";

                        if (click.Id != 0)
                            _db.AdClicks.Update(click);
                    }
                    else
                    {
                        click.Country ??= "Unknown";
                        click.City ??= "Unknown";
                    }
                }
                catch
                {
                    click.Country ??= "Unknown";
                    click.City ??= "Unknown";
                }
            }

            var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Id == click.AdId);
            if (ad == null) return;

            if ((ad.MaxImpressions != null && ad.Impressions.Count >= ad.MaxImpressions) ||
                (ad.MaxClicks != null && ad.Clicks.Count >= ad.MaxClicks))
                ad.Status = AdStatus.Paused;

            var website = await _db.Websites.FirstOrDefaultAsync(w => w.Id == click.WebsiteId);
            var publisherUserId = website?.OwnerId;

            // ✅ Geo-based pricing lookup
            var rule = await _pricingService.GetEffectivePricingAsync(
                ad.AdvertiserId,
                click.Country,
                click.City
            );

            var perClickAmount = Math.Round(rule.CPC, 6);
            click.EarnedAmount = perClickAmount;

            var advertiser = await _db.Users.FirstOrDefaultAsync(u => u.Id == ad.AdvertiserId);
            var publisher = publisherUserId == null
                ? null
                : await _db.Users.FirstOrDefaultAsync(u => u.Id == publisherUserId);

            if (advertiser == null)
            {
                //await _db.SaveChangesAsync();
                return;
            }

            // 🔹 Not enough balance → pause ad
            if (advertiser.Balance < perClickAmount)
            {
                ad.Status = AdStatus.Paused;

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    FromUserId = ad.AdvertiserId,
                    ToUserId = null,
                    Amount = 0,
                    AdId = ad.Id,
                    WebsiteId = click.WebsiteId,
                    Type = TransactionType.Adjustment,
                    Description = $"Ad paused. Advertiser lacks balance for click charge {perClickAmount}"
                });

                //await _db.SaveChangesAsync();
                return;
            }

            advertiser.Balance -= perClickAmount;
            if (publisher != null) publisher.Balance += perClickAmount;

            _db.WalletTransactions.Add(new WalletTransaction
            {
                FromUserId = ad.AdvertiserId,
                ToUserId = publisherUserId,
                AdId = click.AdId,
                WebsiteId = click.WebsiteId,
                Type = TransactionType.Click,
                Amount = perClickAmount,
                Description = $"Click charged ({perClickAmount}) [{click.Country}/{click.City}]"
            });

            // 🔹 Save click (now includes geo)
            _db.AdClicks.Add(click);

            //await _db.SaveChangesAsync();
        }


        public async Task WalletAdjustAsync(string adminUserId, string userId, decimal amount, string description)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (amount > 0)
            {
                user.Balance += amount;

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    FromUserId = adminUserId,
                    ToUserId = userId,
                    Amount = amount,
                    Type = TransactionType.ManualCredit,
                    Description = description
                });
            }
            else if (amount < 0)
            {
                var debit = Math.Abs(amount);

                if (user.Balance < debit)
                    throw new InvalidOperationException("Insufficient user balance");

                user.Balance -= debit;

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    FromUserId = userId,
                    ToUserId = adminUserId,
                    Amount = debit,
                    Type = TransactionType.ManualDebit,
                    Description = description
                });
            }

            await _db.SaveChangesAsync();
        }
        // ---------- New: Admin manual adjust ----------
        public async Task ManualAdjustAsync(string adminUserId, string userId, decimal amount, string description)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (amount == 0) throw new ArgumentException("Amount cannot be zero", nameof(amount));

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("User not found");

            // Positive amount => credit ToUserId, Negative => debit FromUserId
            if (amount > 0)
            {
                user.Balance += amount;
                _db.WalletTransactions.Add(new WalletTransaction
                {
                    FromUserId = adminUserId,
                    ToUserId = userId,
                    Amount = amount,
                    Type = TransactionType.ManualCredit,
                    Description = description
                });
            }
            else
            {
                var debit = Math.Abs(amount);
                if (user.Balance < debit) throw new InvalidOperationException("Insufficient user balance for debit");
                user.Balance -= debit;
                _db.WalletTransactions.Add(new WalletTransaction
                {
                    FromUserId = userId,
                    ToUserId = adminUserId,
                    Amount = debit,
                    Type = TransactionType.ManualDebit,
                    Description = description
                });
            }

            await _db.SaveChangesAsync();
        }

        // ---------- New: Deposit helper for admin UI ----------
        public Task DepositToAdvertiserAsync(string adminUserId, string advertiserId, decimal amount, string description)
        {
            if (amount <= 0) throw new ArgumentException("Deposit must be positive", nameof(amount));
            return ManualAdjustAsync(adminUserId, advertiserId, amount, description);
        }

        // ---------- New: Publisher withdrawal (automatic) ----------
        public async Task RequestWithdrawalAsync(string publisherUserId, decimal amount, string? payoutDetails = null)
        {
            if (string.IsNullOrEmpty(publisherUserId)) throw new ArgumentNullException(nameof(publisherUserId));
            if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

            var publisher = await _db.Users.FirstOrDefaultAsync(u => u.Id == publisherUserId);
            if (publisher == null) throw new InvalidOperationException("Publisher not found");

            if (publisher.Balance < amount) throw new InvalidOperationException("Insufficient balance for withdrawal");

            // Deduct immediately (automatic processing)
            publisher.Balance -= amount;

            _db.WalletTransactions.Add(new WalletTransaction
            {
                FromUserId = publisherUserId,
                ToUserId = null, // payout to external system
                Amount = amount,
                Type = TransactionType.Withdrawal,
                Description = $"Automatic payout processed. Details: {payoutDetails ?? "N/A"}"
            });

            await _db.SaveChangesAsync();

            // NOTE: Integrate real payment provider here if required.
        }

        // ---------- New: Query transactions ----------
        public async Task<List<WalletTransaction>> GetTransactionsForUserAsync(string userId, int take = 200)
        {
            return await _db.WalletTransactions
                .Where(t => t.FromUserId == userId || t.ToUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
        // ✅ NEW: Check if Advertiser has enough balance for a specific ad
        public async Task<bool> HasEnoughBalanceForAdAsync(int adId)
        {
            var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Id == adId);
            if (ad == null) return false;

            var advertiser = await _db.Users.FirstOrDefaultAsync(u => u.Id == ad.AdvertiserId);
            if (advertiser == null) return false;

            // Assume cheapest event is impression price
            var rule = await _pricingService.GetEffectivePricingAsync(ad.AdvertiserId, null, null);
            var needed = Math.Round(rule.CPM / 1000m, 6);

            return advertiser.Balance >= needed;
        }

        // ✅ NEW: General user balance check
        public async Task<bool> HasEnoughBalanceAsync(string userId, decimal required)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            return user.Balance >= required;
        }

    }
}
