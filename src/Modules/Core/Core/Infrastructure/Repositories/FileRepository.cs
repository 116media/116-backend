using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Enums;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Core.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IFileRepository" /> using Entity Framework Core. Uploads live in
/// <see cref="Application.Shared.Services.IFileUploadService" />.
/// </summary>
/// <param name="context">The core database context.</param>
/// <param name="timeProvider">The clock the lifecycle stamps are read from.</param>
public class FileRepository(CoreDbContext context, TimeProvider timeProvider)
    : CoreRepository<FileEntity>(context),
        IFileRepository
{
    /// <inheritdoc />
    public override async Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var specification = new FileByIdNotDeletedSpecification(fileId);

        return await Context.Files.FirstOrDefaultBySpecificationAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, FileEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        if (fileIds.Count == 0)
        {
            return new Dictionary<Guid, FileEntity>();
        }

        return await Context
            .Files.Where(file =>
                fileIds.Contains(file.Id) && file.State != EnumFileState.Deleted && file.State != EnumFileState.Replaced
            )
            .ToDictionaryAsync(file => file.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FileEntity?> GetAvatarFileAsync(Guid? avatarFileId, CancellationToken cancellationToken = default)
    {
        return avatarFileId.HasValue ? await GetByIdAsync(avatarFileId.Value, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        FileEntity? file = await GetByIdAsync(fileId, cancellationToken);

        if (file is null || !file.Delete(timeProvider.GetUtcNow().UtcDateTime))
        {
            return false;
        }

        return true;
    }
}
