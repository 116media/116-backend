using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Customer entity mappings.
/// </summary>
public static class CustomerMapper
{
    /// <summary>
    /// Registers Customer entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerEntity, CustomerDto>();
    }

    /// <summary>
    /// Maps a <see cref="CustomerEntity" /> to a <see cref="CustomerDto" />.
    /// </summary>
    public static CustomerDto ToCustomerDto(this CustomerEntity entity, IMapper mapper)
    {
        return mapper.Map<CustomerDto>(entity);
    }

    /// <summary>
    /// Maps a collection of <see cref="CustomerEntity" /> to a list of <see cref="CustomerDto" />.
    /// </summary>
    public static IReadOnlyList<CustomerDto> ToCustomerDtos(this IReadOnlyList<CustomerEntity> entities, IMapper mapper)
    {
        return mapper.Map<IReadOnlyList<CustomerDto>>(entities);
    }
}
