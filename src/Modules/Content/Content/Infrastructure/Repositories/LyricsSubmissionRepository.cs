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
public class LyricsSubmissionRepository(ContentDbContext context)
    : ContentRepository<LyricsSubmissionEntity>(context),
        ILyricsSubmissionRepository
{
    /// <inheritdoc />
    public async Task<(List<LyricsSubmissionEntity> Submissions, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumSubmissionStatus? status,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<LyricsSubmissionEntity> query = Context.LyricsSubmissions;

        if (status.HasValue)
        {
            query = query.Where(submission => submission.Status == status.Value);
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
            from submission in Context.LyricsSubmissions
            where submission.Status == EnumSubmissionStatus.Pending
            where
                Context.Lyrics.Any(lyrics =>
                    lyrics.SongTitle == submission.SongTitle && lyrics.ArtistName == submission.ArtistName
                )
            select submission
        ).ToListAsync(cancellationToken);
    }
}
