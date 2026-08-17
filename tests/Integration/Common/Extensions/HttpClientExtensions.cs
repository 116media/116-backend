using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;
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
        client.AuthenticateAs(User.SuperAdminId, nameof(EnumCoreUserRole.SuperAdmin));
    }

    /// <summary>
    /// Authenticates the client as an Admin user.
    /// </summary>
    public static void AuthenticateAsAdmin(this HttpClient client)
    {
        client.AuthenticateAs(User.AdminId, nameof(EnumCoreUserRole.Admin));
    }

    /// <summary>
    /// Authenticates the client as a Visitor user.
    /// </summary>
    public static void AuthenticateAsVisitor(this HttpClient client)
    {
        client.AuthenticateAs(User.VisitorId, nameof(EnumCoreUserRole.Visitor));
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
    /// Authenticates the client with a correctly signed token whose security markers diverge
    /// from the well-known state the fixture seeds.
    /// </summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="userId">The user identifier to put in the token.</param>
    /// <param name="role">The role to put in the token.</param>
    /// <param name="securityStamp">The stamp to emit; the well-known one when omitted.</param>
    /// <param name="tokenVersion">The version to emit; zero when omitted.</param>
    public static void AuthenticateWithSecurityMarkers(
        this HttpClient client,
        Guid userId,
        string role,
        Guid? securityStamp = null,
        long tokenVersion = 0
    )
    {
        string token = GenerateToken(userId, role, securityStamp: securityStamp, tokenVersion: tokenVersion);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Authenticates the client with a correctly signed token whose account-status claims carry
    /// the given values.
    /// </summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="userId">The user identifier to put in the token.</param>
    /// <param name="role">The role to put in the token.</param>
    /// <param name="isActive">The <c>is_active</c> claim value.</param>
    /// <param name="isVerified">The <c>is_verified</c> claim value.</param>
    public static void AuthenticateWithAccountFlags(
        this HttpClient client,
        Guid userId,
        string role,
        bool isActive,
        bool isVerified
    )
    {
        string token = GenerateToken(userId, role, isActive: isActive, isVerified: isVerified);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Authenticates the client with a correctly signed token that carries no security markers,
    /// reproducing a credential minted before token invalidation shipped.
    /// </summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="userId">The user identifier to put in the token.</param>
    /// <param name="role">The role to put in the token.</param>
    public static void AuthenticateWithoutSecurityMarkers(this HttpClient client, Guid userId, string role)
    {
        string token = GenerateToken(userId, role, includeSecurityMarkers: false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
    /// <param name="securityStamp">The security stamp to emit; the well-known one when omitted.</param>
    /// <param name="tokenVersion">The token version to emit; zero — the seeded value — when omitted.</param>
    /// <param name="includeSecurityMarkers">Whether to emit the security stamp and version claims at all.</param>
    /// <param name="isActive">The <c>is_active</c> claim value.</param>
    /// <param name="isVerified">The <c>is_verified</c> claim value.</param>
    private static string GenerateToken(
        Guid userId,
        string role,
        Guid? sessionId = null,
        bool includeSessionId = true,
        string? malformedUserId = null,
        Guid? securityStamp = null,
        long tokenVersion = 0,
        bool includeSecurityMarkers = true,
        bool isActive = true,
        bool isVerified = true
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
            new(JwtClaimsConstants.IsVerified, isVerified ? "true" : "false", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.IsActive, isActive ? "true" : "false", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.AuthProvider, "Email"),
        };

        if (includeSessionId)
        {
            claims.Add(new Claim(JwtClaimsConstants.SessionId, (sessionId ?? Guid.NewGuid()).ToString()));
        }

        // The markers must agree with the fixture-seeded token-state row, or the request is rejected.
        if (includeSecurityMarkers)
        {
            Guid stamp = securityStamp ?? Jwt.WellKnownSecurityStamp;
            claims.Add(new Claim(JwtClaimsConstants.SecurityStamp, stamp.ToString()));
            claims.Add(new Claim(JwtClaimsConstants.TokenVersion, $"{tokenVersion}", ClaimValueTypes.Integer64));
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
