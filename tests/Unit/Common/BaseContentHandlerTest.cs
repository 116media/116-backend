using _116.Content.Application.Shared.Mappers;
using Mapster;
using MapsterMapper;

namespace _116.Unit.Tests.Common;

/// <summary>
/// Base class for Content module handler tests that provides a configured IMapper instance.
/// </summary>
public abstract class BaseContentHandlerTest
{
    /// <summary>
    /// Configured Mapster mapper instance using Content module mapping configuration.
    /// </summary>
    protected readonly IMapper Mapper;

    protected BaseContentHandlerTest()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        Mapper = new Mapper(config);
    }
}
