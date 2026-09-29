using Mapster;

namespace _116.Storage.Application.Shared.Mappers;

/// <summary>
/// The Storage module's Mapster registrations, contributed to the shared cross-module config
/// through <see cref="IRegister" />.
/// </summary>
public sealed class MappingRegistration : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        FileMapper.Register(config);
    }

    /// <summary>
    /// Creates and compiles a standalone config carrying only the Storage module's mappings.
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
