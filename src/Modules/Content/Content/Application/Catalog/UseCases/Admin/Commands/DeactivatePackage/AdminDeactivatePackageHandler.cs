using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Handles the <see cref="AdminDeactivatePackageCommand" /> to deactivate a package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminDeactivatePackageHandler(
    IPackageRepository packageRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminDeactivatePackageCommand, AdminDeactivatePackageResult>
{
    /// <inheritdoc />
    public async Task<AdminDeactivatePackageResult> Handle(
        AdminDeactivatePackageCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PackageEntity package = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool deactivated = package.Deactivate();

        if (!deactivated)
        {
            throw PackageErrors.AlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageEntity updated = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToPackageDto(mapper);
        return new AdminDeactivatePackageResult(Package: dto);
    }
}
