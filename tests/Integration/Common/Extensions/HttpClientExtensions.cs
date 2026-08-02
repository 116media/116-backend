using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Authenticates the client with a specific user ID, role, and session ID.
    /// Use this overload when the handler validates the session against the database.
    /// </summary>
    public static void AuthenticateAs(this HttpClient client, Guid userId, string role, Guid sessionId)
    {
        string token = GenerateToken(userId, role, sessionId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Authenticates the client with a correctly signed token that carries no session claim,
    /// reproducing a credential minted before sessions were bound into the token.
    /// </summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="userId">The user identifier to put in the token.</param>
    /// <param name="role">The role to put in the token.</param>
    public static void AuthenticateWithoutSessionClaim(this HttpClient client, Guid userId, string role)
    {
        string token = GenerateToken(userId, role, includeSessionId: false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Authenticates the client with a correctly signed token whose subject identifier is not a
    /// parsable UUID, reproducing a tampered or foreign-issued credential.
    /// </summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="role">The role to put in the token.</param>
    public static void AuthenticateWithMalformedUserId(this HttpClient client, string role)
    {
        string token = GenerateToken(Guid.Empty, role, malformedUserId: "not-a-uuid");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
    /// that the test server validates against. The claim set is complete by default;
    /// the optional parameters exist to mint the malformed shapes the endpoints
    /// must reject rather than trust.
    /// </summary>
    /// <param name="userId">The user identifier to put in the token.</param>
    /// <param name="role">The role to put in the token.</param>
    /// <param name="sessionId">The session identifier; a fresh one when omitted.</param>
    /// <param name="includeSessionId">Whether to emit the session claim at all.</param>
    /// <param name="malformedUserId">When set, replaces the subject identifier with this raw value.</param>
    private static string GenerateToken(
        Guid userId,
        string role,
        Guid? sessionId = null,
        bool includeSessionId = true,
        string? malformedUserId = null
    )
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.ValidSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        string subject = malformedUserId ?? userId.ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subject),
            new(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}"),
            new(ClaimTypes.Email, $"test-{role.ToLowerInvariant()}@116.com"),
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
            new(JwtClaimsConstants.IsVerified, "true", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.IsActive, "true", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.AuthProvider, "Email"),
        };

        if (includeSessionId)
        {
            claims.Add(new Claim(JwtClaimsConstants.SessionId, (sessionId ?? Guid.NewGuid()).ToString()));
        }

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
