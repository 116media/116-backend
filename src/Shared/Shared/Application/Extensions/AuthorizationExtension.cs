using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for attaching authorization policies to endpoint convention builders.
/// </summary>
public static class AuthorizationExtension
{
    /// <summary>
    /// Attaches a named authorization policy to an endpoint without allocating a params string array.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="policy">The authorization policy name.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder WithAuthorization<TBuilder>(this TBuilder builder, string policy)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(new AuthorizeAttribute(policy));
    }
}
