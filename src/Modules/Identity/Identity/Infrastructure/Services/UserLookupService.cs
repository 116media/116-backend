using _116.Identity.Contracts.Application;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Resolves user display names from the Identity database.
/// Registered as a cross-module contract so other modules can
/// look up user names without a direct dependency on the
/// Identity domain or database context.
/// </summary>
public class UserLookupService(IdentityDbContext context) : IUserLookupService
{
    /// <inheritdoc />
    public async Task<string?> GetUserNameByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.Users.Where(u => u.Id == userId).Select(u => u.UserName).FirstOrDefaultAsync(ct);
    }
}
