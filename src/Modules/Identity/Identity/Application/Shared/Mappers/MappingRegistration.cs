using Mapster;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// Centralized mapping configuration registration.
/// Creates and configures a TypeAdapterConfig instance without mutating global state.
/// </summary>
public static class MappingRegistration
{
    /// <summary>
    /// Creates and configures a new TypeAdapterConfig with all Identity module mappings.
    /// This config can be registered in DI and injected where needed.
    /// </summary>
    /// <returns>A fully configured TypeAdapterConfig instance.</returns>
    public static TypeAdapterConfig CreateConfiguration()
    {
        var config = new TypeAdapterConfig();

        // Register all mapper configurations
        UserMapper.Register(config);
        SessionMapper.Register(config);
        RoleMapper.Register(config);

        // Compile once for performance
        config.Compile();

        return config;
    }
}
