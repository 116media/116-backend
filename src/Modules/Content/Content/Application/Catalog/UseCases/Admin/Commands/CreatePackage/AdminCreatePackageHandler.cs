using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Handles the <see cref="AdminCreatePackageCommand" /> to create a new content package.
/// </summary>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="packageDtoFactory">Builds package projections with their categories resolved.</param>
public class AdminCreatePackageHandler(
    IPackageRepository packageRepository,
    IContentUnitOfWork unitOfWork,
    IPackageDtoFactory packageDtoFactory
) : ICommandHandler<AdminCreatePackageCommand, AdminCreatePackageResult>
{
    /// <inheritdoc />
    public async Task<AdminCreatePackageResult> Handle(
        AdminCreatePackageCommand command,
        CancellationToken cancellationToken
    )
    {
        var package = PackageEntity.Create(id: Guid.NewGuid(), name: command.Name, description: command.Description);

        await packageRepository.AddAsync(package: package, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PackageEntity created = await packageRepository.GetByIdOrThrowAsync(
            id: package.Id,
            cancellationToken: cancellationToken
        );

        PackageDto dto = await packageDtoFactory.CreateAsync(created, cancellationToken);
        return new AdminCreatePackageResult(Package: dto);
    }
}
