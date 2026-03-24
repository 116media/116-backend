using System.Security.Claims;
using _116.Shared.Application.Services;
using Microsoft.AspNetCore.Http;

namespace _116.Shared.Infrastructure.Services;

/// <summary>
/// Resolves the current actor from the active HTTP context.
/// Returns <c>null</c> for <see cref="ICurrentActor.UserId"/> and <c>false</c> for
/// <see cref="ICurrentActor.HasHttpContext"/> when invoked outside an HTTP request
/// (background jobs, seeders, migrations).
/// </summary>
public class HttpCurrentActor(IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    /// <inheritdoc />
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc />
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public bool HasHttpContext => httpContextAccessor.HttpContext is not null;
}
