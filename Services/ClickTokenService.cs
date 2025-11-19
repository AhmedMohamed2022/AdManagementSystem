// Services/ClickTokenService.cs
using AdManagementSystem.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class ClickTokenService : IClickTokenService
{
    private readonly IConfiguration _config;
    private readonly byte[] _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _defaultTtl;

    public ClickTokenService(IConfiguration config)
    {
        _config = config;
        var secretString = _config["ClickToken:Secret"];
        if (string.IsNullOrEmpty(secretString))
            throw new InvalidOperationException("ClickToken:Secret must be set in configuration.");

        _secret = Encoding.UTF8.GetBytes(secretString);
        _issuer = _config["ClickToken:Issuer"] ?? "AdPlatform";
        _audience = _config["ClickToken:Audience"] ?? "AdClients";
        _defaultTtl = TimeSpan.FromMinutes(
            int.TryParse(_config["ClickToken:DefaultTtlMinutes"], out var m) ? m : 2
        );
    }

    public string GenerateToken(int adId, int websiteId, TimeSpan? ttl = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(_secret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString("N");
        var expires = DateTime.UtcNow.Add(ttl ?? _defaultTtl);

        var claims = new List<Claim>
        {
            new Claim("adId", adId.ToString()),
            new Claim("websiteId", websiteId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );

        return tokenHandler.WriteToken(token);
    }

    public ClickTokenPayload? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(_secret);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30) // small skew
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);
            var jwt = (JwtSecurityToken)validatedToken;

            var adIdStr = principal.FindFirst("adId")?.Value;
            var websiteIdStr = principal.FindFirst("websiteId")?.Value;
            var jti = jwt.Id ?? principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!int.TryParse(adIdStr, out var adId)) return null;
            if (!int.TryParse(websiteIdStr, out var websiteId)) return null;
            if (string.IsNullOrEmpty(jti)) return null;

            var expUnix = jwt.Payload.Exp;
            var exp = expUnix.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(expUnix.Value).UtcDateTime
                : DateTime.UtcNow.AddMinutes(2);

            return new ClickTokenPayload(adId, websiteId, jti, exp);
        }
        catch
        {
            return null;
        }
    }
}
