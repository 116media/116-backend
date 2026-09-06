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
        // IsActive is owned by ToSessionDto/ToSessionExportDtos, which judge it through
        // SessionEntity.IsActive against the caller-supplied instant.
        config.NewConfig<SessionEntity, SessionDto>().Map(dest => dest.Client, src => src.Client.Value);
        config.NewConfig<SessionEntity, SessionExportDto>().Map(dest => dest.Client, src => src.Client.Value);
    }

    /// <summary>
    /// Maps a SessionEntity to a SessionDto for display to users.
    /// </summary>
    /// <param name="session">The session entity to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="now">The current UTC instant activity is judged at.</param>
    /// <param name="currentSessionId">The ID of the requesting session, used to flag the current session.</param>
    /// <returns>A SessionDto containing session information.</returns>
    public static SessionDto ToSessionDto(
        this SessionEntity session,
        IMapper mapper,
        DateTime now,
        Guid? currentSessionId = null
    )
    {
        var dto = mapper.Map<SessionDto>(session);
        return dto with
        {
            IsActive = session.IsActive(now: now),
            IsCurrent = currentSessionId.HasValue && session.Id == currentSessionId.Value,
        };
    }

    /// <summary>
    /// Maps a collection of SessionEntity to a list of SessionExportDto for data export.
    /// </summary>
    /// <param name="sessions">The session entities to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="now">The current UTC instant activity is judged at.</param>
    /// <returns>A list of SessionExportDto containing session export data.</returns>
    public static List<SessionExportDto> ToSessionExportDtos(
        this List<SessionEntity> sessions,
        IMapper mapper,
        DateTime now
    )
    {
        return sessions
            .Select(s =>
            {
                var dto = mapper.Map<SessionExportDto>(s);
                return dto with { IsActive = s.IsActive(now: now) };
            })
            .ToList();
    }
}
