namespace AdManagementSystem.Services
{
    // Services/IClickTokenService.cs
    public interface IClickTokenService
    {
        string GenerateToken(int adId, int websiteId, TimeSpan? ttl = null);
        ClickTokenPayload? ValidateToken(string token);
    }

    public record ClickTokenPayload(int AdId, int WebsiteId, string Jti, DateTime ExpirationUtc);

}
