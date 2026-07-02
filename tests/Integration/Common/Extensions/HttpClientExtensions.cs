using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _116.BuildingBlocks.Constants;
using Microsoft.IdentityModel.Tokens;

namespace _116.Integration.Tests.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpClient" /> that attach JWT Bearer tokens
/// for integration test authentication. All methods are synchronous — no network calls.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Authenticates the client as a SuperAdmin user.
    /// </summary>
    public static void AuthenticateAsSuperAdmin(this HttpClient client)
    {
        client.AuthenticateAs(User.SuperAdminId, "SuperAdmin");
    }

    /// <summary>
    /// Authenticates the client as an Admin user.
    /// </summary>
    public static void AuthenticateAsAdmin(this HttpClient client)
    {
        client.AuthenticateAs(User.AdminId, "Admin");
    }

    /// <summary>
    /// Authenticates the client as a Visitor user.
    /// </summary>
    public static void AuthenticateAsVisitor(this HttpClient client)
    {
        client.AuthenticateAs(User.VisitorId, "Visitor");
    }

    /// <summary>
    /// Authenticates the client with a specific user ID and role.
    /// </summary>
    public static void AuthenticateAs(this HttpClient client, Guid userId, string role)
    {
        string token = GenerateToken(userId, role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    /// <summary>
    /// Authenticates the client with a specific user ID, role, and session ID.
    /// Use this overload when the handler validates the session against the database.
    /// </summary>
    public static void AuthenticateAs(this HttpClient client, Guid userId, string role, Guid sessionId)
    {
        string token = GenerateToken(userId, role, sessionId);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    /// <summary>
    /// Removes any authentication header from the client.
    /// </summary>
    public static void ClearAuthentication(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Mints a JWT token in-memory using the same secret, issuer, and audience
    /// that the test server validates against.
    /// </summary>
    private static string GenerateToken(Guid userId, string role, Guid? sessionId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.ValidSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}"),
            new(ClaimTypes.Email, $"test-{role.ToLowerInvariant()}@116.com"),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
            new(JwtClaimsConstants.IsVerified, "true", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.IsActive, "true", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.SessionId, (sessionId ?? Guid.NewGuid()).ToString()),
            new(JwtClaimsConstants.AuthProvider, "Email"),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credentials,
            Issuer = Jwt.ValidIssuer,
            Audience = Jwt.ValidAudience,
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
