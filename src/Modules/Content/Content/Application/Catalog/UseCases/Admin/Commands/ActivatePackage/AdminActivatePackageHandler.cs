using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Handles the <see cref="AdminActivatePackageCommand" /> to activate a package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="packageDtoFactory">Builds package projections with their categories resolved.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminActivatePackageHandler(
    IPackageRepository packageRepository,
    IContentUnitOfWork unitOfWork,
    IPackageDtoFactory packageDtoFactory,
    ContentI18n i18n
) : ICommandHandler<AdminActivatePackageCommand, AdminActivatePackageResult>
{
    /// <inheritdoc />
    public async Task<AdminActivatePackageResult> Handle(
        AdminActivatePackageCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PackageEntity package = await packageRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool activated = package.Activate();

        if (!activated)
        {
            throw i18n.Package.AlreadyActive();
        }
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageEntity updated = await packageRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        PackageDto dto = await packageDtoFactory.CreateAsync(updated, cancellationToken);
        return new AdminActivatePackageResult(Package: dto);
    }
}
