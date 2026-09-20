using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ITranslationRevisionRepository" /> for managing translation
/// revision entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class TranslationRevisionRepository(ContentDbContext context)
    : ContentRepository<LyricsTranslationRevisionEntity>(context),
        ITranslationRevisionRepository
{
    /// <inheritdoc />
    public override async Task<LyricsTranslationRevisionEntity> GetByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new TranslationRevisionByIdSpecification(id: id);
        return await Context
            .LyricsTranslationRevisions.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LyricsTranslationRevisionEntity>> GetAllByTranslationIdAsync(
        Guid translationId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new TranslationRevisionByTranslationIdSpecification(translationId: translationId);
        return await Context
            .LyricsTranslationRevisions.ApplySpecification(specification: specification)
            .OrderByDescending(revision => revision.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LyricsTranslationRevisionEntity>> GetAcceptedButUnappliedAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await (
            from revision in Context.LyricsTranslationRevisions
            join translation in Context.LyricsTranslations on revision.TranslationId equals translation.Id
            where revision.Status == EnumRevisionStatus.Accepted && revision.ProposedText != translation.Text
            select revision
        ).ToListAsync(cancellationToken);
    }
}
