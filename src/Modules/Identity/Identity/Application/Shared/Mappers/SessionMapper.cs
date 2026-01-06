using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using Mapster;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Session entity mappings.
/// </summary>
public static class SessionMapper
{
    /// <summary>
    /// Configures Mapster mappings for SessionEntity.
    /// </summary>
    public static void Configure()
    {
        TypeAdapterConfig<SessionEntity, SessionDto>
            .NewConfig()
            .Map(dest => dest.IsActive, src => !src.IsDeleted && src.ExpiresAt > DateTime.UtcNow)
            .Compile();
    }

    /// <summary>
    /// Maps a SessionEntity to a SessionDto for display to users.
    /// </summary>
    /// <param name="session">The session entity to map.</param>
    /// <returns>A SessionDto containing session information.</returns>
    public static SessionDto ToSessionDto(this SessionEntity session)
    {
        return session.Adapt<SessionDto>();
    }
}
