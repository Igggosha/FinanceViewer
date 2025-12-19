using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace FinanceViewer;

public class ApiKeyAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string HashApiKey(string apiKey)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(apiKey)));
    }
    
    private const string HeaderName = "X-API-KEY";
    private readonly FinDbContext _db;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        FinDbContext db)
        : base(options, logger, encoder, clock)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var apiKey))
            return AuthenticateResult.NoResult();

        var hashed = HashApiKey(apiKey!);

        var record = await _db.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k =>
                k.KeyHash == hashed &&
                !k.IsRevoked &&
                (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));

        if (record == null)
            return AuthenticateResult.Fail("Invalid API Key");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, record.User.Id),
            new Claim(ClaimTypes.Name, record.User.UserName!),
            new Claim("auth_type", "apikey")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}