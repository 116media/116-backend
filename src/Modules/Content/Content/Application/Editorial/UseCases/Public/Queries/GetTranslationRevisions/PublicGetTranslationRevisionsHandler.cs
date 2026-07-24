using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;

/// <summary>
/// Handles the <see cref="PublicGetTranslationRevisionsQuery" /> to list a translation's full
/// revision history.
/// </summary>
/// <param name="translationRepository">Repository for lyrics translation data access operations.</param>
/// <param name="revisionRepository">Repository for translation revision data access operations.</param>
public class PublicGetTranslationRevisionsHandler(
    ITranslationRepository translationRepository,
    ITranslationRevisionRepository revisionRepository
) : IQueryHandler<PublicGetTranslationRevisionsQuery, PublicGetTranslationRevisionsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetTranslationRevisionsResult> Handle(
        PublicGetTranslationRevisionsQuery query,
        CancellationToken cancellationToken
    )
    {
        await translationRepository.GetByIdOrThrowAsync(id: query.TranslationId, cancellationToken: cancellationToken);

        IReadOnlyList<LyricsTranslationRevisionEntity> revisions = await revisionRepository.GetAllByTranslationIdAsync(
            translationId: query.TranslationId,
            cancellationToken: cancellationToken
        );

        List<TranslationRevisionDto> dtos = revisions
            .Select(revision => new TranslationRevisionDto(
                Id: revision.Id,
                ProposedText: revision.ProposedText,
                EditSummary: revision.EditSummary,
                ProposedByUserId: revision.ProposedByUserId,
                Status: revision.Status.ToString(),
                DecidedByUserId: revision.DecidedByUserId
            ))
            .ToList();

        return new PublicGetTranslationRevisionsResult(Revisions: dtos);
    }
}
