using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Handles the <see cref="ActivatePackageCommand" /> to activate a package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class ActivatePackageHandler(IPackageRepository packageRepository, IContentUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<ActivatePackageCommand, ActivatePackageResult>
{
    /// <inheritdoc />
    public async Task<ActivatePackageResult> Handle(ActivatePackageCommand command, CancellationToken cancellationToken)
    {
        PackageEntity package = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool activated = package.Activate();

        if (!activated)
        {
            throw PackageErrors.AlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageEntity updated = await packageRepository.GetByIdWithSlotsOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToPackageDto(mapper);
        return new ActivatePackageResult(Package: dto);
    }
}
