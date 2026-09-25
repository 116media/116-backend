using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Entities;
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
    public async Task<AuthorDto?> GetAuthorInfoByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
        {
            return null;
        }

        return new AuthorDto(
            user.UserName,
            user.Email?.Value,
            user.AvatarFileId,
            user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault(),
            user.PreferredLocale
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, AuthorDto>> GetAuthorInfosByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct = default
    )
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, AuthorDto>();
        }

        Guid[] distinctIds = userIds.Distinct().ToArray();

        List<UserEntity> users = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => distinctIds.Contains(u.Id))
            .ToListAsync(ct);

        return users.ToDictionary(
            user => user.Id,
            user => new AuthorDto(
                user.UserName,
                user.Email?.Value,
                user.AvatarFileId,
                user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault(),
                user.PreferredLocale
            )
        );
    }
}
