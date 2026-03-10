using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Handles the <see cref="GetPackageByIdQuery" /> to retrieve a package by its identifier.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetPackageByIdHandler(IPackageRepository packageRepository, IMapper mapper)
    : IQueryHandler<GetPackageByIdQuery, GetPackageByIdResult>
{
    /// <inheritdoc />
    public async Task<GetPackageByIdResult> Handle(GetPackageByIdQuery query, CancellationToken cancellationToken)
    {
        PackageEntity package = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = package.ToPackageDto(mapper);
        return new GetPackageByIdResult(Package: dto);
    }
}
