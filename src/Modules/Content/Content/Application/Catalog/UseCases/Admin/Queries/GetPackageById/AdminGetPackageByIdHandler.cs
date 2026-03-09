using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Handles the <see cref="AdminGetPackageByIdQuery" /> to retrieve a package by its identifier.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetPackageByIdHandler(IPackageRepository packageRepository, IMapper mapper)
    : IQueryHandler<AdminGetPackageByIdQuery, AdminGetPackageByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetPackageByIdResult> Handle(
        AdminGetPackageByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(query.Id);

        PackageEntity package = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = package.ToPackageDto(mapper);
        return new AdminGetPackageByIdResult(Package: dto);
    }
}
