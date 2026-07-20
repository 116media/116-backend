using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ITranslationRepository" /> for managing lyrics translation
/// entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class TranslationRepository(ContentDbContext context) : ITranslationRepository
{
    /// <inheritdoc />
    public async Task<LyricsTranslationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new TranslationByIdSpecification(id: id);
        return await context
            .LyricsTranslations.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsTranslationEntity> GetByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new TranslationByIdSpecification(id: id);
        return await context
            .LyricsTranslations.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsTranslationEntity?> GetByLyricsAndLanguageAsync(
        Guid lyricsId,
        string language,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new TranslationByLyricsAndLanguageSpecification(lyricsId: lyricsId, language: language);
        return await context
            .LyricsTranslations.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LyricsTranslationEntity>> GetAllByLyricsIdAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new TranslationByLyricsIdSpecification(lyricsId: lyricsId);
        return await context
            .LyricsTranslations.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(LyricsTranslationEntity translation, CancellationToken cancellationToken = default)
    {
        await context.LyricsTranslations.AddAsync(translation, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(LyricsTranslationEntity translation)
    {
        context.LyricsTranslations.Update(translation);
    }
}
