using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using _116.BuildingBlocks.Constants;
using _116.Shared.Application.Configurations;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Enums;
using _116.Auth.Domain.Results;
using Microsoft.IdentityModel.Tokens;

namespace _116.Auth.Infrastructure.Services;

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
        bool isLoggedIn,
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

        claims.AddRange(BuildRoleClaims(userRoles));
        claims.AddRange(BuildPermissionsClaims(userPermissions));
        claims.AddRange(BuildStatusClaims(isVerified, isActive, isLoggedIn));

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
    /// <param name="isVerified">Whether the user's account is verified</param>
    /// <param name="isActive">Whether the user's account is active</param>
    /// <param name="isLoggedIn">Whether the user is currently logged in</param>
    /// <returns>Collection of status claims</returns>
    private static List<Claim> BuildStatusClaims(bool isVerified, bool isActive, bool isLoggedIn)
    {
        return new Dictionary<string, bool>
        {
            [JwtClaimsConstants.IsVerified] = isVerified,
            [JwtClaimsConstants.IsActive] = isActive,
            [JwtClaimsConstants.IsLoggedIn] = isLoggedIn
        }.Select(kvp => new Claim(kvp.Key, kvp.Value ? "true" : "false", ClaimValueTypes.Boolean)).ToList();
    }

    /// <summary>
    /// Builds role claims from the user's assigned roles.
    /// </summary>
    /// <param name="userRoles">Collection of user role entities</param>
    /// <returns>Collection of role claims</returns>
    private static List<Claim> BuildRoleClaims(ICollection<UserRoleEntity> userRoles)
    {
        return userRoles.Select(r => new Claim(ClaimTypes.Role, r.Role.Name)).ToList();
    }

    /// <summary>
    /// Builds permission claims from the user's assigned permissions as a JSON array.
    /// </summary>
    /// <param name="permissions">Collection of role permission entities</param>
    /// <returns>Collection of permission claims in the format "resource:action"</returns>
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
