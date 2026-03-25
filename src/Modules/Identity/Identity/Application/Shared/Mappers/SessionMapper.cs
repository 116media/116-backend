using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Session entity mappings.
/// Uses dependency injection instead of global static state.
/// </summary>
public static class SessionMapper
{
    /// <summary>
    /// Registers Session entity mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<SessionEntity, SessionDto>()
            .Map(dest => dest.IsActive, src => !src.IsRevoked && src.ExpiresAt > DateTime.UtcNow);

        config
            .NewConfig<SessionEntity, SessionExportDto>()
            .Map(dest => dest.IsActive, src => !src.IsRevoked && src.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Maps a SessionEntity to a SessionDto for display to users.
    /// </summary>
    /// <param name="session">The session entity to map.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>A SessionDto containing session information.</returns>
    public static SessionDto ToSessionDto(this SessionEntity session, IMapper mapper)
    {
        var dto = mapper.Map<SessionDto>(session);
        return dto with { IsActive = session.IsActive() };
    }

    /// <summary>
    /// Maps a collection of SessionEntity to a list of SessionExportDto for data export.
    /// </summary>
    /// <param name="sessions">The session entities to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <returns>A list of SessionExportDto containing session export data.</returns>
    public static List<SessionExportDto> ToSessionExportDtos(this List<SessionEntity> sessions, IMapper mapper)
    {
        return sessions
            .Select(s =>
            {
                var dto = mapper.Map<SessionExportDto>(s);
                return dto with { IsActive = s.IsActive() };
            })
            .ToList();
    }
}
