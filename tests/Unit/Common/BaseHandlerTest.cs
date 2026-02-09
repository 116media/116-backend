using _116.Identity.Application.Shared.Mappers;
using Mapster;
using MapsterMapper;

namespace _116.Unit.Tests.Common;

/// <summary>
/// Base class for handler tests that provides a configured IMapper instance.
/// </summary>
public abstract class BaseHandlerTest
{
    /// <summary>
    /// Configured Mapster mapper instance for use in tests.
    /// </summary>
    protected readonly IMapper Mapper;

    protected BaseHandlerTest()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        Mapper = new Mapper(config);
    }
}
