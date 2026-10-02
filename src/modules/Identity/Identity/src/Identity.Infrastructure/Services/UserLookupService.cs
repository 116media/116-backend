using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Resolves user display names and profiles from the Identity database.
/// Registered as a cross-module contract so other modules can
/// look up user info without a direct dependency on the
/// Identity domain or database context.
/// </summary>
public class UserLookupService(IdentityDbContext context) : IUserLookupService
{
    /// <inheritdoc />
    public async Task<string?> GetUserNameByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.Users.Where(u => u.Id == userId).Select(u => u.UserName).FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetUserProfileByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context
            .Users.Where(u => u.Id == userId)
            .Select(u => new UserProfileDto(
                u.UserName,
                u.Email!.Value,
                u.AvatarFileId,
                u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault(),
                u.PreferredLocale
            ))
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, UserProfileDto>> GetUserProfilesByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct = default
    )
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, UserProfileDto>();
        }

        Guid[] distinctIds = userIds.Distinct().ToArray();

        return await context
            .Users.Where(u => distinctIds.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                Profile = new UserProfileDto(
                    u.UserName,
                    u.Email!.Value,
                    u.AvatarFileId,
                    u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault(),
                    u.PreferredLocale
                ),
            })
            .ToDictionaryAsync(row => row.Id, row => row.Profile, ct);
    }
}
