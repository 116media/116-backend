using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _116.Core.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of <see cref="IFileRepository"/> using Entity Framework Core.
/// </summary>
public class FileRepository(CoreDbContext context) : IFileRepository
{
    /// <inheritdoc />
    public async Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await context.Files
            .Where(f => !f.IsDeleted)
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        await context.Files.AddAsync(file, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        context.Files.Update(file);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
