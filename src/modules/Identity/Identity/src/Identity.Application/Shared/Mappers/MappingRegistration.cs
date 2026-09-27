using Mapster;

namespace _116.Identity.Application.Shared.Mappers;

/// <summary>
/// The Identity module's Mapster registrations, contributed to the shared cross-module config
/// through <see cref="IRegister" />.
/// </summary>
public sealed class MappingRegistration : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        UserMapper.Register(config);
        SessionMapper.Register(config);
        RoleMapper.Register(config);
    }

    /// <summary>
    /// Creates and compiles a standalone config carrying only the Identity module's mappings.
    /// </summary>
    /// <returns>A fully configured TypeAdapterConfig instance.</returns>
    public static TypeAdapterConfig CreateConfiguration()
    {
        var config = new TypeAdapterConfig();
        new MappingRegistration().Register(config);
        config.Compile();

        return config;
    }
}
