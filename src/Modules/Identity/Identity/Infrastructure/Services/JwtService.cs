using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
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
        Guid sessionId,
        string email,
        string userName,
        ICollection<UserRoleEntity> userRoles,
        ICollection<RolePermissionEntity> userPermissions,
        bool isVerified,
        bool isActive,
        EnumAuthProvider authProvider
    )
    {
        var (secret, issuer, audience, accessTokenExpiration, _) = AppEnvironment.Jwt();
        if (string.IsNullOrWhiteSpace(value: secret))
        {
            throw new InvalidOperationException("JWT_SECRET env variable is missing or empty.");
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s: secret));
        var credentials = new SigningCredentials(key: key, algorithm: SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(type: ClaimTypes.NameIdentifier, $"{userId}"),
            new(type: ClaimTypes.Name, value: userName),
            new(type: ClaimTypes.Email, value: email),
            new(type: JwtRegisteredClaimNames.Sub, $"{userId}"),
            new(type: JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}"),
            new(type: JwtRegisteredClaimNames.Iat, $"{now.ToUnixTimeSeconds()}", valueType: ClaimValueTypes.Integer64),
            new(type: JwtClaimsConstants.AuthProvider, $"{authProvider}"),
            new(type: JwtClaimsConstants.SessionId, $"{sessionId}"),
        };

        claims.AddRange(BuildAccountStatusClaims(isVerified: isVerified, isActive: isActive));
        claims.AddRange(BuildRoleClaims(userRoles: userRoles));
        claims.AddRange(BuildPermissionsClaims(permissions: userPermissions));

        int expirationMinutes = int.TryParse(s: accessTokenExpiration, out int parsed)
            ? parsed
            : JwtClaimsConstants.DefaultExpiration;
        DateTime expiresAt = now.AddMinutes(minutes: expirationMinutes).UtcDateTime;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims: claims),
            Expires = expiresAt,
            SigningCredentials = credentials,
            Issuer = issuer,
            Audience = audience,
        };

        var handler = new JwtSecurityTokenHandler();
        string? token = handler.WriteToken(handler.CreateToken(tokenDescriptor: descriptor));

        return new JwtGenerationResult(Token: token, ExpiresAt: expiresAt);
    }

    /// <summary>
    /// Builds account status claims for the JWT token.
    /// </summary>
    private static List<Claim> BuildAccountStatusClaims(bool isVerified, bool isActive)
    {
        return new Dictionary<string, bool>
        {
            [key: JwtClaimsConstants.IsVerified] = isVerified,
            [key: JwtClaimsConstants.IsActive] = isActive,
        }
            .Select(kvp => new Claim(type: kvp.Key, kvp.Value ? "true" : "false", valueType: ClaimValueTypes.Boolean))
            .ToList();
    }

    /// <summary>
    /// Builds role claims from the user's assigned roles.
    /// </summary>
    private static List<Claim> BuildRoleClaims(ICollection<UserRoleEntity> userRoles)
    {
        return userRoles.Select(r => new Claim(type: ClaimTypes.Role, value: r.Role.Name)).ToList();
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
                    type: JwtClaimsConstants.Permissions,
                    JsonSerializer.Serialize(value: permissionsList),
                    valueType: JsonClaimValueTypes.JsonArray
                )
            );
        }

        return permissionClaims;
    }
}
