using _116.Content.Application.Shared.Mappers;
using MapsterMapper;

namespace _116.Content.TestData.Factories.Helpers;

/// <summary>
/// Builds the Mapster mapper the Content suites assert against, from the module's own registration.
/// </summary>
public static class ContentMapperFactory
{
    /// <summary>
    /// Creates a mapper configured exactly as the module configures it at startup.
    /// </summary>
    /// <returns>A configured IMapper.</returns>
    public static IMapper Create() => new Mapper(MappingRegistration.CreateConfiguration());
}
