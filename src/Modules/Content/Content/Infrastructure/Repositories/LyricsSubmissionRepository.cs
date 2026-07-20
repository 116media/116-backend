using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ILyricsSubmissionRepository" /> for managing community lyrics
/// submission entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LyricsSubmissionRepository(ContentDbContext context) : ILyricsSubmissionRepository
{
    /// <inheritdoc />
    public async Task<LyricsSubmissionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new SubmissionByIdSpecification(id: id);
        return await context
            .LyricsSubmissions.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsSubmissionEntity> GetByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new SubmissionByIdSpecification(id: id);
        return await context
            .LyricsSubmissions.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<LyricsSubmissionEntity> Submissions, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumSubmissionStatus? status,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<LyricsSubmissionEntity> query = context.LyricsSubmissions;

        if (status.HasValue)
        {
            var specification = new SubmissionByStatusSpecification(status: status.Value);
            query = query.ApplySpecification(specification: specification);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<LyricsSubmissionEntity> submissions = await query
            .OrderByDescending(submission => submission.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (submissions, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LyricsSubmissionEntity>> GetPendingWithMatchingLyricsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await (
            from submission in context.LyricsSubmissions
            where submission.Status == EnumSubmissionStatus.Pending
            where
                context.Lyrics.Any(lyrics =>
                    lyrics.SongTitle == submission.SongTitle && lyrics.ArtistName == submission.ArtistName
                )
            select submission
        ).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(LyricsSubmissionEntity submission, CancellationToken cancellationToken = default)
    {
        await context.LyricsSubmissions.AddAsync(submission, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(LyricsSubmissionEntity submission)
    {
        context.LyricsSubmissions.Update(submission);
    }
}
