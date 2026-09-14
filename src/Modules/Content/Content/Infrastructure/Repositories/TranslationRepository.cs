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
public class TranslationRepository(ContentDbContext context)
    : ContentRepository<LyricsTranslationEntity>(context),
        ITranslationRepository
{
    /// <inheritdoc />
    public async Task<LyricsTranslationEntity?> GetByLyricsAndLanguageAsync(
        Guid lyricsId,
        string language,
        CancellationToken cancellationToken = default
    )
    {
        return await Context.LyricsTranslations.FirstOrDefaultAsync(
            translation => translation.LyricsId == lyricsId && translation.Language == language,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LyricsTranslationEntity>> GetAllByLyricsIdAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .LyricsTranslations.Where(translation => translation.LyricsId == lyricsId)
            .ToListAsync(cancellationToken);
    }
}
