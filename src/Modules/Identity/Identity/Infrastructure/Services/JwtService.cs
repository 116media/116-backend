using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Shared.Application.Configurations;

using Microsoft.IdentityModel.Tokens;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Service responsible for generating JWT tokens with user claims, roles, and permissions.
/// </summary>
public class JwtService : IJwtService
{
    /// <inheritdoc />
    public JwtGenerationResult GenerateToken(
        Guid userId,
        string email,
        string userName,
        ICollection<UserRoleEntity> userRoles,
        ICollection<RolePermissionEntity> userPermissions,
        bool isVerified,
        bool isActive,
        EnumAuthProvider authProvider
    )
    {
        var (secret, issuer, audience, expiration) = AppEnvironment.Jwt();
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT_SECRET env variable is missing or empty.");
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"{userId}"),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Sub, $"{userId}"),
            new(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}"),
            new(JwtRegisteredClaimNames.Iat, $"{now.ToUnixTimeSeconds()}", ClaimValueTypes.Integer64),
            new(JwtClaimsConstants.AuthProvider, $"{authProvider}")
        };

        claims.AddRange(BuildAccountStatusClaims(isVerified, isActive));
        claims.AddRange(BuildRoleClaims(userRoles));
        claims.AddRange(BuildPermissionsClaims(userPermissions));

        int expirationHours = int.TryParse(expiration, out int parsed)
            ? parsed
            : JwtClaimsConstants.DefaultExpiration;
        DateTime expiresAt = now.AddHours(expirationHours).UtcDateTime;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            SigningCredentials = credentials,
            Issuer = issuer,
            Audience = audience
        };

        var handler = new JwtSecurityTokenHandler();
        string? token = handler.WriteToken(handler.CreateToken(descriptor));

        return new JwtGenerationResult(token, expiresAt);
    }

    /// <summary>
    /// Builds account status claims for the JWT token.
    /// </summary>
    private static List<Claim> BuildAccountStatusClaims(bool isVerified, bool isActive)
    {
        return new Dictionary<string, bool>
        {
            [JwtClaimsConstants.IsVerified] = isVerified,
            [JwtClaimsConstants.IsActive] = isActive
        }.Select(kvp => new Claim(kvp.Key, kvp.Value ? "true" : "false", ClaimValueTypes.Boolean)).ToList();
    }

    /// <summary>
    /// Builds role claims from the user's assigned roles.
    /// </summary>
    private static List<Claim> BuildRoleClaims(ICollection<UserRoleEntity> userRoles)
    {
        return userRoles.Select(r => new Claim(ClaimTypes.Role, r.Role.Name)).ToList();
    }

    /// <summary>
    /// Builds permission claims from the user's assigned permissions as a JSON array.
    /// </summary>
    private static List<Claim> BuildPermissionsClaims(ICollection<RolePermissionEntity> permissions)
    {
        string[] permissionsList = permissions
            .Select(p => $"{p.Permission.Resource}:{p.Permission.Action}")
            .Distinct()
            .ToArray();

        var permissionClaims = new List<Claim>();

        // Add permissions as JSON array for frontend consumption
        if (permissionsList.Length > 0)
        {
            permissionClaims.Add(
                new Claim(
                    JwtClaimsConstants.Permissions,
                    JsonSerializer.Serialize(permissionsList),
                    JsonClaimValueTypes.JsonArray
                )
            );
        }

        return permissionClaims;
    }
}
