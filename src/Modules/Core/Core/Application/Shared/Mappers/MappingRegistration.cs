using Mapster;

namespace _116.Core.Application.Shared.Mappers;

/// <summary>
/// The Core module's Mapster registrations, contributed to the shared cross-module config
/// through <see cref="IRegister" />.
/// </summary>
public sealed class MappingRegistration : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        FileMapper.Register(config);
    }
}
