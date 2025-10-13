using _116.Shared.Infrastructure.Extensions;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;

namespace _116.Core.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IFileRepository"/> using Entity Framework Core.
/// </summary>
public class FileRepository(CoreDbContext context) : IFileRepository
{
    /// <inheritdoc />
    public async Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var specification = new FileByIdNotDeletedSpecification(fileId);

        return await context.Files
            .FirstOrDefaultBySpecificationAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        await context.Files.AddAsync(file, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        context.Files.Update(file);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Remove(FileEntity file)
    {
        context.Files.Remove(file);
    }

    /// <inheritdoc />
    public async Task<FileEntity?> GetAvatarFileAsync(
        Guid? avatarFileId,
        CancellationToken cancellationToken = default
    )
    {
        return avatarFileId.HasValue
            ? await GetByIdAsync(avatarFileId.Value, cancellationToken)
            : null;
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
